using Microsoft.Playwright;

namespace train_automation;

public sealed class IrctcBookingService : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async Task BookTrainAsync(
        TrainSearchSettings searchSettings,
        TrainResult selectedTrain,
        BookingConfiguration config,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report("Starting visible browser for IRCTC...");
        _playwright ??= await Playwright.CreateAsync();
        
        // Launch in non-headless mode so the user can type CAPTCHA and do Payment
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false, 
            SlowMo = 50 // Slight delay to ensure Angular registers keystrokes
        });

        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 }
        });

        var page = await context.NewPageAsync();
        
        try
        {
            progress?.Report("Opening IRCTC...");
            await page.GotoAsync("https://www.irctc.co.in/nget/train-search", new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 90_000
            });

            // Dismiss the "Welcome / Select Language" popup — click the "English" button
            progress?.Report("Waiting for IRCTC to load...");
            await page.WaitForTimeoutAsync(3000); // Give Angular time to render

            try
            {
                // Use JavaScript to find and click the English button — most reliable approach
                var clicked = await page.EvaluateAsync<bool>("""
                    () => {
                        const buttons = document.querySelectorAll('button');
                        for (const btn of buttons) {
                            if (btn.textContent.trim() === 'English') {
                                btn.click();
                                return true;
                            }
                        }
                        return false;
                    }
                    """);

                if (clicked)
                {
                    progress?.Report("Selected English language.");
                    await page.WaitForTimeoutAsync(1500); // Let the popup close
                }
                else
                {
                    progress?.Report("No language popup found, continuing...");
                }
            }
            catch
            {
                // Ignore and continue
            }

            // 1. Search for Train
            progress?.Report("Filling train search details...");

            // From Station — type code char by char so Angular fires input events, then pick correct dropdown item
            var fromInput = page.Locator(".ui-autocomplete-input").First;
            await fromInput.ClickAsync();
            await fromInput.PressSequentiallyAsync(searchSettings.FromStationCode, new LocatorPressSequentiallyOptions { Delay = 150 });
            await page.WaitForTimeoutAsync(2000); // Wait for dropdown to fully appear

            // Find dropdown item that contains " - CODE" (e.g. " - NDLS") to avoid partial matches like BADLI matching DLI
            var fromOption = page.Locator("li").Filter(new LocatorFilterOptions { HasText = $"- {searchSettings.FromStationCode}" }).First;
            if (await fromOption.IsVisibleAsync())
            {
                await fromOption.ClickAsync();
                progress?.Report($"Selected From: {searchSettings.FromStationCode}");
            }
            else
            {
                // Fallback: press ArrowDown + Enter to pick first item
                await page.Keyboard.PressAsync("ArrowDown");
                await page.Keyboard.PressAsync("Enter");
            }
            await page.WaitForTimeoutAsync(500);

            // To Station — same approach
            var toInput = page.Locator(".ui-autocomplete-input").Nth(1);
            await toInput.ClickAsync();
            await toInput.PressSequentiallyAsync(searchSettings.ToStationCode, new LocatorPressSequentiallyOptions { Delay = 150 });
            await page.WaitForTimeoutAsync(2000);

            var toOption = page.Locator("li").Filter(new LocatorFilterOptions { HasText = $"- {searchSettings.ToStationCode}" }).First;
            if (await toOption.IsVisibleAsync())
            {
                await toOption.ClickAsync();
                progress?.Report($"Selected To: {searchSettings.ToStationCode}");
            }
            else
            {
                await page.Keyboard.PressAsync("ArrowDown");
                await page.Keyboard.PressAsync("Enter");
            }
            await page.WaitForTimeoutAsync(500);


            // Date
            var dateString = searchSettings.TravelDate.ToString("dd/MM/yyyy");
            var dateInput = page.Locator("p-calendar input");
            await dateInput.ClickAsync();
            await page.Keyboard.PressAsync("Control+A");
            await page.Keyboard.PressAsync("Backspace");
            await dateInput.PressSequentiallyAsync(dateString, new LocatorPressSequentiallyOptions { Delay = 100 });
            await page.Keyboard.PressAsync("Escape");
            await page.WaitForTimeoutAsync(500);

            // Click "Search Trains" button using JS
            progress?.Report("Clicking Search Trains...");
            await page.EvaluateAsync("() => { const buttons = document.querySelectorAll('button'); for (const btn of buttons) { if (btn.textContent.trim() === 'Search Trains') { btn.click(); return; } } }");

            await page.WaitForTimeoutAsync(2000); // Wait for results or dialog

            // Handle "No direct trains available" confirmation dialog if it appears
            try
            {
                var clickedNo = await page.EvaluateAsync<bool>("""
                    () => {
                        if (document.body.textContent.includes('No direct trains available')) {
                            const buttons = document.querySelectorAll('button');
                            for (const btn of buttons) {
                                if (btn.textContent.trim() === 'No' || btn.textContent.includes('No')) {
                                    btn.click();
                                    return true;
                                }
                            }
                        }
                        return false;
                    }
                    """);
                
                if (clickedNo)
                {
                    progress?.Report("No direct trains dialog detected — clicked No.");
                    await page.WaitForTimeoutAsync(1000);
                    progress?.Report("No direct trains found. Please choose a different route (e.g. NDLS to PNBE)!");
                    return; // Exit booking
                }
            }
            catch { /* No dialog — continue normally */ }

            // 2. Wait for Results and Click Book Now
            progress?.Report($"Waiting for train {selectedTrain.TrainNumber} to load...");
            
            try 
            {
                // Wait for the specific train number to appear on the screen (Wait up to 20 seconds)
                await page.Locator($"text={selectedTrain.TrainNumber}").First.WaitForAsync(new LocatorWaitForOptions { Timeout = 20000 });
                await page.WaitForTimeoutAsync(1500); // Give the DOM an extra 1.5 seconds to fully render the 'Refresh' buttons
            }
            catch (TimeoutException)
            {
                progress?.Report("Warning: Train search results took too long to load or train was not found on this date!");
            }

            // Find the train row, click "Refresh" on the first class, and click "Book Now" using JS
            var clickedBookNow = await page.EvaluateAsync<bool>("""
                async (trainNum) => {
                    // Find all possible train wrappers
                    const elements = document.querySelectorAll('app-train-avl-enq, div.form-group');
                    let targetRow = null;
                    
                    // Look for a container that has BOTH the train number and the word 'Refresh'
                    for (const el of elements) {
                        if (el.textContent.includes(trainNum) && el.textContent.includes('Refresh')) {
                            targetRow = el;
                            break;
                        }
                    }

                    if (!targetRow) {
                        // Fallback: search all divs
                        const allDivs = document.querySelectorAll('div');
                        for (const el of allDivs) {
                            if (el.textContent.includes(trainNum) && el.textContent.includes('Refresh')) {
                                targetRow = el; // This will overwrite with the innermost div containing both
                            }
                        }
                    }

                    if (!targetRow) return false;

                    // Click the first "Refresh" button in this row
                    const refreshBtns = targetRow.querySelectorAll('.prenext'); // IRCTC class for refresh
                    let refreshClicked = false;
                    for (const btn of refreshBtns) {
                        if (btn.textContent.includes('Refresh')) {
                            btn.click();
                            refreshClicked = true;
                            break;
                        }
                    }
                    
                    if (!refreshClicked) {
                        // Fallback: look for any div containing "Refresh"
                        const divs = targetRow.querySelectorAll('div, td, a, span');
                        for (const div of divs) {
                            if (div.textContent.trim().startsWith('Refresh')) {
                                div.click();
                                break;
                            }
                        }
                    }

                    return true;
                }
                """, selectedTrain.TrainNumber);

            if (clickedBookNow)
            {
                progress?.Report("Refreshing class availability...");
                await page.WaitForTimeoutAsync(2000); // Wait for Book Now button to appear
                
                // Now explicitely click the first availability box, then click Book Now
                await page.EvaluateAsync("""
                    (trainNum) => {
                        const elements = document.querySelectorAll('app-train-avl-enq, app-train-list div.form-group');
                        for (const el of elements) {
                            if (el.textContent.includes(trainNum)) {
                                
                                // 1. Click the availability box (td containing 'AVAILABLE', 'RAC', or 'WL')
                                const availBoxes = el.querySelectorAll('td div, td table');
                                for (const box of availBoxes) {
                                    if (box.textContent.includes('AVAILABLE') || box.textContent.includes('WL') || box.textContent.includes('RAC')) {
                                        box.click();
                                        break; // Only click the first one
                                    }
                                }

                                // 2. Click Book Now
                                const buttons = el.querySelectorAll('button');
                                for (const btn of buttons) {
                                    if (btn.textContent.includes('Book Now')) {
                                        btn.click();
                                        return;
                                    }
                                }
                            }
                        }
                    }
                    """, selectedTrain.TrainNumber);

                await page.WaitForTimeoutAsync(1500); // Wait for potential confirmation popup

                // Smart Popup Handler (e.g. Station mismatch: "You searched NDLS to PNBE but booking ANVT to DNR. Do you want to continue?")
                try
                {
                    var clickedContinue = await page.EvaluateAsync<bool>("""
                        () => {
                            if (document.body.textContent.includes('Do you want to continue') || document.body.textContent.includes('confirmation')) {
                                const buttons = document.querySelectorAll('button');
                                for (const btn of buttons) {
                                    if (btn.textContent.trim() === 'Yes' || btn.textContent.includes('Yes')) {
                                        btn.click();
                                        return true;
                                    }
                                }
                            }
                            return false;
                        }
                        """);
                    
                    if (clickedContinue)
                    {
                        progress?.Report("Confirmation dialog detected — clicked Yes automatically.");
                        await page.WaitForTimeoutAsync(1000);
                    }
                }
                catch { /* No dialog — continue normally */ }
            }
            else
            {
                progress?.Report("Could not find train automatically. Please select class manually.");
            }

            // 3. Handle Login Popup
            progress?.Report("Waiting for Login Popup...");
            
            // Wait for the VISIBLE username input in the login popup
            var userInput = page.Locator("input[formcontrolname='userid']:visible, input[placeholder='User Name']:visible").First;
            await userInput.WaitForAsync(new LocatorWaitForOptions { Timeout = 15_000 });
            
            if (string.IsNullOrEmpty(config.Credentials.Username) || string.IsNullOrEmpty(config.Credentials.Password))
            {
                progress?.Report("ERROR: Username or Password is empty! Please save them in Settings tab.");
                await Task.Delay(-1, cancellationToken);
                return;
            }

            progress?.Report($"Typing Username...");
            await userInput.ClickAsync();
            await userInput.PressSequentiallyAsync(config.Credentials.Username, new LocatorPressSequentiallyOptions { Delay = 60 });
            
            progress?.Report("Typing Password...");
            var passInput = page.Locator("input[formcontrolname='password']:visible, input[placeholder='Password']:visible").First;
            await passInput.ClickAsync();
            await passInput.PressSequentiallyAsync(config.Credentials.Password, new LocatorPressSequentiallyOptions { Delay = 60 });

            // Click "Visually Impaired" checkbox to get OTP instead of image CAPTCHA
            progress?.Report("Switching to OTP login (no CAPTCHA needed)...");
            try
            {
                var otpCheckbox = page.Locator("input[type='checkbox']").Filter(new LocatorFilterOptions { HasText = "" }).First;
                // Try to find the visually impaired checkbox specifically
                var viCheckbox = page.Locator("label:has-text('Visually impaired') input, input[id*='impaired'], input[id*='visual']").First;
                if (await viCheckbox.CountAsync() > 0 && await viCheckbox.IsVisibleAsync())
                {
                    await viCheckbox.CheckAsync();
                    progress?.Report("Checked OTP option!");
                }
            }
            catch { /* Checkbox may not be found - continue */ }

            // Click Sign In button
            progress?.Report("Clicking Sign In...");
            await page.WaitForTimeoutAsync(500);
            var signInBtn = page.Locator("button:has-text('SIGN IN'):visible, button:has-text('Sign In'):visible").First;
            await signInBtn.ClickAsync();

            // Wait - if CAPTCHA appears the user needs to solve it, otherwise OTP is sent
            progress?.Report("ACTION REQUIRED: If OTP box appears, enter the OTP from your phone. If CAPTCHA appears, solve it and click Sign In.");
            
            // Wait until we navigate away from the train-list page (login succeeded)
            await page.WaitForURLAsync(url => !url.Contains("train-list"), new PageWaitForURLOptions { Timeout = 300_000 });

            // 4. Fill Passenger Details
            progress?.Report("Login successful! Loading Passenger Details page...");
            
            // Wait for the passenger name input to be visible (more reliable than waiting for app-passenger)
            var firstNameInput = page.Locator("input[placeholder='Full Name as per Govt. ID']").First;
            await firstNameInput.WaitForAsync(new LocatorWaitForOptions { Timeout = 30_000 });
            await page.WaitForTimeoutAsync(1000); // Extra wait for Angular to bind

            progress?.Report("Auto-filling passengers...");
            int pIndex = 0;
            foreach (var pax in config.Passengers)
            {
                // If it's the second+ passenger, click Add Passenger first
                if (pIndex > 0)
                {
                    var addPaxBtn = page.Locator("a:has-text('Add Passenger'), button:has-text('Add Passenger'), span:has-text('+ Add Passenger')").First;
                    if (await addPaxBtn.IsVisibleAsync())
                    {
                        await addPaxBtn.ClickAsync();
                        await page.WaitForTimeoutAsync(800);
                    }
                }

                // Name field: placeholder is "Full Name as per Govt. ID"
                var nameInput = page.Locator("input[placeholder='Full Name as per Govt. ID']").Nth(pIndex);
                if (await nameInput.IsVisibleAsync())
                {
                    await nameInput.ClickAsync();
                    await nameInput.PressSequentiallyAsync(pax.Name, new LocatorPressSequentiallyOptions { Delay = 40 });
                }

                // Age field
                var ageInputs = page.Locator("input[placeholder='Age'], input[formcontrolname='passengerAge']");
                var ageInput = ageInputs.Nth(pIndex);
                if (await ageInput.IsVisibleAsync())
                {
                    await ageInput.ClickAsync();
                    await ageInput.PressSequentiallyAsync(pax.Age, new LocatorPressSequentiallyOptions { Delay = 40 });
                }

                // Gender dropdown
                var genderDropdown = page.Locator("select[formcontrolname='passengerGender'], select[aria-label*='Gender']").Nth(pIndex);
                if (await genderDropdown.IsVisibleAsync())
                {
                    string gCode = pax.Gender.StartsWith("M", StringComparison.OrdinalIgnoreCase) ? "M" 
                                 : pax.Gender.StartsWith("F", StringComparison.OrdinalIgnoreCase) ? "F" : "T";
                    await genderDropdown.SelectOptionAsync(new[] { gCode });
                }

                // Berth preference dropdown
                var berthDropdown = page.Locator("select[formcontrolname='passengerBerthChoice'], select[aria-label*='Berth']").Nth(pIndex);
                if (await berthDropdown.IsVisibleAsync() && pax.BerthPreference != "No Preference")
                {
                    string bCode = pax.BerthPreference switch
                    {
                        "Lower" => "LB",
                        "Middle" => "MB",
                        "Upper" => "UB",
                        "Side Lower" => "SL",
                        "Side Upper" => "SU",
                        "Window" => "WS",
                        _ => ""
                    };
                    if (!string.IsNullOrEmpty(bCode))
                    {
                        try { await berthDropdown.SelectOptionAsync(new[] { bCode }); } catch { }
                    }
                }

                pIndex++;
                await page.WaitForTimeoutAsync(300);
            }

            progress?.Report($"✅ Filled {pIndex} passenger(s)! Selecting payment method...");
            await page.WaitForTimeoutAsync(2000); // Let Angular validate the form

            // ── Step 5: Select Payment Mode BEFORE clicking Continue ───────────────
            // Payment mode is on the SAME /psgninput page as passenger details!
            try
            {
                // Scroll down to the payment section
                await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
                await page.WaitForTimeoutAsync(800);
                
                // Select BHIM/UPI radio button (₹10 fee, cheaper than ₹15)
                var upiLabel = page.Locator("label:has-text('BHIM/UPI')").First;
                if (await upiLabel.IsVisibleAsync())
                {
                    await upiLabel.ClickAsync();
                    progress?.Report("✅ Selected BHIM/UPI payment (₹10 fee).");
                    await page.WaitForTimeoutAsync(500);
                }
                else
                {
                    progress?.Report("BHIM/UPI label not visible — using default.");
                }
            }
            catch (Exception ex) 
            { 
                progress?.Report($"Payment selection note: {ex.Message}"); 
            }

            // ── Step 6: Click Continue (single click, wait for real navigation) ────
            progress?.Report("Clicking Continue...");
            var urlBeforePassenger = page.Url;
            var continueBtn = page.Locator("button:has-text('Continue'):visible").Last;
            if (await continueBtn.IsVisibleAsync())
            {
                await continueBtn.ClickAsync();
                progress?.Report("Clicked Continue — waiting for navigation...");
                try
                {
                    await page.WaitForURLAsync(url => url != urlBeforePassenger, new PageWaitForURLOptions { Timeout = 20_000 });
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    await page.WaitForTimeoutAsync(2000);
                }
                catch
                {
                    progress?.Report("Navigation took long — continuing anyway...");
                }
            }
            else
            {
                progress?.Report("ACTION REQUIRED: Could not find Continue. Please click it manually.");
                await page.WaitForURLAsync(url => url != urlBeforePassenger, new PageWaitForURLOptions { Timeout = 120_000 });
            }

            progress?.Report($"Now on: {page.Url}");

            // ── Step 7: Review Journey page — if present, click Continue ──────────
            if (page.Url.Contains("reviewJrny") || page.Url.Contains("review"))
            {
                progress?.Report("On Review page — clicking Continue...");
                await page.WaitForTimeoutAsync(1500);
                var urlBeforeReview = page.Url;
                var reviewBtn = page.Locator("button:has-text('Continue'):visible").Last;
                if (await reviewBtn.IsVisibleAsync())
                {
                    await reviewBtn.ClickAsync();
                    await page.WaitForURLAsync(url => url != urlBeforeReview, new PageWaitForURLOptions { Timeout = 20_000 });
                    await page.WaitForLoadStateAsync(LoadState.NetworkIdle);
                    await page.WaitForTimeoutAsync(2000);
                }
            }

            // ── Step 8: Final payment gateway / proceed to pay ────────────────────
            if (page.Url.Contains("payment") || page.Url.Contains("pay") || page.Url.Contains("bankpay"))
            {
                progress?.Report("On Payment Gateway — waiting for QR...");
            }
            else
            {
                // If there's still a Continue/Proceed button on an intermediate page, click it
                progress?.Report($"On: {page.Url} — looking for final Pay button...");
                await page.WaitForTimeoutAsync(1000);
                var urlBeforePay = page.Url;
                var payBtn = page.Locator("button:has-text('Continue'):visible, button:has-text('Proceed'):visible, button:has-text('PAY'):visible").Last;
                if (await payBtn.IsVisibleAsync())
                {
                    await payBtn.ClickAsync();
                    progress?.Report("Clicked final Pay button...");
                    await page.WaitForURLAsync(url => url != urlBeforePay, new PageWaitForURLOptions { Timeout = 15_000 });
                    await page.WaitForTimeoutAsync(2000);
                }
            }

            // ── Step 8: Wait for QR Code / Payment Gateway ────────────────────────
            progress?.Report("Waiting for payment QR code (up to 2 minutes)...");
            
            // Wait for QR image or UPI deep link to appear
            try
            {
                // Wait for page to navigate to payment gateway or show QR
                await page.WaitForTimeoutAsync(3000);
                
                // Take a screenshot of the QR / payment page
                var screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "payment_qr.png");
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = false });
                
                progress?.Report($"📸 QR screenshot saved to: {screenshotPath}");

                // Open the screenshot in default image viewer so user can scan it
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = screenshotPath,
                    UseShellExecute = true
                });
                
                progress?.Report("✅ QR Code opened! Scan it with your UPI app to pay. Waiting for payment confirmation...");
            }
            catch (Exception qrEx)
            {
                progress?.Report($"QR capture note: {qrEx.Message}. Please scan QR manually in browser.");
            }

            // ── Step 9: Wait for booking confirmation & save ticket ───────────────
            progress?.Report("Waiting for booking confirmation (you have 10 minutes to pay)...");
            try
            {
                // Wait for confirmation page — typically has "Booking Confirmed" or PNR number
                await page.WaitForURLAsync(url => url.Contains("bookingConfirm") || url.Contains("printTicket") || url.Contains("confirmation"), 
                    new PageWaitForURLOptions { Timeout = 600_000 }); // 10 minutes

                progress?.Report("🎉 BOOKING CONFIRMED! Saving ticket...");
                await page.WaitForTimeoutAsync(2000);

                var ticketPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ticket.png");
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = ticketPath, FullPage = true });
                
                progress?.Report($"🎟️ Ticket screenshot saved: {ticketPath}");
                
                // Open ticket image
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = ticketPath,
                    UseShellExecute = true
                });
                
                progress?.Report("✅ Done! Ticket opened. Check your IRCTC registered email/SMS for e-ticket.");
            }
            catch
            {
                progress?.Report("Payment pending or manual action needed. Browser stays open — complete payment there.");
            }
        }
        catch (Exception ex)
        {
            progress?.Report($"Automation stopped: {ex.Message}");
            // Leave browser open on failure so user can see what happened
            await Task.Delay(-1, cancellationToken);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;
    }
}
