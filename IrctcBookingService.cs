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
        progress?.Report(config.UseBetaView
            ? "Starting browser for IRCTC BETA site..."
            : "Starting visible browser for IRCTC (classic)...");
        _playwright ??= await Playwright.CreateAsync();

        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = false,
            SlowMo = 40
        });

        var context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            ViewportSize = new ViewportSize { Width = 1366, Height = 900 },
            Locale = "en-IN"
        });

        var page = await context.NewPageAsync();

        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (config.UseBetaView)
            {
                await BookTrainBetaAsync(page, searchSettings, selectedTrain, config, progress, cancellationToken);
            }
            else
            {
                await BookTrainClassicAsync(page, searchSettings, selectedTrain, config, progress, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            progress?.Report("Automation stopped by user.");
            throw;
        }
        catch (Exception ex)
        {
            progress?.Report($"Automation stopped: {ex.Message}");
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // expected on Stop
            }
        }
    }

    private static async Task BookTrainClassicAsync(
        IPage page,
        TrainSearchSettings searchSettings,
        TrainResult selectedTrain,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
            progress?.Report("Opening IRCTC classic...");
            await page.GotoAsync(IrctcSelectors.ClassicTrainSearchUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 90_000
            });

            await DismissLanguagePopupAsync(page, progress);
            await WaitForScheduledSearchAsync(config.ScheduledSearchTime, progress, cancellationToken);

            await FillJourneySearchAsync(page, searchSettings, progress, cancellationToken);
            await HandleNoDirectTrainsDialogAsync(page, progress);

            var booked = await SelectTrainClassAndBookAsync(
                page, selectedTrain, searchSettings, config, progress, cancellationToken);

            if (!booked)
            {
                progress?.Report(
                    "Stopped on train list: train/class/date not auto-selected. "
                    + "In the browser pick a train that is listed → class Refresh → date → Book Now, "
                    + "or press Stop and choose a train that appears on IRCTC for this route.");
                try
                {
                    await page.Locator(IrctcSelectors.LoginUserId).First
                        .WaitForAsync(new LocatorWaitForOptions { Timeout = 300_000 });
                    progress?.Report("Login appeared after manual Book Now — continuing...");
                }
                catch (TimeoutException)
                {
                    progress?.Report("No login after 5 min. Press Stop, then Book IRCTC again with a listed train.");
                    await Task.Delay(Timeout.Infinite, cancellationToken);
                    return;
                }
            }

            await LoginAsync(page, config, progress, cancellationToken, waitLonger: true);
            await FillPassengersAsync(page, config, progress, cancellationToken);
            await SelectPassengerPaymentTypeAndContinueAsync(page, config, progress, cancellationToken);
            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report(SessionErrorMessage());
                await HandleSessionErrorAsync(page, progress, cancellationToken);
                return;
            }

            await HandleReviewCaptchaAndContinueAsync(page, progress, cancellationToken);
            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report(SessionErrorMessage());
                await HandleSessionErrorAsync(page, progress, cancellationToken);
                return;
            }

            await SelectPaymentGatewayAndPayAsync(page, config, progress, cancellationToken);
            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report(SessionErrorMessage());
                await HandleSessionErrorAsync(page, progress, cancellationToken);
                return;
            }

            await CapturePaymentAndWaitForConfirmationAsync(page, progress, cancellationToken);
    }

    /// <summary>
    /// New IRCTC beta UI (eticket): different search, train cards, BOOK buttons, login modal.
    /// </summary>
    private static async Task BookTrainBetaAsync(
        IPage page,
        TrainSearchSettings searchSettings,
        TrainResult selectedTrain,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report("Opening IRCTC beta (eticket)...");
        await page.GotoAsync(IrctcSelectors.BetaTrainSearchUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 90_000
        });

        // If redirected to classic home, click Explore beta / go again
        if (!page.Url.Contains("eticket", StringComparison.OrdinalIgnoreCase))
        {
            progress?.Report("Not on beta yet — trying Explore beta / BETA Version...");
            try
            {
                var betaBtn = page.Locator(IrctcSelectors.BetaExploreButton).First;
                if (await betaBtn.CountAsync() > 0)
                {
                    await betaBtn.ClickAsync();
                    await page.WaitForTimeoutAsync(2000);
                }
            }
            catch
            {
                // ignore
            }

            if (!page.Url.Contains("eticket", StringComparison.OrdinalIgnoreCase))
            {
                await page.GotoAsync(IrctcSelectors.BetaHomeUrl, new PageGotoOptions
                {
                    WaitUntil = WaitUntilState.DOMContentLoaded,
                    Timeout = 90_000
                });
            }
        }

        await WaitForScheduledSearchAsync(config.ScheduledSearchTime, progress, cancellationToken);

        // If LOGIN popup is already on search (blocks From/To) → fill & sign in now.
        // If not → search first; login after BOOK if the modal appears then.
        await page.WaitForTimeoutAsync(1000);
        if (await LoginBetaIfModalPresentAsync(page, config, progress, cancellationToken, waitMs: 3_000))
        {
            progress?.Report("Beta: logged in on search page — continuing From/To...");
        }

        await FillBetaJourneySearchAsync(page, searchSettings, config, progress, cancellationToken);

        if (await IsSessionErrorPageAsync(page)
            || page.Url.Contains("/error", StringComparison.OrdinalIgnoreCase))
        {
            progress?.Report(SessionErrorMessage());
            await HandleSessionErrorAsync(page, progress, cancellationToken);
            return;
        }

        if (!page.Url.Contains("train-list", StringComparison.OrdinalIgnoreCase))
        {
            progress?.Report("Beta: not on train list after search — stopping before train clicks.");
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return;
        }

        // LOGIN shows after BOOK — don't wait here; search/select first
        var booked = await SelectBetaTrainAndBookAsync(
            page, selectedTrain, searchSettings, config, progress, cancellationToken);

        if (!booked)
        {
            progress?.Report(
                "Beta: could not auto BOOK. Click Check Availability → BOOK once in the browser; automation will continue at login.");
        }

        // LOGIN modal appears after BOOK (still on train-list URL) — fill immediately
        if (await IsSessionErrorPageAsync(page)
            || page.Url.Contains("/error", StringComparison.OrdinalIgnoreCase))
        {
            progress?.Report(SessionErrorMessage());
            await HandleSessionErrorAsync(page, progress, cancellationToken);
            return;
        }

        await LoginBetaAsync(page, config, progress, cancellationToken);
        await FillBetaPassengersAsync(page, config, progress, cancellationToken);
        await SelectBetaPaymentAndContinueAsync(page, config, progress, cancellationToken);
        if (await IsSessionErrorPageAsync(page))
        {
            await ReportBetaActivityAuditAsync(page, progress, "after Calculate Fare / Continue");
            progress?.Report(SessionErrorMessage());
            await HandleSessionErrorAsync(page, progress, cancellationToken);
            return;
        }

        // Beta review has no classic captcha step — payment page follows Continue To Payment.
        if (!page.Url.Contains("payment", StringComparison.OrdinalIgnoreCase)
            && !page.Url.Contains("bkgPayment", StringComparison.OrdinalIgnoreCase))
        {
            await HandleReviewCaptchaAndContinueAsync(page, progress, cancellationToken);
            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report(SessionErrorMessage());
                await HandleSessionErrorAsync(page, progress, cancellationToken);
                return;
            }
        }

        await SelectBetaPaymentGatewayAndPayAsync(page, config, progress, cancellationToken);
        if (await IsSessionErrorPageAsync(page))
        {
            progress?.Report(SessionErrorMessage());
            await HandleSessionErrorAsync(page, progress, cancellationToken);
            return;
        }

        await CapturePaymentAndWaitForConfirmationAsync(page, progress, cancellationToken);
    }

    private static async Task FillBetaJourneySearchAsync(
        IPage page,
        TrainSearchSettings settings,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report("Beta: filling From / To / Date / Quota...");
        // LOGIN can pop over From/To — sign in if present, never Escape/close
        await LoginBetaIfModalPresentAsync(page, config, progress, cancellationToken, waitMs: 800);
        await page.Locator(IrctcSelectors.BetaFromCombobox)
            .First
            .WaitForAsync(new LocatorWaitForOptions { Timeout = 45_000 });
        await page.WaitForTimeoutAsync(800);

        await FillBetaStationComboboxAsync(
            page, IrctcSelectors.BetaFromCombobox, settings.FromStationCode, "From",
            config, progress, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        await LoginBetaIfModalPresentAsync(page, config, progress, cancellationToken, waitMs: 400);
        await FillBetaStationComboboxAsync(
            page, IrctcSelectors.BetaToCombobox, settings.ToStationCode, "To",
            config, progress, cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();

        await LoginBetaIfModalPresentAsync(page, config, progress, cancellationToken, waitMs: 400);
        await SelectBetaTravelDateAsync(page, settings.TravelDate, progress);

        try
        {
            var quotaLabel = IrctcQuotaLabels.ToDisplayLabel(settings.Quota);
            if (!quotaLabel.Equals("GENERAL", StringComparison.OrdinalIgnoreCase))
            {
                var quotaBox = page.Locator(IrctcSelectors.BetaQuotaCombobox).First;
                await quotaBox.ClickAsync();
                await page.WaitForTimeoutAsync(400);
                var item = page.Locator($"{IrctcSelectors.BetaStationOption}, li, button, a, span")
                    .Filter(new LocatorFilterOptions { HasText = quotaLabel })
                    .First;
                if (await item.CountAsync() > 0)
                {
                    await item.ClickAsync();
                    progress?.Report($"Beta quota: {quotaLabel}");
                }
            }
        }
        catch (Exception ex)
        {
            progress?.Report($"Beta quota note: {ex.Message}");
        }

        cancellationToken.ThrowIfCancellationRequested();

        // Do NOT dismiss-login / Escape around Search — that can kill the beta session.
        progress?.Report("Beta: Search Trains (one DOM click)...");
        var searchBtn = page.GetByRole(AriaRole.Button, new() { Name = "Search Trains" }).First;
        await searchBtn.WaitForAsync(new LocatorWaitForOptions
        {
            State = WaitForSelectorState.Visible,
            Timeout = 20_000
        });
        // Native element.click() once — avoids Playwright actionability re-clicks during Angular nav
        await searchBtn.EvaluateAsync("el => el.click()");

        try
        {
            await page.WaitForURLAsync(
                url => url.Contains("train-list", StringComparison.OrdinalIgnoreCase)
                       || url.Contains("/error", StringComparison.OrdinalIgnoreCase),
                new PageWaitForURLOptions { Timeout = 60_000 });
        }
        catch
        {
            progress?.Report("Beta: waiting for train list...");
            await page.WaitForTimeoutAsync(3000);
        }

        if (await IsSessionErrorPageAsync(page)
            || page.Url.Contains("/error", StringComparison.OrdinalIgnoreCase))
        {
            progress?.Report(
                "Beta: IRCTC session error right after Search Trains "
                + "(not train selection yet). Usually multi-login or Search was rejected.");
            await ReportBetaActivityAuditAsync(page, progress, "after Search Trains");
            return;
        }

        if (!page.Url.Contains("train-list", StringComparison.OrdinalIgnoreCase))
        {
            progress?.Report($"Beta: unexpected URL after search: {page.Url}");
        }
        else
        {
            progress?.Report("Beta: train list loaded...");
            await page.WaitForTimeoutAsync(600);
        }
    }

    /// <summary>
    /// If the LOGIN modal is visible (within waitMs), fill credentials and sign in.
    /// Returns true when login was performed. Does not close/Escape the modal.
    /// </summary>
    private static async Task<bool> LoginBetaIfModalPresentAsync(
        IPage page,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken,
        int waitMs = 2_000)
    {
        try
        {
            var user = page.GetByPlaceholder("Enter Username")
                .Or(page.Locator("input[placeholder*='Username']"))
                .First;
            try
            {
                await user.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = Math.Max(200, waitMs)
                });
            }
            catch
            {
                return false;
            }

            progress?.Report("Beta: LOGIN appeared — entering username/password...");
            await SubmitBetaLoginCredentialsAsync(
                page, config, progress, cancellationToken, waitForPassengerPage: false);
            return true;
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            progress?.Report($"Beta login-if-present note: {ex.Message}");
            return false;
        }
    }

    private static async Task SubmitBetaLoginCredentialsAsync(
        IPage page,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken,
        bool waitForPassengerPage)
    {
        if (string.IsNullOrWhiteSpace(config.Credentials.Username) ||
            string.IsNullOrWhiteSpace(config.Credentials.Password))
        {
            progress?.Report("ERROR: Username/password empty.");
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return;
        }

        // Prefer Angular-safe DOM fill (Playwright Fill often does not update beta form controls)
        progress?.Report("Beta: filling LOGIN via DOM...");
        var filled = await page.EvaluateAsync<bool>("""
            ({ username, password }) => {
              const visible = (el) => {
                if (!el) return false;
                const r = el.getBoundingClientRect();
                const st = window.getComputedStyle(el);
                return r.width > 0 && r.height > 0 && st.visibility !== 'hidden' && st.display !== 'none';
              };
              const setVal = (el, value) => {
                el.focus();
                const proto = window.HTMLInputElement.prototype;
                const desc = Object.getOwnPropertyDescriptor(proto, 'value');
                if (desc && desc.set) desc.set.call(el, value);
                else el.value = value;
                el.dispatchEvent(new Event('input', { bubbles: true }));
                el.dispatchEvent(new Event('change', { bubbles: true }));
              };
              const inputs = Array.from(document.querySelectorAll('input')).filter(visible);
              const user = inputs.find(i => {
                const ph = (i.getAttribute('placeholder') || '').toLowerCase();
                const name = (i.getAttribute('formcontrolname') || '').toLowerCase();
                const aria = (i.getAttribute('aria-label') || '').toLowerCase();
                return ph.includes('username') || ph.includes('user name') || name === 'userid' || aria.includes('username');
              });
              const pass = inputs.find(i => {
                const ph = (i.getAttribute('placeholder') || '').toLowerCase();
                const name = (i.getAttribute('formcontrolname') || '').toLowerCase();
                const type = (i.getAttribute('type') || '').toLowerCase();
                return type === 'password' || ph.includes('password') || name === 'password';
              });
              if (!user || !pass) return false;
              setVal(user, username);
              setVal(pass, password);
              return true;
            }
            """, new { username = config.Credentials.Username, password = config.Credentials.Password });

        if (!filled)
        {
            progress?.Report("Beta: DOM fill missed fields — trying Playwright placeholders...");
            var user = page.GetByPlaceholder("Enter Username").Or(page.GetByPlaceholder("Username")).First;
            await user.WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 10_000 });
            await user.ClickAsync();
            await user.FillAsync(config.Credentials.Username);
            var pass = page.GetByPlaceholder("Enter password").Or(page.GetByPlaceholder("Password")).First;
            await pass.ClickAsync();
            await pass.FillAsync(config.Credentials.Password);
        }
        else
        {
            progress?.Report("Beta: username/password filled.");
        }

        await page.WaitForTimeoutAsync(400);
        progress?.Report("Beta: clicking LOGIN once...");

        // ONE click only — IRCTC kills the session on double-click / double-submit
        var clicked = await page.EvaluateAsync<bool>("""
            () => {
              const norm = (el) => (el.textContent || el.value || '').replace(/\s+/g, ' ').trim().toUpperCase();
              const isHeaderLogin = (b) => String(b.className || '').includes('btn-login');

              const userInput = Array.from(document.querySelectorAll('input')).find(i => {
                const ph = (i.getAttribute('placeholder') || '').toLowerCase();
                return ph.includes('username') || ph.includes('user name');
              });
              let root = userInput;
              for (let i = 0; i < 12 && root; i++) {
                root = root.parentElement;
                if (!root) break;
                const cls = String(root.className || '').toLowerCase();
                if (cls.includes('login') || cls.includes('dialog') || cls.includes('modal') ||
                    root.getAttribute('role') === 'dialog') {
                  break;
                }
              }
              root = root || document.body;

              const scrollables = [root, ...root.querySelectorAll('*')];
              for (const el of scrollables) {
                try {
                  if (el.scrollHeight > el.clientHeight + 20) el.scrollTop = el.scrollHeight;
                } catch (e) {}
              }

              const candidates = Array.from(root.querySelectorAll('button, a, input[type=submit]'))
                .filter(el => {
                  const t = norm(el);
                  if (t !== 'LOGIN' && t !== 'SIGN IN') return false;
                  if (isHeaderLogin(el)) return false;
                  return true;
                });

              candidates.sort((a, b) => {
                const score = (el) => el.tagName === 'BUTTON' || el.tagName === 'INPUT' ? 0 : 1;
                return score(a) - score(b);
              });

              const btn = candidates[0];
              if (!btn) return false;
              try { btn.scrollIntoView({ block: 'center', inline: 'nearest' }); } catch (e) {}
              // Single native click — do not also dispatch MouseEvents (that = double-click to IRCTC)
              btn.click();
              return true;
            }
            """);

        if (!clicked)
        {
            progress?.Report("Beta: LOGIN button not found via DOM — one Playwright click...");
            try
            {
                var loginBtn = page.Locator(IrctcSelectors.BetaLoginButton).First;
                await loginBtn.ScrollIntoViewIfNeededAsync();
                // Force + no retry spam: one click
                await loginBtn.ClickAsync(new LocatorClickOptions
                {
                    Timeout = 8_000,
                    Force = true
                });
                clicked = true;
            }
            catch (Exception ex)
            {
                progress?.Report($"Beta: could not click LOGIN ({ex.Message}). Click LOGIN once yourself.");
            }
        }
        else
        {
            progress?.Report("Beta: LOGIN clicked (once).");
        }

        progress?.Report("ACTION REQUIRED: Enter OTP if IRCTC asks, then wait for passenger page...");

        if (waitForPassengerPage)
        {
            await WaitForBetaPassengerPageAsync(page, progress, cancellationToken);
            return;
        }

        // Early login: wait until username field hides
        try
        {
            await page.GetByPlaceholder("Enter Username").First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Hidden,
                Timeout = 300_000
            });
            progress?.Report("Beta: LOGIN modal closed — signed in.");
        }
        catch
        {
            progress?.Report("Beta: LOGIN still open (OTP?) — finish manually if needed, then continue.");
        }

        await page.WaitForTimeoutAsync(800);
    }

    private static async Task WaitForBetaPassengerPageAsync(
        IPage page,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        try
        {
            await page.GetByText("Passenger Details", new() { Exact = false }).First
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 300_000 });
            progress?.Report("Beta passenger / review page ready.");
        }
        catch
        {
            try
            {
                await page.GetByText("New Passenger", new() { Exact = false }).First
                    .WaitForAsync(new LocatorWaitForOptions { Timeout = 60_000 });
                progress?.Report("Beta passenger page ready (New Passenger).");
            }
            catch
            {
                progress?.Report("Waiting for passenger page after beta login...");
                await Task.Delay(3000, cancellationToken);
            }
        }
    }

    private static async Task FillBetaStationComboboxAsync(
        IPage page,
        string comboboxSelector,
        string stationCode,
        string label,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            // If LOGIN covers the station box, fill credentials then retry From/To
            await LoginBetaIfModalPresentAsync(page, config, progress, cancellationToken, waitMs: 400);

            var box = page.Locator(comboboxSelector).First;
            await box.ClickAsync();
            await page.WaitForTimeoutAsync(350);

            var loginUser = page.GetByPlaceholder("Enter Username").First;
            if (await loginUser.CountAsync() > 0 && await loginUser.IsVisibleAsync())
            {
                progress?.Report($"Beta {label}: LOGIN blocked station — signing in...");
                await LoginBetaIfModalPresentAsync(page, config, progress, cancellationToken, waitMs: 1_000);
                continue;
            }

            var search = page.Locator(IrctcSelectors.BetaStationSearchInput).First;
            try
            {
                await search.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 8_000
                });
            }
            catch
            {
                progress?.Report($"Beta {label}: search box missing (attempt {attempt})...");
                await LoginBetaIfModalPresentAsync(page, config, progress, cancellationToken, waitMs: 500);
                continue;
            }

            await search.ClickAsync();
            await search.FillAsync("");
            await search.PressSequentiallyAsync(stationCode, new LocatorPressSequentiallyOptions { Delay = 70 });
            await page.WaitForTimeoutAsync(900);

            if (await loginUser.CountAsync() > 0 && await loginUser.IsVisibleAsync())
            {
                progress?.Report($"Beta {label}: LOGIN stole focus — signing in then retrying...");
                await LoginBetaIfModalPresentAsync(page, config, progress, cancellationToken, waitMs: 1_000);
                continue;
            }

            var option = page.Locator(IrctcSelectors.BetaStationOption)
                .Filter(new LocatorFilterOptions { HasText = stationCode })
                .First;
            try
            {
                await option.WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 12_000
                });
                await option.ClickAsync();
            }
            catch
            {
                progress?.Report($"Beta {label}: no dropdown match for {stationCode}, trying Enter...");
                await page.Keyboard.PressAsync("ArrowDown");
                await page.Keyboard.PressAsync("Enter");
            }

            await page.WaitForTimeoutAsync(400);
            progress?.Report($"Beta {label}: {stationCode}");
            return;
        }

        throw new InvalidOperationException(
            $"Beta could not fill {label} station ({stationCode}).");
    }

    private static async Task SelectBetaTravelDateAsync(
        IPage page,
        DateTime travelDate,
        IProgress<string>? progress)
    {
        var dateText = travelDate.ToString("dd MMM yyyy", System.Globalization.CultureInfo.InvariantCulture);
        try
        {
            // Skip if already showing the target date
            var dateBtn = page.Locator(IrctcSelectors.BetaDateButton).First;
            var current = (await dateBtn.InnerTextAsync()).Replace("\n", " ").Trim();
            if (current.Contains(dateText, StringComparison.OrdinalIgnoreCase)
                || current.Contains(travelDate.ToString("d MMM yyyy", System.Globalization.CultureInfo.InvariantCulture),
                    StringComparison.OrdinalIgnoreCase))
            {
                progress?.Report($"Beta date already set: {dateText}");
                return;
            }

            await dateBtn.ClickAsync();
            await page.Locator(IrctcSelectors.BetaCalendarPanel).First
                .WaitForAsync(new LocatorWaitForOptions { State = WaitForSelectorState.Visible, Timeout = 8_000 });

            var targetMonth = travelDate.ToString("MMMM", System.Globalization.CultureInfo.InvariantCulture);
            var targetYear = travelDate.Year.ToString(System.Globalization.CultureInfo.InvariantCulture);
            var day = travelDate.Day.ToString(System.Globalization.CultureInfo.InvariantCulture);

            for (var i = 0; i < 18; i++)
            {
                var month = (await page.Locator(IrctcSelectors.BetaCalendarMonth).First.InnerTextAsync()).Trim();
                var year = (await page.Locator(IrctcSelectors.BetaCalendarYear).First.InnerTextAsync()).Trim();
                if (month.Equals(targetMonth, StringComparison.OrdinalIgnoreCase)
                    && year.Equals(targetYear, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                var shown = DateTime.TryParseExact(
                    $"{month} {year}",
                    "MMMM yyyy",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None,
                    out var shownMonth)
                    ? shownMonth
                    : DateTime.MinValue;

                if (shown != DateTime.MinValue && shown > new DateTime(travelDate.Year, travelDate.Month, 1))
                {
                    await page.Locator(IrctcSelectors.BetaCalendarPrev).First.ClickAsync();
                }
                else
                {
                    await page.Locator(IrctcSelectors.BetaCalendarNext).First.ClickAsync();
                }

                await page.WaitForTimeoutAsync(250);
            }

            var cells = page.Locator(IrctcSelectors.BetaCalendarDayCells);
            var count = await cells.CountAsync();
            ILocator? dayCell = null;
            for (var i = 0; i < count; i++)
            {
                var cell = cells.Nth(i);
                var text = (await cell.InnerTextAsync()).Trim();
                if (text == day)
                {
                    dayCell = cell;
                    break;
                }
            }

            if (dayCell is null)
            {
                throw new InvalidOperationException($"Day {day} not found in beta calendar.");
            }

            await dayCell.ClickAsync();
            progress?.Report($"Beta date: {dateText}");
            await page.WaitForTimeoutAsync(400);
        }
        catch (Exception ex)
        {
            progress?.Report($"Beta date note: {ex.Message} (leaving calendar default)");
            try { await page.Keyboard.PressAsync("Escape"); } catch { /* ignore */ }
        }
    }

    private static async Task<bool> SelectBetaTrainAndBookAsync(
        IPage page,
        TrainResult selectedTrain,
        TrainSearchSettings settings,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var trainNum = NormalizeTrainNumber(selectedTrain.TrainNumber);
        var classCode = NormalizeClassCode(
            string.IsNullOrWhiteSpace(settings.PreferredClass)
                ? config.PreferredClass
                : settings.PreferredClass);

        progress?.Report($"Beta: on train list — looking for {trainNum} / {classCode}...");

        if (await IsSessionErrorPageAsync(page))
        {
            return false;
        }

        // Wait for at least one Check Availability control (button or clickable text)
        try
        {
            await page.GetByText("Check Availability", new() { Exact = false })
                .First
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 45_000 });
        }
        catch (TimeoutException)
        {
            progress?.Report("Beta: no Check Availability on page yet.");
            if (await IsSessionErrorPageAsync(page))
            {
                return false;
            }
        }

        await page.WaitForTimeoutAsync(400);

        // Resolve which train to use (exact → visible list fallback messaging)
        var resolvedTrain = await ResolveBetaTrainNumberAsync(page, trainNum, progress);
        if (string.IsNullOrWhiteSpace(resolvedTrain))
        {
            return false;
        }

        trainNum = resolvedTrain;

        // 1) Click class chip + Check Availability via DOM (Angular-safe)
        var checkJson = await page.EvaluateAsync<string>("""
            ({ trainNum, classCode }) => {
              const dig = (s) => (s || '').replace(/\D/g, '');
              const want = dig(trainNum);
              // button/a only — clicking span+parent was treated as double-click by IRCTC
              const nodes = Array.from(document.querySelectorAll('button, a'));
              const checks = nodes.filter(el => {
                const t = (el.textContent || '').replace(/\s+/g, ' ').trim();
                return /^check\s*avail/i.test(t) && t.length < 36;
              });
              const visibleTrains = [];
              for (const btn of checks) {
                let card = btn;
                for (let i = 0; i < 14 && card; i++) {
                  const tx = card.textContent || '';
                  if (/train\s*schedule/i.test(tx) && /check\s*avail/i.test(tx) && tx.length < 5000) break;
                  card = card.parentElement;
                }
                if (!card) continue;
                const cardText = card.textContent || '';
                const m = cardText.match(/\b(\d{4,5})\b/);
                if (m) visibleTrains.push(m[1]);
                if (dig(cardText).indexOf(want) < 0 && cardText.indexOf(trainNum) < 0) continue;

                const chips = Array.from(card.querySelectorAll('button, a'));
                for (const c of chips) {
                  const ct = (c.textContent || '').replace(/\s+/g, ' ').trim();
                  if (ct === classCode) {
                    try { c.click(); } catch (e) {}
                    break;
                  }
                }

                try { btn.scrollIntoView({ block: 'center' }); } catch (e) {}
                try { btn.click(); } catch (e) {}
                return JSON.stringify({ ok: true, step: 'check', train: trainNum, visible: visibleTrains.slice(0, 15) });
              }
              return JSON.stringify({ ok: false, step: 'no-check', visible: [...new Set(visibleTrains)].slice(0, 15) });
            }
            """, new { trainNum, classCode });

        var checkResult = ParseBetaJson(checkJson);
        if (checkResult.Ok)
        {
            progress?.Report($"Beta: Check Availability clicked for {trainNum}.");
        }
        else
        {
            progress?.Report(
                $"Beta: could not click Check Availability for {trainNum}. "
                + $"Visible trains: {string.Join(", ", checkResult.Visible)}. Trying Playwright fallback...");

            // Playwright fallback: Check Availability near train text
            try
            {
                var card = page.Locator("div, article, section, li")
                    .Filter(new LocatorFilterOptions { HasText = trainNum })
                    .Filter(new LocatorFilterOptions { HasText = "Check Availability" })
                    .Last;
                await card.ScrollIntoViewIfNeededAsync();
                var classEl = card.GetByText(classCode, new() { Exact = true }).First;
                if (await classEl.CountAsync() > 0)
                {
                    await classEl.ClickAsync(new LocatorClickOptions { Timeout = 3_000, Force = true });
                }

                await card.GetByText("Check Availability", new() { Exact = false }).First
                    .ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
                checkResult = new BetaDomResult { Ok = true };
                progress?.Report("Beta: Check Availability clicked (Playwright).");
            }
            catch (Exception ex)
            {
                progress?.Report($"Beta Check Availability failed: {ex.Message}");
                return false;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        await page.WaitForTimeoutAsync(800);

        // 2) Wait for BOOK and click once (button/a only — never span/div)
        for (var attempt = 1; attempt <= 8; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await IsSessionErrorPageAsync(page)
                || page.Url.Contains("/error", StringComparison.OrdinalIgnoreCase))
            {
                progress?.Report("Beta: session error before BOOK — stopping.");
                return false;
            }

            progress?.Report($"Beta: looking for BOOK ({classCode}) attempt {attempt}/8...");

            var bookJson = await page.EvaluateAsync<string>("""
                ({ trainNum, classCode }) => {
                  const dig = (s) => (s || '').replace(/\D/g, '');
                  const want = dig(trainNum);
                  // Only real controls — clicking nested span+button was treated as double-click by IRCTC
                  const books = Array.from(document.querySelectorAll('button, a')).filter(el => {
                    const t = (el.textContent || '').replace(/\s+/g, ' ').trim().toUpperCase();
                    return t === 'BOOK' || t === 'BOOK NOW';
                  });
                  if (books.length === 0) {
                    return JSON.stringify({ ok: false, step: 'no-book', count: 0 });
                  }

                  const classHints = {
                    'SL': ['SL', 'SLEEPER'],
                    '3A': ['3A', 'AC 3', 'AC3', 'THIRD'],
                    '2A': ['2A', 'AC 2', 'AC2', 'SECOND'],
                    '1A': ['1A', 'FIRST', 'AC FIRST'],
                    '3E': ['3E'],
                    '2S': ['2S'],
                    'CC': ['CC', 'CHAIR']
                  };
                  const hints = classHints[classCode] || [classCode];

                  for (const b of books) {
                    let row = b;
                    let ctx = '';
                    for (let i = 0; i < 7 && row; i++) {
                      const t = (row.textContent || '').trim();
                      if (t.length > 15 && t.length < 2000) ctx = t;
                      row = row.parentElement;
                    }
                    const up = ctx.toUpperCase();
                    const classOk = hints.some(h => up.indexOf(h) >= 0);
                    const trainOk = dig(ctx).indexOf(want) >= 0 || ctx.indexOf(trainNum) >= 0;
                    if (!classOk && books.length > 1) continue;
                    if (!trainOk && books.length > 1) continue;
                    try { b.scrollIntoView({ block: 'center' }); } catch (e) {}
                    try { b.click(); } catch (e) {}
                    return JSON.stringify({ ok: true, step: 'book', classOk: classOk });
                  }

                  try { books[0].scrollIntoView({ block: 'center' }); books[0].click(); } catch (e) {}
                  return JSON.stringify({ ok: true, step: 'book-first', count: books.length });
                }
                """, new { trainNum, classCode });

            var bookResult = ParseBetaJson(bookJson);
            if (bookResult.Ok)
            {
                progress?.Report("Beta: BOOK clicked once — waiting for LOGIN...");
                await page.WaitForTimeoutAsync(400);
                if (await IsSessionErrorPageAsync(page)
                    || page.Url.Contains("/error", StringComparison.OrdinalIgnoreCase))
                {
                    progress?.Report("Beta: session error right after BOOK.");
                    return false;
                }

                return true;
            }

            // Do NOT re-click Check Availability (IRCTC treats that as spam / double action)
            await page.WaitForTimeoutAsync(600);
        }

        progress?.Report(
            "Beta: BOOK still not available. In browser click Check Availability → BOOK once for your class.");
        return false;
    }

    private static async Task<string?> ResolveBetaTrainNumberAsync(
        IPage page,
        string trainNum,
        IProgress<string>? progress)
    {
        var visible = await ListBetaVisibleTrainNumbersAsync(page);
        if (visible.Count == 0)
        {
            // Page text dump may still have trains before buttons hydrate
            await page.WaitForTimeoutAsync(500);
            visible = await ListBetaVisibleTrainNumbersAsync(page);
        }

        if (visible.Any(v => NormalizeTrainNumber(v) == trainNum || v.Contains(trainNum, StringComparison.Ordinal)))
        {
            return trainNum;
        }

        // Exact text present somewhere?
        try
        {
            if (await page.GetByText(trainNum, new() { Exact = false }).CountAsync() > 0)
            {
                return trainNum;
            }
        }
        catch
        {
            // ignore
        }

        progress?.Report(
            $"Beta: train {trainNum} not on this IRCTC list. Visible: "
            + (visible.Count == 0 ? "(none detected)" : string.Join(", ", visible))
            + ". Pick a listed train in the app, or click Check Availability → BOOK manually.");
        return null;
    }

    private sealed class BetaDomResult
    {
        public bool Ok { get; set; }
        public List<string> Visible { get; set; } = [];
    }

    private static BetaDomResult ParseBetaJson(string? json)
    {
        var result = new BetaDomResult();
        if (string.IsNullOrWhiteSpace(json))
        {
            return result;
        }

        try
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            if (root.TryGetProperty("ok", out var ok))
            {
                result.Ok = ok.GetBoolean();
            }

            if (root.TryGetProperty("visible", out var vis) && vis.ValueKind == System.Text.Json.JsonValueKind.Array)
            {
                foreach (var x in vis.EnumerateArray())
                {
                    var s = x.GetString();
                    if (!string.IsNullOrWhiteSpace(s))
                    {
                        result.Visible.Add(s);
                    }
                }
            }
        }
        catch
        {
            // ignore parse errors
        }

        return result;
    }

    private static async Task<List<string>> ListBetaVisibleTrainNumbersAsync(IPage page)
    {
        try
        {
            var json = await page.EvaluateAsync<string>("""
                () => {
                  const text = document.body ? document.body.innerText : '';
                  const matches = text.match(/\b\d{5}\b/g) || [];
                  const unique = [...new Set(matches)].slice(0, 25);
                  return JSON.stringify(unique);
                }
                """);
            if (string.IsNullOrWhiteSpace(json))
            {
                return [];
            }

            return System.Text.Json.JsonSerializer.Deserialize<List<string>>(json) ?? [];
        }
        catch
        {
            return [];
        }
    }


    private static async Task LoginBetaAsync(
        IPage page,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report("Beta: after BOOK — looking for LOGIN modal to fill...");

        // Poll: LOGIN fields OR passenger page (do not use flaky .Or().First that can skip fill)
        var deadline = DateTime.UtcNow.AddMinutes(3);
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await IsSessionErrorPageAsync(page)
                || page.Url.Contains("/error", StringComparison.OrdinalIgnoreCase))
            {
                progress?.Report("Beta: session error while waiting for login.");
                return;
            }

            // Visible LOGIN username? Fill it — this is the post-BOOK modal on train-list
            var loginVisible = await page.EvaluateAsync<bool>("""
                () => {
                  const visible = (el) => {
                    if (!el) return false;
                    const r = el.getBoundingClientRect();
                    const st = window.getComputedStyle(el);
                    return r.width > 2 && r.height > 2
                      && st.visibility !== 'hidden' && st.display !== 'none'
                      && st.opacity !== '0';
                  };
                  const inputs = Array.from(document.querySelectorAll('input')).filter(visible);
                  return inputs.some(i => {
                    const ph = (i.getAttribute('placeholder') || '').toLowerCase();
                    return ph.includes('enter username') || ph === 'username' || ph.includes('username');
                  });
                }
                """);

            if (loginVisible)
            {
                progress?.Report("Beta: LOGIN modal visible after BOOK — filling now...");
                await SubmitBetaLoginCredentialsAsync(
                    page, config, progress, cancellationToken, waitForPassengerPage: true);
                progress?.Report("Beta login done.");
                return;
            }

            // Already past login?
            try
            {
                var passenger = page.GetByText("Passenger Details", new() { Exact = false }).First;
                if (await passenger.CountAsync() > 0 && await passenger.IsVisibleAsync())
                {
                    progress?.Report("Beta: passenger page already open — login not needed.");
                    return;
                }
            }
            catch
            {
                // keep waiting
            }

            await page.WaitForTimeoutAsync(400);
        }

        progress?.Report("Beta: LOGIN modal did not appear in 3 min — trying classic login...");
        await LoginAsync(page, config, progress, cancellationToken, waitLonger: true);
    }

    private static async Task FillBetaPassengersAsync(
        IPage page,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report("Beta: filling passenger details...");

        // Already on passenger page?
        try
        {
            await page.GetByText("Passenger Details", new() { Exact = false }).First
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 60_000 });
        }
        catch (TimeoutException)
        {
            progress?.Report("Beta: passenger page not detected — trying classic filler...");
            await FillPassengersAsync(page, config, progress, cancellationToken);
            return;
        }

        await page.WaitForTimeoutAsync(800);
        var added = 0;

        foreach (var pax in config.Passengers)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (string.IsNullOrWhiteSpace(pax.Name))
            {
                continue;
            }

            progress?.Report($"Beta: adding passenger {pax.Name}...");

            // Open New Passenger form / modal
            var opened = await ClickBetaNewPassengerAsync(page, progress);
            if (!opened)
            {
                progress?.Report("Beta: New Passenger button not found.");
                break;
            }

            await page.WaitForTimeoutAsync(700);

            // Name
            if (!await FillFirstVisibleInputAsync(page, IrctcSelectors.BetaPassengerNameInput, pax.Name))
            {
                // Broader fallback
                await FillFirstVisibleInputAsync(
                    page,
                    "input[type='text']:visible, input:not([type]):visible",
                    pax.Name);
            }

            // Age
            if (!string.IsNullOrWhiteSpace(pax.Age))
            {
                if (!await FillFirstVisibleInputAsync(page, IrctcSelectors.BetaPassengerAgeInput, pax.Age))
                {
                    await page.EvaluateAsync("""
                        (age) => {
                          const inputs = Array.from(document.querySelectorAll('input'));
                          for (const i of inputs) {
                            const ph = (i.placeholder || '') + ' ' + (i.getAttribute('aria-label') || '') + ' ' + (i.name || '');
                            if (/age/i.test(ph) || i.type === 'number') {
                              i.focus(); i.value = ''; i.dispatchEvent(new Event('input', { bubbles: true }));
                              i.value = String(age);
                              i.dispatchEvent(new Event('input', { bubbles: true }));
                              i.dispatchEvent(new Event('change', { bubbles: true }));
                              return true;
                            }
                          }
                          return false;
                        }
                        """, pax.Age);
                }
            }

            // Gender (required on beta — dropdown defaults to "Select")
            var genderOk = await SelectBetaGenderAsync(page, pax.Gender, progress);
            if (!genderOk)
            {
                progress?.Report($"Beta: gender not set for {pax.Name} — retrying...");
                await page.WaitForTimeoutAsync(400);
                genderOk = await SelectBetaGenderAsync(page, pax.Gender, progress);
            }

            // Berth (optional)
            if (!string.IsNullOrWhiteSpace(pax.BerthPreference)
                && !pax.BerthPreference.Equals("No Preference", StringComparison.OrdinalIgnoreCase))
            {
                await SelectBetaDropdownByTextAsync(page, pax.BerthPreference, "berth", progress);
            }

            // Save / Add in modal (retry if validation toast / dialog stays open)
            var saved = false;
            for (var saveTry = 1; saveTry <= 2; saveTry++)
            {
                if (!genderOk)
                {
                    genderOk = await SelectBetaGenderAsync(page, pax.Gender, progress);
                    if (!genderOk)
                    {
                        progress?.Report("Beta: Gender still Select — not clicking Add yet.");
                        break;
                    }
                }

                await ClickBetaPassengerSaveAsync(page);
                await page.WaitForTimeoutAsync(1200);

                if (!await IsBetaAddPassengerDialogOpenAsync(page))
                {
                    saved = true;
                    break;
                }

                progress?.Report($"Beta: Add Passenger still open (try {saveTry})...");
                await page.WaitForTimeoutAsync(800);
                genderOk = await SelectBetaGenderAsync(page, pax.Gender, progress);
            }

            if (!saved)
            {
                progress?.Report($"Beta: could not save passenger {pax.Name} — set Gender and click Add Passenger in browser.");
                await page.WaitForTimeoutAsync(8000);
                if (!await IsBetaAddPassengerDialogOpenAsync(page))
                {
                    saved = true;
                }
            }

            if (saved)
            {
                added++;
                progress?.Report($"Beta: saved passenger {pax.Name}.");
            }

            await page.WaitForTimeoutAsync(600);
        }

        // Preferences
        try
        {
            if (config.AutoUpgrade)
            {
                await page.GetByText("Auto Upgradation", new() { Exact = false }).First.ClickAsync(
                    new LocatorClickOptions { Timeout = 3_000, Force = true });
            }
        }
        catch { /* optional */ }

        try
        {
            if (config.ConfirmBerthsOnly)
            {
                await page.GetByText("confirm berths", new() { Exact = false }).First.ClickAsync(
                    new LocatorClickOptions { Timeout = 3_000, Force = true });
            }
        }
        catch { /* optional */ }

        // Alternate mobile
        if (!string.IsNullOrWhiteSpace(config.MobileNumber))
        {
            try
            {
                var mobile = page.Locator(
                        "input[placeholder*='mobile'], input[placeholder*='Mobile'], input[placeholder*='Alternate'], input[formcontrolname*='mobile']")
                    .First;
                if (await mobile.CountAsync() > 0 && await mobile.IsVisibleAsync())
                {
                    await mobile.FillAsync("");
                    await mobile.PressSequentiallyAsync(config.MobileNumber,
                        new LocatorPressSequentiallyOptions { Delay = 30 });
                    progress?.Report("Beta: filled alternate mobile.");
                }
            }
            catch { /* ignore */ }
        }

        progress?.Report(added > 0
            ? $"Beta: filled {added} passenger(s)."
            : "Beta: no passengers auto-added — add them in the browser if needed.");
    }

    private static async Task<bool> ClickBetaNewPassengerAsync(IPage page, IProgress<string>? progress)
    {
        try
        {
            var btn = page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("New Passenger", System.Text.RegularExpressions.RegexOptions.IgnoreCase) }).First;
            if (await btn.CountAsync() > 0 && await btn.IsVisibleAsync())
            {
                await btn.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
                return true;
            }
        }
        catch { /* try text */ }

        try
        {
            var t = page.GetByText("New Passenger", new() { Exact = false }).First;
            if (await t.CountAsync() > 0)
            {
                await t.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Force = true });
                return true;
            }
        }
        catch { /* try JS */ }

        var clicked = await page.EvaluateAsync<string>("""
            () => {
              const nodes = Array.from(document.querySelectorAll('button, a, span, div'));
              for (const el of nodes) {
                const t = (el.textContent || '').replace(/\s+/g, ' ').trim();
                if (/new\s*passenger/i.test(t) && t.length < 40) {
                  el.click();
                  return JSON.stringify({ ok: true });
                }
              }
              return JSON.stringify({ ok: false });
            }
            """);
        var ok = clicked?.Contains("true", StringComparison.OrdinalIgnoreCase) == true;
        if (!ok)
        {
            progress?.Report("Beta: New Passenger control missing.");
        }

        return ok;
    }

    private static async Task<bool> FillFirstVisibleInputAsync(IPage page, string selector, string value)
    {
        try
        {
            var inputs = page.Locator(selector);
            var n = await inputs.CountAsync();
            for (var i = 0; i < n; i++)
            {
                var input = inputs.Nth(i);
                if (!await input.IsVisibleAsync())
                {
                    continue;
                }

                await input.ClickAsync();
                await input.FillAsync("");
                await input.PressSequentiallyAsync(value, new LocatorPressSequentiallyOptions { Delay = 35 });
                return true;
            }
        }
        catch
        {
            // ignore
        }

        return false;
    }

    private static async Task<bool> IsBetaAddPassengerDialogOpenAsync(IPage page)
    {
        try
        {
            // Modal heading / form still visible
            var dialog = page.Locator("[role='dialog'], .p-dialog, .ui-dialog, .modal").Filter(
                new LocatorFilterOptions { HasText = "Add Passenger" });
            if (await dialog.CountAsync() > 0 && await dialog.First.IsVisibleAsync())
            {
                return true;
            }

            // Gender "Select" still showing in an open form is a strong signal
            var genderSelect = page.Locator("text=Gender").Locator("xpath=ancestor::*[.//text()[contains(.,'Select')]][1]");
            if (await page.GetByRole(AriaRole.Dialog).CountAsync() > 0)
            {
                return await page.GetByRole(AriaRole.Dialog).First.IsVisibleAsync();
            }

            _ = genderSelect;
            return false;
        }
        catch
        {
            return false;
        }
    }

    private static async Task<bool> SelectBetaGenderAsync(IPage page, string gender, IProgress<string>? progress)
    {
        var label = gender.StartsWith("F", StringComparison.OrdinalIgnoreCase) ? "Female"
            : gender.StartsWith("T", StringComparison.OrdinalIgnoreCase) ? "Transgender"
            : "Male";
        var shortCode = label.StartsWith("F", StringComparison.OrdinalIgnoreCase) ? "F"
            : label.StartsWith("T", StringComparison.OrdinalIgnoreCase) ? "T"
            : "M";

        progress?.Report($"Beta: selecting gender {label}...");

        // 1) Native <select>
        try
        {
            var selects = page.Locator("select");
            var sc = await selects.CountAsync();
            for (var i = 0; i < sc; i++)
            {
                var sel = selects.Nth(i);
                if (!await sel.IsVisibleAsync())
                {
                    continue;
                }

                var meta = ((await sel.GetAttributeAsync("formcontrolname")) ?? "")
                           + " " + ((await sel.GetAttributeAsync("name")) ?? "")
                           + " " + ((await sel.GetAttributeAsync("id")) ?? "");
                if (!meta.Contains("gender", StringComparison.OrdinalIgnoreCase)
                    && !meta.Contains("sex", StringComparison.OrdinalIgnoreCase))
                {
                    // Still try if options include Male/Female
                    var html = await sel.InnerHTMLAsync();
                    if (!html.Contains("Male", StringComparison.OrdinalIgnoreCase)
                        && !html.Contains("Female", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }
                }

                try
                {
                    await sel.SelectOptionAsync(new SelectOptionValue { Label = label });
                    progress?.Report($"Beta gender: {label} (select)");
                    return true;
                }
                catch
                {
                    try
                    {
                        await sel.SelectOptionAsync(new SelectOptionValue { Value = shortCode });
                        progress?.Report($"Beta gender: {label} (select code)");
                        return true;
                    }
                    catch
                    {
                        // next
                    }
                }
            }
        }
        catch
        {
            // continue
        }

        // 2) Open Gender dropdown (shows "Select") then pick option — scoped to dialog when possible
        try
        {
            var root = page.Locator("[role='dialog'], .p-dialog, .ui-dialog").First;
            if (await root.CountAsync() == 0 || !await root.IsVisibleAsync())
            {
                root = page.Locator("body");
            }

            // Click the control that currently shows Select under Gender
            var opened = await page.EvaluateAsync<string>("""
                () => {
                  const roots = [
                    ...document.querySelectorAll('[role="dialog"], .p-dialog, .ui-dialog, .modal'),
                    document.body
                  ];
                  for (const root of roots) {
                    if (!root) continue;
                    const labels = Array.from(root.querySelectorAll('label, span, div, p'));
                    let genderLabel = null;
                    for (const l of labels) {
                      const t = (l.textContent || '').replace(/\s+/g, ' ').trim();
                      if (t === 'Gender' || t === 'Gender*') { genderLabel = l; break; }
                    }
                    if (!genderLabel) continue;

                    // Walk up to a row/container, find clickable showing Select
                    let row = genderLabel.parentElement;
                    for (let i = 0; i < 5 && row; i++) {
                      const clickables = Array.from(row.querySelectorAll(
                        '[role="combobox"], .p-dropdown, .ui-dropdown, select, button, .p-dropdown-trigger, .field-box, [aria-haspopup="listbox"]'));
                      for (const c of clickables) {
                        const tx = (c.textContent || '').replace(/\s+/g, ' ').trim();
                        if (/select/i.test(tx) || c.tagName === 'SELECT' || c.getAttribute('role') === 'combobox') {
                          c.scrollIntoView({ block: 'center' });
                          c.click();
                          return JSON.stringify({ ok: true, how: 'row-control' });
                        }
                      }
                      // also click element next to label that says Select
                      const sels = Array.from(row.querySelectorAll('div, span, button')).filter(el => {
                        const t = (el.textContent || '').replace(/\s+/g, ' ').trim();
                        return t === 'Select' || t === 'Select Gender';
                      });
                      if (sels.length) {
                        sels[0].click();
                        return JSON.stringify({ ok: true, how: 'select-text' });
                      }
                      row = row.parentElement;
                    }
                  }
                  return JSON.stringify({ ok: false });
                }
                """);

            await page.WaitForTimeoutAsync(400);

            // Pick Male / Female from opened panel
            try
            {
                var option = page.GetByRole(AriaRole.Option, new() { Name = label }).First;
                if (await option.CountAsync() > 0 && await option.IsVisibleAsync())
                {
                    await option.ClickAsync(new LocatorClickOptions { Timeout = 3_000 });
                    progress?.Report($"Beta gender: {label} (option)");
                    return true;
                }
            }
            catch { /* next */ }

            try
            {
                // Prefer exact text match in open dropdown panel
                var candidates = page.Locator("li.p-dropdown-item, li.ui-dropdown-item, [role='option'], li, span");
                var n = Math.Min(await candidates.CountAsync(), 40);
                for (var i = 0; i < n; i++)
                {
                    var el = candidates.Nth(i);
                    if (!await el.IsVisibleAsync())
                    {
                        continue;
                    }

                    var tx = (await el.InnerTextAsync()).Trim();
                    if (tx.Equals(label, StringComparison.OrdinalIgnoreCase)
                        || tx.Equals(shortCode, StringComparison.OrdinalIgnoreCase))
                    {
                        await el.ClickAsync(new LocatorClickOptions { Timeout = 3_000, Force = true });
                        progress?.Report($"Beta gender: {label} (list)");
                        return true;
                    }
                }

                _ = opened;
            }
            catch { /* next */ }

            var picked = await page.EvaluateAsync<string>("""
                (label) => {
                  const want = (label || '').toLowerCase();
                  const code = want.startsWith('f') ? 'f' : want.startsWith('t') ? 't' : 'm';
                  const nodes = Array.from(document.querySelectorAll(
                    'li, [role="option"], .p-dropdown-item, .ui-dropdown-item, span, div, option'));
                  for (const el of nodes) {
                    const t = (el.textContent || '').replace(/\s+/g, ' ').trim();
                    if (!t || t.length > 24) continue;
                    if (t.toLowerCase() === want || t.toLowerCase() === code
                        || (code === 'm' && t === 'Male') || (code === 'f' && t === 'Female')) {
                      el.click();
                      return JSON.stringify({ ok: true, t });
                    }
                  }
                  return JSON.stringify({ ok: false });
                }
                """, label);

            if (picked?.Contains("\"ok\":true") == true)
            {
                progress?.Report($"Beta gender: {label} (dom)");
                return true;
            }
        }
        catch (Exception ex)
        {
            progress?.Report($"Beta gender note: {ex.Message}");
        }

        progress?.Report($"Beta: gender still not selected ({label}).");
        return false;
    }

    private static async Task SelectBetaDropdownByTextAsync(
        IPage page,
        string value,
        string hint,
        IProgress<string>? progress)
    {
        try
        {
            await page.EvaluateAsync("""
                ({ value, hint }) => {
                  const nodes = Array.from(document.querySelectorAll('select, [role="combobox"], button, div, span'));
                  for (const el of nodes) {
                    const t = ((el.getAttribute('aria-label') || '') + ' ' + (el.textContent || '')).toLowerCase();
                    if (t.includes(hint) && t.length < 80) {
                      el.click();
                      break;
                    }
                  }
                  const opts = Array.from(document.querySelectorAll('li, option, span, div, button'));
                  for (const o of opts) {
                    const tx = (o.textContent || '').replace(/\s+/g, ' ').trim();
                    if (tx.toLowerCase() === value.toLowerCase() || tx.toLowerCase().includes(value.toLowerCase())) {
                      o.click();
                      return true;
                    }
                  }
                  return false;
                }
                """, new { value, hint });
            progress?.Report($"Beta {hint}: {value}");
        }
        catch
        {
            // optional
        }
    }

    private static async Task<bool> ClickBetaPassengerSaveAsync(IPage page)
    {
        // Prefer footer primary button inside Add Passenger dialog (not the title text)
        var json = await page.EvaluateAsync<string>("""
            () => {
              const dialogs = Array.from(document.querySelectorAll('[role="dialog"], .p-dialog, .ui-dialog, .modal'));
              const root = dialogs.find(d => /add\s*passenger/i.test(d.textContent || '')) || dialogs[0] || document.body;
              const buttons = Array.from(root.querySelectorAll('button'));
              // Prefer explicit actions at the bottom
              const prefer = [/add\s*passenger/i, /^add$/i, /^save$/i, /^done$/i, /^submit$/i, /^ok$/i];
              for (const re of prefer) {
                for (let i = buttons.length - 1; i >= 0; i--) {
                  const el = buttons[i];
                  const t = (el.textContent || '').replace(/\s+/g, ' ').trim();
                  if (!re.test(t) || t.length > 40) continue;
                  // skip disabled
                  if (el.disabled || el.getAttribute('aria-disabled') === 'true') continue;
                  el.scrollIntoView({ block: 'center' });
                  el.click();
                  return JSON.stringify({ ok: true, t });
                }
              }
              const primary = root.querySelector('button.btn-primary, button[type="submit"]');
              if (primary && !primary.disabled) {
                primary.click();
                return JSON.stringify({ ok: true, t: 'primary' });
              }
              return JSON.stringify({ ok: false });
            }
            """);
        return json?.Contains("\"ok\":true") == true;
    }

    private static async Task SelectBetaPaymentAndContinueAsync(
        IPage page,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Manual beta flow: pick payment option → Calculate Fare (ONCE) → Continue To Payment (ONCE)
        // IRCTC session dies on double Calculate Fare — never retry that click.
        var wantBhim = MapGatewayToPaymentType(config.PaymentMethod)
            .Contains("BHIM", StringComparison.OrdinalIgnoreCase)
            || MapGatewayToPaymentType(config.PaymentMethod)
                .Contains("UPI", StringComparison.OrdinalIgnoreCase);

        progress?.Report(wantBhim
            ? "Beta: selecting Pay through BHIM/UPI..."
            : "Beta: selecting Cards / Net Banking payment option...");

        var radioOk = false;
        // Listen BEFORE clicking the radio — fee / Akamai sensor POST must finish before Calculate Fare
        var settleResponseTask = page.WaitForResponseAsync(
            IsBetaPaymentOptionSettleResponse,
            new PageWaitForResponseOptions { Timeout = 12_000 });

        radioOk = await SelectBetaPassengerPaymentRadioAsync(page, wantBhim, progress);
        if (!radioOk)
        {
            progress?.Report("Beta: retrying payment option selection...");
            // Restart listener for retry click
            settleResponseTask = page.WaitForResponseAsync(
                IsBetaPaymentOptionSettleResponse,
                new PageWaitForResponseOptions { Timeout = 12_000 });
            await page.WaitForTimeoutAsync(500);
            radioOk = await SelectBetaPassengerPaymentRadioAsync(page, wantBhim, progress);
        }

        if (!radioOk)
        {
            try { await settleResponseTask; } catch { /* ignore — radio failed */ }
            progress?.Report(
                "ACTION REQUIRED: Click 'Pay through BHIM/UPI' (or your payment option) once — bot will wait, then Calculate Fare.");
            try
            {
                // Wait until user selects something / Continue To Payment appears after they calculate
                await page.GetByRole(AriaRole.Button, new()
                {
                    NameRegex = new System.Text.RegularExpressions.Regex(
                        "Continue To Payment",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                }).First.WaitForAsync(new LocatorWaitForOptions { Timeout = 180_000 });
                progress?.Report("Beta: Continue To Payment visible after manual payment select.");
            }
            catch (TimeoutException)
            {
                progress?.Report("Beta: payment option still not selected — stopping before Calculate Fare.");
                return;
            }
        }
        else
        {
            progress?.Report("Beta: waiting for post-payment network settle (fee / sensor)...");
            await WaitForBetaPaymentOptionSettledAsync(page, settleResponseTask, progress);

            if (await IsSessionErrorPageAsync(page))
            {
                await ReportBetaActivityAuditAsync(page, progress, "before Calculate Fare");
                return;
            }

            // Already past Calculate Fare?
            if (await page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("Continue To Payment", System.Text.RegularExpressions.RegexOptions.IgnoreCase) }).CountAsync() > 0
                && await page.GetByRole(AriaRole.Button, new() { NameRegex = new System.Text.RegularExpressions.Regex("Continue To Payment", System.Text.RegularExpressions.RegexOptions.IgnoreCase) }).First.IsVisibleAsync())
            {
                progress?.Report("Beta: Continue To Payment already visible — skipping Calculate Fare.");
            }
            else
            {
                progress?.Report("Beta: Calculate Fare — single click, no retry...");
                var calcOk = await ClickBetaCriticalButtonOnceAsync(page, "Calculate Fare", progress);
                if (!calcOk)
                {
                    progress?.Report("ACTION REQUIRED: Click Calculate Fare once yourself (bot will not retry).");
                    try
                    {
                        await page.GetByRole(AriaRole.Button, new()
                        {
                            NameRegex = new System.Text.RegularExpressions.Regex(
                                "Continue To Payment",
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                        }).First.WaitForAsync(new LocatorWaitForOptions { Timeout = 180_000 });
                    }
                    catch (TimeoutException)
                    {
                        progress?.Report("Calculate Fare / Continue To Payment not reached.");
                        return;
                    }
                }
                else
                {
                    // Do NOT click Calculate Fare again — only wait for result or session error
                    for (var i = 0; i < 30; i++)
                    {
                        if (await IsSessionErrorPageAsync(page))
                        {
                            progress?.Report(
                                "IRCTC killed session right after Calculate Fare "
                                + "(often treated as double-submit). Close all other IRCTC logins and retry.");
                            return;
                        }

                        try
                        {
                            var cont = page.GetByRole(AriaRole.Button, new()
                            {
                                NameRegex = new System.Text.RegularExpressions.Regex(
                                    "Continue To Payment",
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase)
                            }).First;
                            if (await cont.CountAsync() > 0 && await cont.IsVisibleAsync())
                            {
                                progress?.Report("Beta: fare OK — Continue To Payment visible.");
                                break;
                            }
                        }
                        catch
                        {
                            // keep waiting
                        }

                        await page.WaitForTimeoutAsync(500);
                    }
                }
            }
        }

        if (await IsSessionErrorPageAsync(page))
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        await page.WaitForTimeoutAsync(400);

        // --- Continue To Payment (once, never retry on exception after click) ---
        progress?.Report("Beta: Continue To Payment — single click, no retry...");
        var urlBefore = page.Url;
        var contClicked = await ClickBetaCriticalButtonOnceAsync(page, "Continue To Payment", progress);

        if (!contClicked)
        {
            progress?.Report("ACTION REQUIRED: Click Continue To Payment once yourself.");
            try
            {
                await page.WaitForURLAsync(
                    url => url.Contains("payment", StringComparison.OrdinalIgnoreCase),
                    new PageWaitForURLOptions { Timeout = 180_000 });
            }
            catch (TimeoutException)
            {
                progress?.Report("Still on review booking page.");
            }

            return;
        }

        try
        {
            await page.WaitForURLAsync(
                url => url.Contains("payment", StringComparison.OrdinalIgnoreCase)
                       || url.Contains("error", StringComparison.OrdinalIgnoreCase)
                       || url != urlBefore,
                new PageWaitForURLOptions { Timeout = 45_000 });
            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report("Session error after Continue To Payment.");
                return;
            }

            await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
            await page.WaitForTimeoutAsync(1200);
            progress?.Report("Beta: payment options page loaded.");
        }
        catch
        {
            progress?.Report("Beta: waiting for payment page...");
            await page.WaitForTimeoutAsync(2500);
        }
    }

    /// <summary>
    /// Matches XHR/fetch after selecting BHIM/UPI (fee update and/or Akamai sensor).
    /// Must finish before Calculate Fare or IRCTC may kill the session.
    /// </summary>
    private static bool IsBetaPaymentOptionSettleResponse(IResponse response)
    {
        try
        {
            var url = response.Url ?? string.Empty;
            if (url.Contains(".js", StringComparison.OrdinalIgnoreCase)
                || url.Contains(".css", StringComparison.OrdinalIgnoreCase)
                || url.Contains(".png", StringComparison.OrdinalIgnoreCase)
                || url.Contains(".woff", StringComparison.OrdinalIgnoreCase)
                || url.Contains("google", StringComparison.OrdinalIgnoreCase)
                || url.Contains("gstatic", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            var status = response.Status;
            if (status is < 200 or >= 400)
            {
                return false;
            }

            // Akamai Bot Manager / sensor path observed on IRCTC
            if (url.Contains("FJLVHkB", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var method = response.Request?.Method ?? "";
            if (!method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                && !method.Equals("PUT", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (!url.Contains("irctc.co.in", StringComparison.OrdinalIgnoreCase))
            {
                return false;
            }

            if (url.Contains("eticket", StringComparison.OrdinalIgnoreCase)
                || url.Contains("booking", StringComparison.OrdinalIgnoreCase)
                || url.Contains("fare", StringComparison.OrdinalIgnoreCase)
                || url.Contains("payment", StringComparison.OrdinalIgnoreCase)
                || url.Contains("convenience", StringComparison.OrdinalIgnoreCase)
                || url.Contains("/api/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            try
            {
                var path = new Uri(url).AbsolutePath.Trim('/');
                if (path.Length is >= 6 and <= 24 && path.All(char.IsLetterOrDigit))
                {
                    return true;
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// After payment radio: await settle response (listener started before click), then Calculate Fare enabled.
    /// </summary>
    private static async Task WaitForBetaPaymentOptionSettledAsync(
        IPage page,
        Task<IResponse> settleResponseTask,
        IProgress<string>? progress)
    {
        try
        {
            var resp = await settleResponseTask;
            progress?.Report(
                $"Beta: settle response {resp.Status} {TruncateUrl(resp.Url)} — ok to Calculate Fare.");
        }
        catch (TimeoutException)
        {
            progress?.Report("Beta: no settle response in 12s — short fallback wait...");
            await page.WaitForTimeoutAsync(1200);
        }
        catch (Exception ex)
        {
            progress?.Report($"Beta: settle wait note: {ex.Message} — short fallback...");
            await page.WaitForTimeoutAsync(1000);
        }

        await page.WaitForTimeoutAsync(400);

        var calc = page.Locator("button")
            .Filter(new LocatorFilterOptions { HasText = "Calculate Fare" })
            .First;
        try
        {
            await calc.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 8_000
            });

            for (var i = 0; i < 12; i++)
            {
                if (await IsSessionErrorPageAsync(page))
                {
                    return;
                }

                try
                {
                    if (await calc.IsEnabledAsync())
                    {
                        await page.WaitForTimeoutAsync(250);
                        progress?.Report("Beta: Calculate Fare ready — clicking soon.");
                        return;
                    }
                }
                catch
                {
                    // keep trying
                }

                await page.WaitForTimeoutAsync(120);
            }

            progress?.Report("Beta: Calculate Fare still settling — clicking anyway.");
            await page.WaitForTimeoutAsync(250);
        }
        catch (Exception ex)
        {
            progress?.Report($"Beta: wait-for-Calculate-Fare note: {ex.Message}");
            await page.WaitForTimeoutAsync(600);
        }
    }

    /// <summary>
    /// Passenger page "Payment Options": select Pay through BHIM/UPI (or Cards).
    /// Must succeed before Calculate Fare — clicking fare with no option selected / wrong option kills session.
    /// </summary>
    private static async Task<bool> SelectBetaPassengerPaymentRadioAsync(
        IPage page,
        bool wantBhim,
        IProgress<string>? progress)
    {
        try
        {
            // Wait for Payment Options section (same place as manual booking)
            try
            {
                await page.GetByText("Payment Options", new() { Exact = false }).First
                    .WaitForAsync(new LocatorWaitForOptions
                    {
                        State = WaitForSelectorState.Visible,
                        Timeout = 30_000
                    });
            }
            catch
            {
                progress?.Report("Beta: 'Payment Options' heading not found yet — scrolling...");
            }

            await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
            await page.WaitForTimeoutAsync(500);

            // Prefer exact visible line: "Pay through BHIM/UPI"
            if (wantBhim)
            {
                try
                {
                    // Narrowest row that mentions BHIM/UPI but not the Cards row
                    var option = page.Locator("label, div, li, span, p")
                        .Filter(new LocatorFilterOptions { HasText = "Pay through BHIM/UPI" })
                        .Filter(new LocatorFilterOptions { HasNotText = "Credit & Debit" })
                        .Filter(new LocatorFilterOptions { HasNotText = "Net Banking" })
                        .Last;

                    if (await option.CountAsync() > 0)
                    {
                        await option.ScrollIntoViewIfNeededAsync();
                        await page.WaitForTimeoutAsync(300);
                        // Click radio inside if present, else the option row
                        var radio = option.Locator("input[type='radio']").First;
                        if (await radio.CountAsync() > 0)
                        {
                            await radio.ClickAsync(new LocatorClickOptions { Force = true, Timeout = 5_000 });
                        }
                        else
                        {
                            await option.ClickAsync(new LocatorClickOptions { Force = true, Timeout = 5_000 });
                        }

                        await page.WaitForTimeoutAsync(400);
                        progress?.Report("Beta: clicked Pay through BHIM/UPI.");
                    }
                }
                catch (Exception ex)
                {
                    progress?.Report($"Beta: Playwright BHIM click note: {ex.Message}");
                }
            }

            // DOM fallback + Angular-friendly check
            var selected = await page.EvaluateAsync<string>("""
                (wantBhim) => {
                  const norm = (s) => (s || '').replace(/\s+/g, ' ').trim();
                  const isBhim = (t) => /pay\s*through\s*bhim\s*\/\s*upi/i.test(t)
                    && !/credit\s*&\s*debit/i.test(t);
                  const isCards = (t) => /pay\s*through\s*credit/i.test(t)
                    || (/credit\s*&\s*debit/i.test(t) && /net banking/i.test(t));

                  const clickRadio = (input) => {
                    input.scrollIntoView({ block: 'center' });
                    input.focus();
                    input.click();
                    input.checked = true;
                    input.dispatchEvent(new Event('input', { bubbles: true }));
                    input.dispatchEvent(new Event('change', { bubbles: true }));
                    input.dispatchEvent(new MouseEvent('click', { bubbles: true, cancelable: true, view: window }));
                  };

                  // 1) Real radios
                  const radios = Array.from(document.querySelectorAll('input[type="radio"]'));
                  for (const input of radios) {
                    const label = input.closest('label')
                      || (input.id ? document.querySelector(`label[for="${input.id}"]`) : null)
                      || input.parentElement;
                    const t = norm((label && label.textContent) || '');
                    const hit = wantBhim ? isBhim(t) : isCards(t);
                    if (!hit) continue;
                    clickRadio(input);
                    return JSON.stringify({ ok: true, how: 'radio', t: t.slice(0, 100), checked: !!input.checked });
                  }

                  // 2) Click the BHIM/UPI text node (fee text makes length > 90 — do not cap at 90)
                  const nodes = Array.from(document.querySelectorAll('label, span, div, p, li'));
                  let best = null;
                  let bestLen = 1e9;
                  for (const el of nodes) {
                    const t = norm(el.textContent);
                    if (t.length < 10 || t.length > 200) continue;
                    const hit = wantBhim ? isBhim(t) : isCards(t);
                    if (!hit) continue;
                    if (t.length < bestLen) {
                      best = el;
                      bestLen = t.length;
                    }
                  }
                  if (best) {
                    const input = best.querySelector('input[type="radio"]')
                      || (best.closest('label') && best.closest('label').querySelector('input[type="radio"]'))
                      || null;
                    if (input) {
                      clickRadio(input);
                      return JSON.stringify({ ok: true, how: 'label-radio', t: norm(best.textContent).slice(0, 100), checked: !!input.checked });
                    }
                    best.scrollIntoView({ block: 'center' });
                    best.click();
                    return JSON.stringify({ ok: true, how: 'text', t: norm(best.textContent).slice(0, 100) });
                  }

                  return JSON.stringify({
                    ok: false,
                    radios: radios.length,
                    sample: nodes.filter(n => /bhim|upi|payment/i.test(n.textContent || '')).slice(0, 5).map(n => norm(n.textContent).slice(0, 80))
                  });
                }
                """, wantBhim);

            var ok = selected?.Contains("\"ok\":true") == true;
            progress?.Report(ok
                ? $"Beta: payment option selected ({selected})."
                : $"Beta: payment option NOT selected ({selected}).");
            return ok;
        }
        catch (Exception ex)
        {
            progress?.Report($"Beta payment option note: {ex.Message}");
            return false;
        }
    }

    /// <summary>
    /// Exactly one click. If Playwright throws after the click was sent, do NOT fall back to another click
    /// (that is what IRCTC treats as double-click on Calculate Fare).
    /// </summary>
    private static async Task<bool> ClickBetaCriticalButtonOnceAsync(
        IPage page,
        string buttonName,
        IProgress<string>? progress)
    {
        var btn = page.Locator("button")
            .Filter(new LocatorFilterOptions { HasText = buttonName })
            .First;

        try
        {
            await btn.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 20_000
            });
        }
        catch (TimeoutException)
        {
            progress?.Report($"Beta: button '{buttonName}' not visible.");
            return false;
        }

        // Ensure enabled
        try
        {
            for (var i = 0; i < 10; i++)
            {
                if (await btn.IsEnabledAsync())
                {
                    break;
                }

                await page.WaitForTimeoutAsync(200);
            }
        }
        catch
        {
            // continue
        }

        await btn.ScrollIntoViewIfNeededAsync();
        await page.WaitForTimeoutAsync(200);

        // Mark so we never intentionally click it twice in this method
        var clicked = false;
        try
        {
            // Native DOM click only — Playwright ClickAsync can fire pointerdown/mouseup/click
            // and Angular may treat that as a double-submit (session killer on Calculate Fare).
            await btn.EvaluateAsync("el => el.click()");
            clicked = true;
            progress?.Report($"Beta: clicked {buttonName} (once via JS).");
        }
        catch (Exception ex)
        {
            // If navigation/error started, the click may already have been accepted — DO NOT retry.
            if (clicked || await IsSessionErrorPageAsync(page)
                || page.Url.Contains("payment", StringComparison.OrdinalIgnoreCase)
                || page.Url.Contains("error", StringComparison.OrdinalIgnoreCase))
            {
                progress?.Report($"Beta: {buttonName} click may have already fired ({ex.Message}). Not retrying.");
                return true;
            }

            progress?.Report($"Beta: {buttonName} click failed before send: {ex.Message}");
            return false;
        }

        return true;
    }

    private static async Task<bool> ClickBetaButtonOnceAsync(
        IPage page,
        IEnumerable<string> names,
        IProgress<string>? progress)
    {
        foreach (var name in names)
        {
            if (await ClickBetaCriticalButtonOnceAsync(page, name, progress))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Beta payment page (bkgPaymentOptions): pick IRCTC iPay / provider, then Pay &amp; Book once.
    /// </summary>
    private static async Task SelectBetaPaymentGatewayAndPayAsync(
        IPage page,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report("Beta: on payment method page...");

        // If classic payment component is present, reuse classic path
        try
        {
            if (await page.Locator(IrctcSelectors.PaymentComponent).CountAsync() > 0)
            {
                await SelectPaymentGatewayAndPayAsync(page, config, progress, cancellationToken);
                return;
            }
        }
        catch
        {
            // beta layout
        }

        try
        {
            await page.GetByText("Payment Method", new() { Exact = false })
                .First
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 30_000 });
        }
        catch
        {
            progress?.Report("Beta: payment method heading not found — trying classic payment...");
            await SelectPaymentGatewayAndPayAsync(page, config, progress, cancellationToken);
            return;
        }

        var wantBhim = MapGatewayToPaymentType(config.PaymentMethod)
            .Contains("BHIM", StringComparison.OrdinalIgnoreCase)
            || MapGatewayToPaymentType(config.PaymentMethod)
                .Contains("UPI", StringComparison.OrdinalIgnoreCase);

        // Prefer IRCTC iPay (covers UPI / cards). Avoid E-Wallet ADD & PAY when balance is 0.
        progress?.Report("Beta: selecting IRCTC iPay...");
        try
        {
            await page.EvaluateAsync("""
                () => {
                  const nodes = Array.from(document.querySelectorAll('div, label, button, span, li'));
                  for (const el of nodes) {
                    const t = (el.textContent || '').replace(/\s+/g, ' ').trim();
                    if (/^IRCTC iPay$/i.test(t) || (t.includes('IRCTC iPay') && t.length < 80)) {
                      (el.closest('div[class], label, button') || el).click();
                      return true;
                    }
                  }
                  return false;
                }
                """);
            await page.WaitForTimeoutAsync(800);
        }
        catch (Exception ex)
        {
            progress?.Report($"Beta iPay note: {ex.Message}");
        }

        // Optional: click a UPI / bank tile if visible (PAYTM etc.)
        var provider = config.PaymentProvider;
        if (!string.IsNullOrWhiteSpace(provider))
        {
            try
            {
                var tile = page.GetByText(provider, new() { Exact = false }).First;
                if (await tile.CountAsync() > 0 && await tile.IsVisibleAsync())
                {
                    await tile.ClickAsync(new LocatorClickOptions { Timeout = 3_000 });
                    progress?.Report($"Beta: clicked provider {provider}.");
                    await page.WaitForTimeoutAsync(500);
                }
            }
            catch
            {
                // optional
            }
        }

        _ = wantBhim;

        progress?.Report("Beta: clicking Pay & Book (once)...");
        var paid = await ClickBetaButtonOnceAsync(page, new[] { "Pay & Book", "Pay and Book" }, progress);
        if (!paid)
        {
            // Fall back to classic pay selectors
            try
            {
                var pay = page.Locator(IrctcSelectors.PayAndBookButton).First;
                if (await pay.CountAsync() > 0 && await pay.IsVisibleAsync())
                {
                    await pay.ClickAsync(new LocatorClickOptions { Timeout = 8_000, ClickCount = 1 });
                    paid = true;
                    progress?.Report("Beta: Pay & Book clicked (classic selector).");
                }
            }
            catch
            {
                // ignore
            }
        }

        if (!paid)
        {
            progress?.Report("ACTION REQUIRED: Select payment method and click Pay & Book once.");
            await page.WaitForTimeoutAsync(120_000);
        }
        else
        {
            await page.WaitForTimeoutAsync(2000);
        }
    }

    private static async Task DismissLanguagePopupAsync(IPage page, IProgress<string>? progress)
    {
        await page.WaitForTimeoutAsync(2500);
        try
        {
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
                await page.WaitForTimeoutAsync(1000);
            }
        }
        catch
        {
            // ignore
        }
    }

    private static async Task WaitForScheduledSearchAsync(
        string scheduledTime,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(scheduledTime))
        {
            return;
        }

        if (!TimeSpan.TryParse(scheduledTime.Trim(), out var target))
        {
            progress?.Report($"Invalid schedule time '{scheduledTime}' — searching immediately.");
            return;
        }

        var now = DateTime.Now;
        var runAt = now.Date.Add(target);
        if (runAt <= now)
        {
            progress?.Report($"Scheduled time {scheduledTime} already passed — searching now.");
            return;
        }

        progress?.Report($"Waiting until {runAt:HH:mm:ss} to click Search (Tatkal timing)...");
        while (DateTime.Now < runAt)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var remaining = runAt - DateTime.Now;
            if (remaining.TotalSeconds <= 10 || (int)remaining.TotalSeconds % 15 == 0)
            {
                progress?.Report($"Search starts in {remaining:mm\\:ss}...");
            }

            await Task.Delay(250, cancellationToken);
        }

        progress?.Report("Scheduled time reached — searching now!");
    }

    private static async Task FillJourneySearchAsync(
        IPage page,
        TrainSearchSettings settings,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report("Filling train search details...");

        var fromInput = page.Locator(".ui-autocomplete-input").First;
        await fromInput.ClickAsync();
        await fromInput.FillAsync("");
        await fromInput.PressSequentiallyAsync(settings.FromStationCode, new LocatorPressSequentiallyOptions { Delay = 80 });
        await page.WaitForTimeoutAsync(1500);
        await SelectStationOptionAsync(page, settings.FromStationCode);
        progress?.Report($"Selected From: {settings.FromStationCode}");
        await page.WaitForTimeoutAsync(400);
        cancellationToken.ThrowIfCancellationRequested();

        var toInput = page.Locator(".ui-autocomplete-input").Nth(1);
        await toInput.ClickAsync();
        await toInput.FillAsync("");
        await toInput.PressSequentiallyAsync(settings.ToStationCode, new LocatorPressSequentiallyOptions { Delay = 80 });
        await page.WaitForTimeoutAsync(1500);
        await SelectStationOptionAsync(page, settings.ToStationCode);
        progress?.Report($"Selected To: {settings.ToStationCode}");
        await page.WaitForTimeoutAsync(400);

        var dateString = settings.TravelDate.ToString("dd/MM/yyyy");
        var dateInput = page.Locator(IrctcSelectors.JourneyDate).First;
        await dateInput.ClickAsync();
        await page.Keyboard.PressAsync("Control+A");
        await page.Keyboard.PressAsync("Backspace");
        await dateInput.PressSequentiallyAsync(dateString, new LocatorPressSequentiallyOptions { Delay = 60 });
        await page.Keyboard.PressAsync("Escape");
        await page.WaitForTimeoutAsync(300);

        await SelectQuotaAsync(page, settings.Quota, progress);
        cancellationToken.ThrowIfCancellationRequested();

        progress?.Report("Clicking Search Trains...");
        var searchClicked = await page.EvaluateAsync<bool>("""
            () => {
                const buttons = document.querySelectorAll('button');
                for (const btn of buttons) {
                    const text = btn.textContent.trim();
                    if (text === 'Search' || text === 'Search Trains' || text.includes('Search')) {
                        if (btn.type === 'submit' || btn.classList.contains('search_btn')) {
                            btn.click();
                            return true;
                        }
                    }
                }
                for (const btn of buttons) {
                    if (btn.textContent.trim().includes('Search')) {
                        btn.click();
                        return true;
                    }
                }
                return false;
            }
            """);

        if (!searchClicked)
        {
            await page.Locator(IrctcSelectors.SearchButton).First.ClickAsync();
        }

        try
        {
            await page.WaitForURLAsync(
                url => url.Contains("train-list", StringComparison.OrdinalIgnoreCase),
                new PageWaitForURLOptions { Timeout = 30_000 });
        }
        catch (TimeoutException)
        {
            progress?.Report("Still on search page — waiting for results...");
        }

        try
        {
            await page.Locator("app-train-list, app-train-avl-enq, .bull-back")
                .First
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 30_000 });
        }
        catch (TimeoutException)
        {
            progress?.Report("Train list slow to appear...");
        }

        await page.WaitForTimeoutAsync(1500);
    }

    private static async Task SelectStationOptionAsync(IPage page, string stationCode)
    {
        var option = page.Locator("li").Filter(new LocatorFilterOptions { HasText = $"- {stationCode}" }).First;
        if (await option.CountAsync() > 0 && await option.IsVisibleAsync())
        {
            await option.ClickAsync();
            return;
        }

        await page.Keyboard.PressAsync("ArrowDown");
        await page.Keyboard.PressAsync("Enter");
    }

    private static async Task SelectQuotaAsync(IPage page, string quotaCode, IProgress<string>? progress)
    {
        var label = IrctcQuotaLabels.ToDisplayLabel(quotaCode);
        try
        {
            var quotaDropdown = page.Locator("#journeyQuota").First;
            if (await quotaDropdown.CountAsync() == 0)
            {
                return;
            }

            await quotaDropdown.ClickAsync();
            await page.WaitForTimeoutAsync(400);

            var item = page.Locator("#journeyQuota li[role='option'], #journeyQuota p-dropdownitem span")
                .Filter(new LocatorFilterOptions { HasText = label })
                .First;

            if (await item.CountAsync() > 0)
            {
                await item.ClickAsync();
                progress?.Report($"Quota set to {label} ({quotaCode}).");
            }
            else
            {
                await page.Keyboard.PressAsync("Escape");
                progress?.Report($"Quota '{label}' not found — leaving default.");
            }
        }
        catch (Exception ex)
        {
            progress?.Report($"Quota selection note: {ex.Message}");
        }
    }

    private static async Task HandleNoDirectTrainsDialogAsync(IPage page, IProgress<string>? progress)
    {
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
                progress?.Report("No direct trains dialog — clicked No. Try a different route.");
            }
        }
        catch
        {
            // ignore
        }
    }

    /// <summary>
    /// IRCTC list UI: find train via .train-heading → class (.pre-avl) → date card → Book Now.
    /// Aligned with chrome-extension bookTicket.js findRootTrain / select class / date.
    /// </summary>
    private static async Task<bool> SelectTrainClassAndBookAsync(
        IPage page,
        TrainResult selectedTrain,
        TrainSearchSettings settings,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var preferredClass = NormalizeClassCode(
            string.IsNullOrWhiteSpace(settings.PreferredClass)
                ? config.PreferredClass
                : settings.PreferredClass);
        var trainNum = NormalizeTrainNumber(selectedTrain.TrainNumber);
        // IRCTC date cards look like "Wed, 22 Jul" — match day + English month
        var dateHint = settings.TravelDate.ToString("d MMM", System.Globalization.CultureInfo.InvariantCulture);
        var dateDay = settings.TravelDate.Day.ToString(System.Globalization.CultureInfo.InvariantCulture);
        var dateMonth = settings.TravelDate.ToString("MMM", System.Globalization.CultureInfo.InvariantCulture);

        if (string.IsNullOrWhiteSpace(trainNum))
        {
            progress?.Report("ERROR: Train number is empty.");
            return false;
        }

        progress?.Report($"Waiting for train list / {trainNum} (class {preferredClass}, date {dateHint})...");

        try
        {
            await page.Locator("app-train-avl-enq, app-train-list, .train-heading")
                .First
                .WaitForAsync(new LocatorWaitForOptions
                {
                    Timeout = 45_000,
                    State = WaitForSelectorState.Visible
                });
        }
        catch (TimeoutException)
        {
            progress?.Report("IRCTC train list did not appear. Check search stations/date.");
            return false;
        }

        await page.WaitForTimeoutAsync(800);

        // IRCTC lazy-loads cards — scroll so more trains appear before matching
        for (var scrollPass = 0; scrollPass < 6; scrollPass++)
        {
            await page.EvaluateAsync("() => window.scrollBy(0, Math.floor(window.innerHeight * 0.85))");
            await page.WaitForTimeoutAsync(350);
        }

        await page.EvaluateAsync("() => window.scrollTo(0, 0)");
        await page.WaitForTimeoutAsync(400);

        // Return JSON string — Playwright's typed EvaluateAsync can NRE on custom POCOs
        var foundJson = await page.EvaluateAsync<string>("""
            (trainNum) => {
                const headings = Array.from(document.querySelectorAll('app-train-avl-enq .train-heading, .train-heading'));
                const visible = [];
                const pushVisible = (text) => {
                    if (!text) return;
                    const reParen = /\((\d{3,5})\)/g;
                    let m;
                    while ((m = reParen.exec(text)) !== null) {
                        visible.push(m[1]);
                    }
                    const bare = text.match(/\b(\d{5})\b/g);
                    if (bare) {
                        for (let i = 0; i < bare.length; i++) visible.push(bare[i]);
                    }
                };

                for (let i = 0; i < headings.length; i++) {
                    const h = headings[i];
                    const text = (h.textContent || '').replace(/\s+/g, ' ').trim();
                    pushVisible(text);
                    const digits = text.replace(/\D/g, '');
                    if (digits.indexOf(trainNum) >= 0 || text.indexOf(trainNum) >= 0 || text.indexOf('(' + trainNum + ')') >= 0) {
                        const root = h.closest('app-train-avl-enq') || h.closest('.bull-back') || h.parentElement;
                        if (root) {
                            root.scrollIntoView({ block: 'center', behavior: 'instant' });
                            return JSON.stringify({ ok: true, heading: text.slice(0, 80), visible: [] });
                        }
                    }
                }

                const cards = Array.from(document.querySelectorAll('app-train-avl-enq'));
                for (let i = 0; i < cards.length; i++) {
                    const el = cards[i];
                    const t = (el.textContent || '');
                    pushVisible(t);
                    const digitTokens = t.replace(/\D/g, ' ').split(/\s+/).filter(Boolean);
                    if (t.indexOf('(' + trainNum + ')') >= 0 ||
                        new RegExp('(?:^|\\D)' + trainNum + '(?:\\D|$)').test(t) ||
                        digitTokens.indexOf(trainNum) >= 0) {
                        el.scrollIntoView({ block: 'center', behavior: 'instant' });
                        return JSON.stringify({ ok: true, heading: 'fallback-root', visible: [] });
                    }
                }

                pushVisible((document.body && document.body.innerText) ? document.body.innerText : '');
                const unique = [];
                const seen = {};
                for (let i = 0; i < visible.length && unique.length < 25; i++) {
                    const v = visible[i];
                    if (v && !seen[v]) {
                        seen[v] = true;
                        unique.push(v);
                    }
                }
                return JSON.stringify({ ok: false, heading: '', visible: unique });
            }
            """, trainNum);

        TrainFindResult? found = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(foundJson))
            {
                found = System.Text.Json.JsonSerializer.Deserialize<TrainFindResult>(foundJson);
            }
        }
        catch
        {
            found = null;
        }

        if (found is null || !found.Ok)
        {
            var shown = found?.Visible is { Count: > 0 }
                ? string.Join(", ", found.Visible)
                : "(none parsed)";
            progress?.Report(
                $"Train {trainNum} not found on IRCTC list. Visible train Nos: {shown}. "
                + "Stations/date/quota may differ from Find results — pick the train manually, then Book Now.");
            return false;
        }

        progress?.Report($"Found train {trainNum} ({found.Heading}). Selecting class {preferredClass}...");
        await page.WaitForTimeoutAsync(400);

        var classClicked = await ClickTrainClassAsync(page, trainNum, preferredClass, progress);
        if (!classClicked)
        {
            progress?.Report($"Could not click class {preferredClass}. Click it manually in the browser.");
        }
        else
        {
            progress?.Report($"Clicked class {preferredClass}. Waiting for availability...");
        }

        try
        {
            await page.WaitForFunctionAsync(
                """
                (trainNum) => {
                    const roots = [...document.querySelectorAll('app-train-avl-enq')];
                    let root = roots.find(el => {
                        const t = el.textContent || '';
                        return t.includes(trainNum) || t.replace(/\D/g, ' ').split(/\s+/).includes(trainNum);
                    });
                    if (!root) return false;
                    const text = root.textContent || '';
                    const hasStatus = /AVAILABLE|\bWL\s*\d*|\bRAC\b|REGRET|WAITING/i.test(text);
                    const hasDate = /\d{1,2}\s*,?\s*(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)/i.test(text);
                    const hasTabs = root.querySelectorAll('li[role="tab"], .ui-tabview-nav li, p-tabmenu li').length >= 1;
                    return (hasStatus && hasDate) || (hasTabs && hasStatus);
                }
                """,
                trainNum,
                new PageWaitForFunctionOptions { Timeout = 25_000 });
            progress?.Report("Availability loaded.");
        }
        catch (TimeoutException)
        {
            progress?.Report("Availability slow — re-clicking class...");
            await ClickTrainClassAsync(page, trainNum, preferredClass, progress);
            await page.WaitForTimeoutAsync(2500);
        }

        await page.WaitForTimeoutAsync(600);
        progress?.Report($"Selecting travel date ({dateHint}) then Book Now (one click only)...");

        // Main-style: click date once → wait → Book Now once. Never re-click tabs after that
        // (IRCTC "Sorry!!! Please Try again" is usually double-click / Refresh / tab spam).
        var deadline = DateTime.UtcNow.AddSeconds(Math.Max(60, config.AvailabilityTimeoutSeconds));
        var attempt = 0;
        var bookNowAttempted = false;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;

            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report(SessionErrorMessage());
                return false;
            }

            // Already navigated away from list — treat as success for login wait
            if (!page.Url.Contains("train-list", StringComparison.OrdinalIgnoreCase))
            {
                progress?.Report("Left train list — continuing to login.");
                return true;
            }

            if (bookNowAttempted)
            {
                // Do not click anything else; wait for login / navigation
                if (await page.Locator("app-login, input[formcontrolname='userid']").CountAsync() > 0)
                {
                    progress?.Report("Login popup opened.");
                    return true;
                }

                await page.WaitForTimeoutAsync(1000);
                continue;
            }

            var dateClicked = await TryClickTravelDateCardAsync(
                page, trainNum, dateHint, dateDay, dateMonth, config.ConfirmBerthsOnly, progress);

            if (!dateClicked)
            {
                progress?.Report($"Attempt {attempt}: waiting for date card...");
                await page.WaitForTimeoutAsync(1200);
                continue;
            }

            progress?.Report($"Selected date ({dateHint}). Waiting for Book Now...");
            await page.WaitForTimeoutAsync(1500);

            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report(SessionErrorMessage());
                return false;
            }

            bookNowAttempted = true;
            if (await TryClickBookNowOnceAsync(page, trainNum, progress))
            {
                return true;
            }

            // Book Now click done once — never retry Book Now / tabs (causes session error)
            progress?.Report("Book Now was clicked once. If login did not open, click Book Now manually (once only).");
            await page.WaitForTimeoutAsync(2000);
        }

        progress?.Report("Timed out on train list. Manually: date → Book Now (single click).");
        return false;
    }

    /// <summary>
    /// Click IRCTC date availability card (.pre-avl with "Wed, 22 Jul" + WL/AVAILABLE).
    /// Uses Playwright locator clicks so Angular change detection enables Book Now.
    /// </summary>
    private static async Task<bool> TryClickTravelDateCardAsync(
        IPage page,
        string trainNum,
        string dateHint,
        string dateDay,
        string dateMonth,
        bool confirmOnly,
        IProgress<string>? progress)
    {
        try
        {
            var root = page.Locator("app-train-avl-enq")
                .Filter(new LocatorFilterOptions { HasText = trainNum })
                .First;

            if (await root.CountAsync() == 0)
            {
                return false;
            }

            await root.ScrollIntoViewIfNeededAsync();

            var dateCards = root.Locator(".pre-avl");
            var n = await dateCards.CountAsync();
            ILocator? exact = null;
            ILocator? available = null;
            ILocator? anyOk = null;

            for (var i = 0; i < n; i++)
            {
                var card = dateCards.Nth(i);
                string text;
                try
                {
                    text = (await card.InnerTextAsync()).Replace('\n', ' ');
                }
                catch
                {
                    continue;
                }

                if (text.Contains("REGRET", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Must look like a date availability card, not a class tab "AC 3 Tier (3A)"
                var hasMonthDate = System.Text.RegularExpressions.Regex.IsMatch(
                    text,
                    @"\d{1,2}\s*,?\s*(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                var hasStatus = System.Text.RegularExpressions.Regex.IsMatch(
                    text,
                    @"AVAILABLE|RAC|\bWL\d*\b|WAITING",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (!hasMonthDate || !hasStatus)
                {
                    continue;
                }

                if (confirmOnly && !text.Contains("AVAILABLE", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                // Prefer "22 Jul" / "Wed, 22 Jul" — never bare "22" (matches WL22)
                var matchesTravelDate =
                    text.Contains(dateHint, StringComparison.OrdinalIgnoreCase)
                    || System.Text.RegularExpressions.Regex.IsMatch(
                        text,
                        $@"\b{System.Text.RegularExpressions.Regex.Escape(dateDay)}\s*,?\s*{System.Text.RegularExpressions.Regex.Escape(dateMonth)}\b",
                        System.Text.RegularExpressions.RegexOptions.IgnoreCase);

                if (matchesTravelDate)
                {
                    exact = card;
                    break;
                }

                if (text.Contains("AVAILABLE", StringComparison.OrdinalIgnoreCase))
                {
                    available ??= card;
                }

                anyOk ??= card;
            }

            var pick = exact ?? available ?? anyOk;
            if (pick is null)
            {
                if (confirmOnly)
                {
                    progress?.Report(
                        "No AVAILABLE date cards (only WL/RAC). Uncheck 'Book only if confirm berths' to continue.");
                }

                return false;
            }

            await pick.ClickAsync(new LocatorClickOptions { Force = true, Timeout = 5_000 });
            return true;
        }
        catch (Exception ex)
        {
            progress?.Report($"Date click note: {ex.Message}");
            return false;
        }
    }

    private static async Task<bool> ClickTrainClassAsync(
        IPage page,
        string trainNum,
        string classCode,
        IProgress<string>? progress)
    {
        // If date cards already visible for this class, do not re-click Refresh (keeps current avl).
        var alreadyOpen = await page.EvaluateAsync<bool>("""
            ({ trainNum, classCode }) => {
                const roots = Array.from(document.querySelectorAll('app-train-avl-enq'));
                const root = roots.find(el => (el.textContent || '').indexOf(trainNum) >= 0);
                if (!root) return false;
                const text = root.textContent || '';
                const hasDates = /\d{1,2}\s*,?\s*(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)/i.test(text)
                    && /AVAILABLE|RAC|\bWL\d*\b|WAITING/i.test(text);
                const tabSelected = Array.from(root.querySelectorAll('li[role="tab"][aria-selected="true"], .ui-state-active'))
                    .some(el => new RegExp('\\b' + classCode + '\\b', 'i').test(el.textContent || '')
                        || (el.textContent || '').indexOf('(' + classCode + ')') >= 0);
                return hasDates && (tabSelected || new RegExp('\\(' + classCode + '\\)').test(text));
            }
            """, new { trainNum, classCode });

        if (alreadyOpen)
        {
            progress?.Report($"Class {classCode} already open with dates — skipping Refresh.");
            return true;
        }

        var clicked = await page.EvaluateAsync<bool>("""
            ({ trainNum, classCode }) => {
                const roots = Array.from(document.querySelectorAll('app-train-avl-enq'));
                const root = roots.find(el => {
                    const t = el.textContent || '';
                    return t.indexOf(trainNum) >= 0 || t.replace(/\D/g, ' ').split(/\s+/).indexOf(trainNum) >= 0;
                });
                if (!root) return false;
                root.scrollIntoView({ block: 'center' });

                const needle = '(' + classCode + ')';
                const codeRe = new RegExp('\\b' + classCode + '\\b', 'i');

                // Prefer class tabs if present
                const tabs = Array.from(root.querySelectorAll('li[role="tab"] a, p-tabmenu a, .ui-tabview-nav a'));
                for (let i = 0; i < tabs.length; i++) {
                    const t = (tabs[i].textContent || '').replace(/\s+/g, ' ').trim();
                    if (t.indexOf(needle) >= 0 || codeRe.test(t) || (classCode === 'SL' && /sleeper/i.test(t))) {
                        tabs[i].click();
                        return true;
                    }
                }

                const boxes = Array.from(root.querySelectorAll('.pre-avl'));
                for (let i = 0; i < boxes.length; i++) {
                    const box = boxes[i];
                    const text = (box.textContent || '').replace(/\s+/g, ' ').trim();
                    const strong = ((box.querySelector('strong') || {}).textContent || '').trim();
                    const label = strong || text;
                    if (/\d{1,2}\s*,?\s*(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)/i.test(text) &&
                        /AVAILABLE|WL|RAC|REGRET|WAITING/i.test(text)) {
                        continue;
                    }
                    const matches =
                        label.indexOf(needle) >= 0 ||
                        codeRe.test(label) ||
                        (classCode === 'SL' && /sleeper/i.test(label)) ||
                        (classCode === '3A' && /3\s*tier|ac\s*3/i.test(label)) ||
                        (classCode === '2A' && /2\s*tier|ac\s*2/i.test(label)) ||
                        (classCode === '1A' && /first|1\s*tier|ac\s*first/i.test(label));

                    if (!matches) continue;

                    const refresh = Array.from(box.querySelectorAll('a, span, div, button')).find(n =>
                        /^\s*Refresh\s*$/i.test((n.textContent || '').trim()));
                    (refresh || box).click();
                    return true;
                }
                return false;
            }
            """, new { trainNum, classCode });

        if (clicked)
        {
            return true;
        }

        try
        {
            var trainRoot = page.Locator("app-train-avl-enq")
                .Filter(new LocatorFilterOptions { HasText = trainNum })
                .First;
            var classTab = trainRoot.GetByRole(AriaRole.Tab, new() { NameRegex = new System.Text.RegularExpressions.Regex(classCode, System.Text.RegularExpressions.RegexOptions.IgnoreCase) });
            if (await classTab.CountAsync() > 0)
            {
                await classTab.First.ClickAsync(new LocatorClickOptions { Force = true });
                return true;
            }

            var classBox = trainRoot.Locator(".pre-avl")
                .Filter(new LocatorFilterOptions { HasText = classCode })
                .First;
            if (await classBox.CountAsync() > 0)
            {
                await classBox.ClickAsync(new LocatorClickOptions { Force = true });
                return true;
            }
        }
        catch (Exception ex)
        {
            progress?.Report($"Class click note: {ex.Message}");
        }

        return false;
    }

    private static string NormalizeTrainNumber(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return string.Empty;
        }

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (digits.Length == 0)
        {
            return raw.Trim();
        }

        var trimmed = digits.TrimStart('0');
        return trimmed.Length > 0 ? trimmed : digits;
    }

    private static string NormalizeClassCode(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "SL";
        }

        var t = raw.Trim().ToUpperInvariant();
        var paren = System.Text.RegularExpressions.Regex.Match(t, @"\(([A-Z0-9]{1,3})\)");
        if (paren.Success)
        {
            return paren.Groups[1].Value;
        }

        foreach (var code in new[] { "3E", "3A", "2A", "1A", "SL", "CC", "EC", "2S", "EA", "FC" })
        {
            if (t == code || t.EndsWith(" " + code, StringComparison.Ordinal) || t.StartsWith(code + " ", StringComparison.Ordinal))
            {
                return code;
            }
        }

        return t.Length <= 3 ? t : "SL";
    }

    private sealed class TrainFindResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("heading")]
        public string? Heading { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("visible")]
        public List<string>? Visible { get; set; }
    }

    private static async Task<bool> TryClickBookNowOnceAsync(
        IPage page,
        string trainNum,
        IProgress<string>? progress)
    {
        var root = page.Locator("app-train-avl-enq")
            .Filter(new LocatorFilterOptions { HasText = trainNum })
            .First;

        if (await root.CountAsync() == 0)
        {
            root = page.Locator("app-train-avl-enq, .bull-back")
                .Filter(new LocatorFilterOptions { HasText = trainNum })
                .First;
        }

        var book = root.Locator("button:has-text('Book Now'), button.btnDefault.train_Search:has-text('Book')").First;
        if (await book.CountAsync() == 0)
        {
            progress?.Report("Book Now button not found.");
            return false;
        }

        // Wait until IRCTC enables Book Now after date selection (main approach)
        for (var i = 0; i < 20; i++)
        {
            var classAttr = (await book.GetAttributeAsync("class")) ?? string.Empty;
            var disabled = await book.IsDisabledAsync()
                           || classAttr.Contains("disable-book", StringComparison.OrdinalIgnoreCase);
            if (!disabled)
            {
                break;
            }

            await page.WaitForTimeoutAsync(250);
        }

        var stillDisabled = await book.IsDisabledAsync()
            || ((await book.GetAttributeAsync("class")) ?? string.Empty)
                .Contains("disable-book", StringComparison.OrdinalIgnoreCase);

        if (stillDisabled)
        {
            progress?.Report("Book Now still disabled — date may not be selected.");
            return false;
        }

        await book.ClickAsync(new LocatorClickOptions { Timeout = 5_000, Trial = false });
        // Prevent accidental second click handlers
        try
        {
            await book.EvaluateAsync("el => { el.style.pointerEvents = 'none'; }");
        }
        catch
        {
            // ignore
        }

        progress?.Report("Clicked Book Now (single click).");
        await page.WaitForTimeoutAsync(2500);

        if (await IsSessionErrorPageAsync(page))
        {
            progress?.Report(SessionErrorMessage());
            return false;
        }

        await AcceptContinueDialogsAsync(page, progress);

        if (await page.Locator("app-login, input[formcontrolname='userid']").CountAsync() > 0)
        {
            progress?.Report("Login popup opened.");
            return true;
        }

        if (!page.Url.Contains("train-list", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        try
        {
            await page.Locator("app-login, input[formcontrolname='userid']")
                .First
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 8_000 });
            progress?.Report("Login popup opened.");
            return true;
        }
        catch
        {
            progress?.Report("Still on train list after Book Now.");
            return false;
        }
    }

    private static string SessionErrorMessage() =>
        "IRCTC session error (Sorry! Please try again). Before retry: "
        + "1) Log out / close IRCTC in Chrome, app, and other tabs. "
        + "2) Do not click BOOK/LOGIN yourself while automation runs. "
        + "3) Press Stop, close the Playwright window, wait 1 min, then Book again.";

    private static async Task<bool> IsSessionErrorPageAsync(IPage page)
    {
        try
        {
            var text = await page.InnerTextAsync("body");
            return text.Contains("Sorry!!!", StringComparison.OrdinalIgnoreCase)
                   || text.Contains("Please Try again", StringComparison.OrdinalIgnoreCase)
                   || text.Contains("double clicked", StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    private static async Task HandleSessionErrorAsync(
        IPage page,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        await ReportBetaActivityAuditAsync(page, progress, "session-error handler");
        progress?.Report("Not a payment QR. Press Stop, then Book on IRCTC again with a fresh session.");
        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // stop
        }
    }

    /// <summary>
    /// Passive audit only — never intercept/prevent clicks (that broke IRCTC after Search).
    /// </summary>
    private static void AttachBetaActivityAudit(IPage page, IProgress<string>? progress)
    {
        page.Request += (_, req) =>
        {
            try
            {
                var url = req.Url ?? string.Empty;
                var method = req.Method ?? "";
                if (!method.Equals("POST", StringComparison.OrdinalIgnoreCase)
                    && !url.Contains("error", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                if (url.Contains(".js", StringComparison.OrdinalIgnoreCase)
                    || url.Contains(".css", StringComparison.OrdinalIgnoreCase)
                    || url.Contains(".png", StringComparison.OrdinalIgnoreCase)
                    || url.Contains("google", StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }

                var line = $"NET {method} {TruncateUrl(url)}";
                lock (BetaAuditFileLock)
                {
                    BetaNetEvents.Add($"{DateTime.Now:HH:mm:ss.fff} {line}");
                    if (BetaNetEvents.Count > 80)
                    {
                        BetaNetEvents.RemoveAt(0);
                    }
                }

                progress?.Report($"[IRCTC_AUDIT] {line}");
                AppendBetaAuditFile(line);
            }
            catch
            {
                // ignore audit errors
            }
        };

        page.FrameNavigated += (_, frame) =>
        {
            if (frame != page.MainFrame)
            {
                return;
            }

            var url = frame.Url ?? "";
            var line = $"NAV {TruncateUrl(url)}";
            progress?.Report($"[IRCTC_AUDIT] {line}");
            AppendBetaAuditFile(line);
        };
    }

    private static async Task ReportBetaActivityAuditAsync(
        IPage page,
        IProgress<string>? progress,
        string when)
    {
        try
        {
            progress?.Report($"[IRCTC_AUDIT] DUMP ({when}) url={page.Url}");
            AppendBetaAuditFile($"DUMP ({when}) url={page.Url}");

            string net;
            lock (BetaAuditFileLock)
            {
                net = string.Join(" | ", BetaNetEvents.TakeLast(15));
            }

            if (!string.IsNullOrWhiteSpace(net))
            {
                progress?.Report($"[IRCTC_AUDIT] RECENT_NET: {net}");
                AppendBetaAuditFile($"RECENT_NET: {net}");
            }
        }
        catch (Exception ex)
        {
            progress?.Report($"[IRCTC_AUDIT] dump failed: {ex.Message}");
        }

        await Task.CompletedTask;
    }

    private static readonly object BetaAuditFileLock = new();
    private static readonly List<string> BetaNetEvents = [];

    private static void AppendBetaAuditFile(string line)
    {
        try
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "irctc-click-audit.log");
            lock (BetaAuditFileLock)
            {
                File.AppendAllText(path, $"{DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} {line}{Environment.NewLine}");
            }
        }
        catch
        {
            // ignore file errors
        }
    }

    private static string TruncateUrl(string url)
    {
        if (string.IsNullOrEmpty(url))
        {
            return "";
        }

        try
        {
            var u = new Uri(url);
            var path = u.PathAndQuery;
            return path.Length <= 120 ? path : path[..120] + "...";
        }
        catch
        {
            return url.Length <= 120 ? url : url[..120] + "...";
        }
    }

    private sealed class BookAttemptResult
    {
        [System.Text.Json.Serialization.JsonPropertyName("ok")]
        public bool Ok { get; set; }

        [System.Text.Json.Serialization.JsonPropertyName("reason")]
        public string? Reason { get; set; }
    }

    private static async Task AcceptContinueDialogsAsync(IPage page, IProgress<string>? progress)
    {
        try
        {
            var clicked = await page.EvaluateAsync<bool>("""
                () => {
                    const body = document.body.textContent || '';
                    if (body.includes('Do you want to continue') || body.includes('confirmation')) {
                        const buttons = document.querySelectorAll('button, .ui-confirmdialog-acceptbutton');
                        for (const btn of buttons) {
                            const t = (btn.textContent || '').trim();
                            if (t === 'Yes' || t.includes('Yes') || btn.classList.contains('ui-confirmdialog-acceptbutton')) {
                                btn.click();
                                return true;
                            }
                        }
                    }
                    return false;
                }
                """);

            if (clicked)
            {
                progress?.Report("Confirmation dialog — clicked Yes.");
                await page.WaitForTimeoutAsync(800);
            }
        }
        catch
        {
            // ignore
        }
    }

    private static async Task LoginAsync(
        IPage page,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken,
        bool waitLonger = true)
    {
        var timeoutMs = waitLonger ? 180_000 : 60_000;
        progress?.Report(waitLonger
            ? "Waiting for Login popup (up to 3 min — click Book Now if needed)..."
            : "Waiting for Login popup (1 min — complete Book Now manually if needed)...");
        var userInput = page.Locator(IrctcSelectors.LoginUserId).First;

        try
        {
            await userInput.WaitForAsync(new LocatorWaitForOptions { Timeout = timeoutMs });
        }
        catch (TimeoutException)
        {
            progress?.Report("Login popup did not appear. Click Book Now in the browser, then wait.");
            await userInput.WaitForAsync(new LocatorWaitForOptions { Timeout = timeoutMs });
        }

        if (string.IsNullOrWhiteSpace(config.Credentials.Username) ||
            string.IsNullOrWhiteSpace(config.Credentials.Password))
        {
            progress?.Report("ERROR: Username/password empty. Fill Settings tab.");
            await Task.Delay(Timeout.Infinite, cancellationToken);
            return;
        }

        progress?.Report("Typing username...");
        await userInput.ClickAsync();
        await userInput.FillAsync("");
        await userInput.PressSequentiallyAsync(config.Credentials.Username, new LocatorPressSequentiallyOptions { Delay = 40 });

        progress?.Report("Typing password...");
        var passInput = page.Locator(IrctcSelectors.LoginPassword).First;
        await passInput.ClickAsync();
        await passInput.FillAsync("");
        await passInput.PressSequentiallyAsync(config.Credentials.Password, new LocatorPressSequentiallyOptions { Delay = 40 });

        try
        {
            var vi = page.Locator("label:has-text('Visually impaired'), label:has-text('Visually Impaired')").First;
            if (await vi.CountAsync() > 0 && await vi.IsVisibleAsync())
            {
                await vi.ClickAsync();
                progress?.Report("Enabled OTP login option (visually impaired).");
            }
        }
        catch
        {
            // ignore
        }

        progress?.Report("Clicking Sign In...");
        await page.Locator(IrctcSelectors.LoginSignIn).First.ClickAsync();

        progress?.Report("ACTION REQUIRED: Enter OTP or solve captcha, then Sign In if needed.");
        await page.WaitForURLAsync(
            url => !url.Contains("train-list", StringComparison.OrdinalIgnoreCase) ||
                   url.Contains("psgninput", StringComparison.OrdinalIgnoreCase),
            new PageWaitForURLOptions { Timeout = 300_000 });

        try
        {
            await page.Locator(IrctcSelectors.PassengerName).First
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 60_000 });
        }
        catch
        {
            // may already be past login
        }

        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report("Login successful.");
    }

    private static async Task FillPassengersAsync(
        IPage page,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        progress?.Report("Filling passenger details...");
        var firstName = page.Locator(IrctcSelectors.PassengerName).First;
        await firstName.WaitForAsync(new LocatorWaitForOptions { Timeout = 45_000 });
        await page.WaitForTimeoutAsync(800);

        var index = 0;
        foreach (var pax in config.Passengers)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (index > 0)
            {
                var addBtn = page.Locator(IrctcSelectors.AddPassenger).First;
                if (await addBtn.CountAsync() > 0 && await addBtn.IsVisibleAsync())
                {
                    await addBtn.ClickAsync();
                    await page.WaitForTimeoutAsync(700);
                }
            }

            var nameInput = page.Locator(IrctcSelectors.PassengerName).Nth(index);
            if (await nameInput.IsVisibleAsync())
            {
                await nameInput.ClickAsync();
                await nameInput.FillAsync("");
                await nameInput.PressSequentiallyAsync(pax.Name, new LocatorPressSequentiallyOptions { Delay = 30 });
            }

            var ageInput = page.Locator(IrctcSelectors.PassengerAge).Nth(index);
            if (await ageInput.IsVisibleAsync())
            {
                await ageInput.ClickAsync();
                await ageInput.FillAsync("");
                await ageInput.PressSequentiallyAsync(pax.Age, new LocatorPressSequentiallyOptions { Delay = 30 });
            }

            var genderDropdown = page.Locator(IrctcSelectors.PassengerGender).Nth(index);
            if (await genderDropdown.CountAsync() > 0 && await genderDropdown.IsVisibleAsync())
            {
                var gCode = pax.Gender.StartsWith("M", StringComparison.OrdinalIgnoreCase) ? "M"
                    : pax.Gender.StartsWith("F", StringComparison.OrdinalIgnoreCase) ? "F" : "T";
                await genderDropdown.SelectOptionAsync(new[] { gCode });
            }

            var berthDropdown = page.Locator(IrctcSelectors.PassengerBerth).Nth(index);
            if (await berthDropdown.CountAsync() > 0 &&
                await berthDropdown.IsVisibleAsync() &&
                pax.BerthPreference != "No Preference")
            {
                var bCode = pax.BerthPreference switch
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
                    try { await berthDropdown.SelectOptionAsync(new[] { bCode }); }
                    catch { /* option may not exist for this class */ }
                }
            }

            var foodDropdown = page.Locator(IrctcSelectors.PassengerFood).Nth(index);
            if (await foodDropdown.CountAsync() > 0 &&
                await foodDropdown.IsVisibleAsync() &&
                pax.FoodPreference != "No Preference")
            {
                var fCode = pax.FoodPreference.StartsWith("V", StringComparison.OrdinalIgnoreCase) &&
                            !pax.FoodPreference.Contains("Non", StringComparison.OrdinalIgnoreCase)
                    ? "V"
                    : pax.FoodPreference.Contains("Non", StringComparison.OrdinalIgnoreCase) ? "N" : "";
                if (!string.IsNullOrEmpty(fCode))
                {
                    try { await foodDropdown.SelectOptionAsync(new[] { fCode }); }
                    catch { /* food not offered */ }
                }
            }

            index++;
            await page.WaitForTimeoutAsync(250);
        }

        if (!string.IsNullOrWhiteSpace(config.MobileNumber))
        {
            try
            {
                var mobile = page.Locator(IrctcSelectors.MobileNumber).First;
                if (await mobile.CountAsync() > 0 && await mobile.IsVisibleAsync())
                {
                    await mobile.ClickAsync();
                    await mobile.FillAsync("");
                    await mobile.PressSequentiallyAsync(config.MobileNumber, new LocatorPressSequentiallyOptions { Delay = 30 });
                    progress?.Report("Filled mobile number.");
                }
            }
            catch
            {
                // ignore
            }
        }

        if (config.ConfirmBerthsOnly)
        {
            await TryCheckPreferenceAsync(page, IrctcSelectors.ConfirmBerths);
        }

        if (config.AutoUpgrade)
        {
            await TryCheckPreferenceAsync(page, IrctcSelectors.AutoUpgrade);
        }

        progress?.Report($"Filled {index} passenger(s).");
    }

    private static async Task TryCheckPreferenceAsync(IPage page, string selector)
    {
        try
        {
            var loc = page.Locator(selector).First;
            if (await loc.CountAsync() > 0)
            {
                if (await loc.EvaluateAsync<string>("el => el.tagName") == "INPUT")
                {
                    await loc.CheckAsync(new LocatorCheckOptions { Force = true });
                }
                else
                {
                    await loc.ClickAsync();
                }
            }
        }
        catch
        {
            // preference checkbox may not exist
        }
    }

    /// <summary>
    /// Passenger page: select paymentType radio (BHIM/UPI etc.), then Continue once.
    /// Matches chrome-extension selectPaymentType + passenger submit.
    /// </summary>
    private static async Task SelectPassengerPaymentTypeAndContinueAsync(
        IPage page,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var paymentType = MapGatewayToPaymentType(config.PaymentMethod);
        progress?.Report($"Selecting passenger payment type: {paymentType}...");

        try
        {
            await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
            await page.WaitForTimeoutAsync(500);

            var selected = await page.EvaluateAsync<bool>("""
                (wanted) => {
                    const w = (wanted || 'BHIM/UPI').toUpperCase();
                    const wantCards = w.includes('CARD') || w.includes('NET') || w.includes('WALLET') || w.includes('CREDIT');
                    const wantWallet = w.includes('EWALLET');
                    const radios = document.querySelectorAll("input[type='radio'][name='paymentType'], p-radiobutton[name='paymentType'] input");
                    for (const input of radios) {
                        const label = input.closest('label') || (input.id ? document.querySelector(`label[for='${input.id}']`) : null);
                        const text = ((label && label.textContent) || (input.parentElement && input.parentElement.textContent) || '').toUpperCase();
                        let match = false;
                        if (wantWallet) match = text.includes('EWALLET') || text.includes('E-WALLET');
                        else if (wantCards) match = text.includes('CARD') || text.includes('NET BANKING') || text.includes('WALLET');
                        else match = text.includes('BHIM') || text.includes('UPI');
                        if (match) {
                            input.scrollIntoView({ block: 'center' });
                            input.click();
                            return true;
                        }
                    }
                    const labels = document.querySelectorAll('label');
                    for (const label of labels) {
                        const t = (label.textContent || '').toUpperCase();
                        if ((!wantCards && !wantWallet && (t.includes('BHIM') || t.includes('UPI')))
                            || (wantCards && (t.includes('CARD') || t.includes('NET')))
                            || (wantWallet && t.includes('EWALLET'))) {
                            label.click();
                            return true;
                        }
                    }
                    return false;
                }
                """, paymentType);

            progress?.Report(selected
                ? $"Selected payment type: {paymentType}."
                : "Payment type radio not found — using IRCTC default.");
            await page.WaitForTimeoutAsync(400);
        }
        catch (Exception ex)
        {
            progress?.Report($"Payment type note: {ex.Message}");
        }

        progress?.Report("Clicking Continue (once)...");
        var urlBefore = page.Url;
        var continueBtn = page.Locator(IrctcSelectors.PassengerContinue).Last;
        if (await continueBtn.CountAsync() > 0 && await continueBtn.IsVisibleAsync())
        {
            await continueBtn.ClickAsync(new LocatorClickOptions { Timeout = 10_000 });
            try
            {
                await page.WaitForURLAsync(url => url != urlBefore, new PageWaitForURLOptions { Timeout = 25_000 });
                await page.WaitForLoadStateAsync(LoadState.DOMContentLoaded);
                await page.WaitForTimeoutAsync(1500);
            }
            catch
            {
                progress?.Report("Waiting for navigation after Continue...");
            }
        }
        else
        {
            progress?.Report("ACTION REQUIRED: Click Continue manually.");
            await page.WaitForURLAsync(url => url != urlBefore, new PageWaitForURLOptions { Timeout = 120_000 });
        }
    }

    /// <summary>Review page only — captcha + Continue. Do not click Pay here.</summary>
    private static async Task HandleReviewCaptchaAndContinueAsync(
        IPage page,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        try
        {
            await page.Locator("app-review-booking, app-captcha")
                .First
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 20_000, State = WaitForSelectorState.Visible });
        }
        catch
        {
            // may already be past review
        }

        if (!page.Url.Contains("review", StringComparison.OrdinalIgnoreCase)
            && await page.Locator("app-review-booking").CountAsync() == 0)
        {
            progress?.Report("Review page skipped or already passed.");
            return;
        }

        progress?.Report("On review page — captcha may be required. Fill if prompted, then Continue.");
        await page.WaitForTimeoutAsync(1000);

        var urlBefore = page.Url;
        var deadline = DateTime.UtcNow.AddMinutes(3);
        var continueClicked = false;
        while (DateTime.UtcNow < deadline && !continueClicked)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await IsSessionErrorPageAsync(page))
            {
                return;
            }

            if (await page.Locator(IrctcSelectors.CaptchaInput).CountAsync() > 0)
            {
                progress?.Report("ACTION REQUIRED: Enter review captcha, then click Continue.");
            }

            var reviewBtn = page.Locator(IrctcSelectors.ReviewContinue).Last;
            if (await reviewBtn.CountAsync() > 0 && await reviewBtn.IsVisibleAsync())
            {
                try
                {
                    var disabled = await reviewBtn.IsDisabledAsync();
                    if (!disabled)
                    {
                        await reviewBtn.ClickAsync();
                        continueClicked = true;
                        progress?.Report("Clicked review Continue (once).");
                        await page.WaitForTimeoutAsync(1500);
                        break;
                    }
                }
                catch
                {
                    // button may be disabled until captcha
                }
            }

            if (page.Url != urlBefore
                || await page.Locator(IrctcSelectors.PaymentComponent).CountAsync() > 0)
            {
                break;
            }

            await page.WaitForTimeoutAsync(2000);
        }
    }

    /// <summary>
    /// Payment gateway page: bank-type → bank-text provider → Pay &amp; Book once.
    /// Mirrors chrome-extension payment.js (selectPaymentMethod / Provider / clickPayButton).
    /// </summary>
    private static async Task SelectPaymentGatewayAndPayAsync(
        IPage page,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report("Waiting for payment options page...");

        try
        {
            await page.Locator(IrctcSelectors.PaymentComponent)
                .First
                .WaitForAsync(new LocatorWaitForOptions { Timeout = 90_000, State = WaitForSelectorState.Visible });
        }
        catch (TimeoutException)
        {
            progress?.Report("Payment options page not detected yet — check browser (captcha / continue).");
            return;
        }

        if (await IsSessionErrorPageAsync(page))
        {
            return;
        }

        var methodNeedle = MapGatewayToBankType(config.PaymentMethod);
        var providerNeedle = MapProviderNeedle(config.PaymentProvider);

        progress?.Report($"Selecting payment method: {methodNeedle}...");
        var methodClicked = await ClickBankTypeAsync(page, methodNeedle);
        progress?.Report(methodClicked
            ? $"Selected method containing '{methodNeedle}'."
            : $"Method '{methodNeedle}' not found — leaving IRCTC default.");
        await page.WaitForTimeoutAsync(600);

        if (!string.IsNullOrWhiteSpace(providerNeedle)
            && !methodNeedle.Contains("eWallet", StringComparison.OrdinalIgnoreCase))
        {
            progress?.Report($"Selecting payment provider: {providerNeedle}...");
            var providerClicked = await ClickBankProviderAsync(page, providerNeedle);
            progress?.Report(providerClicked
                ? $"Selected provider containing '{providerNeedle}'."
                : $"Provider '{providerNeedle}' not found — you can pick one manually.");
            await page.WaitForTimeoutAsync(500);
        }

        if (!config.AutoPay)
        {
            progress?.Report("AutoPay off — click Pay & Book manually in the browser.");
            return;
        }

        progress?.Report("Clicking Pay & Book (once)...");
        var urlBefore = page.Url;
        var payBtn = page.Locator(IrctcSelectors.PayAndBookButton).First;
        if (await payBtn.CountAsync() == 0)
        {
            payBtn = page.Locator(IrctcSelectors.PayButton).First;
        }

        if (await payBtn.CountAsync() > 0 && await payBtn.IsVisibleAsync())
        {
            try
            {
                await payBtn.ClickAsync(new LocatorClickOptions { Timeout = 10_000 });
                progress?.Report("Pay & Book clicked. Waiting for UPI/QR or bank page...");
                try
                {
                    await page.WaitForURLAsync(
                        url => url != urlBefore
                               || url.Contains("bankpay", StringComparison.OrdinalIgnoreCase)
                               || url.Contains("payment", StringComparison.OrdinalIgnoreCase),
                        new PageWaitForURLOptions { Timeout = 30_000 });
                }
                catch
                {
                    // QR may open in same page without URL change
                }

                await page.WaitForTimeoutAsync(1500);

                if (methodNeedle.Contains("eWallet", StringComparison.OrdinalIgnoreCase)
                    || await page.Locator(IrctcSelectors.EwalletComponent).CountAsync() > 0)
                {
                    var confirm = page.Locator(IrctcSelectors.EwalletConfirm).First;
                    if (await confirm.CountAsync() > 0 && await confirm.IsVisibleAsync())
                    {
                        await confirm.ClickAsync();
                        progress?.Report("Confirmed IRCTC eWallet.");
                    }
                }
            }
            catch (Exception ex)
            {
                progress?.Report($"Pay & Book note: {ex.Message}. Click it once manually if needed.");
            }
        }
        else
        {
            progress?.Report("ACTION REQUIRED: Click Pay & Book once in the browser.");
        }
    }

    private static async Task<bool> ClickBankTypeAsync(IPage page, string needle)
    {
        return await page.EvaluateAsync<bool>("""
            (needle) => {
                const n = (needle || '').toUpperCase();
                const els = document.querySelectorAll('.bank-type.ng-star-inserted, #pay-type .bank-type, .bank-type');
                for (const el of els) {
                    const t = (el.textContent || '').toUpperCase();
                    if (t.includes(n) || (n.includes('BHIM') && t.includes('UPI')) || (n.includes('UPI') && t.includes('BHIM'))) {
                        el.scrollIntoView({ block: 'center' });
                        el.click();
                        return true;
                    }
                }
                return false;
            }
            """, needle);
    }

    private static async Task<bool> ClickBankProviderAsync(IPage page, string needle)
    {
        return await page.EvaluateAsync<bool>("""
            (needle) => {
                const n = (needle || '').toUpperCase();
                const els = document.querySelectorAll('.bank-text, #bank-type .bank-text, #bank-type .pay_tax_text');
                for (const el of els) {
                    const t = (el.textContent || '').toUpperCase();
                    if (t.includes(n)) {
                        el.scrollIntoView({ block: 'center' });
                        el.click();
                        return true;
                    }
                }
                return false;
            }
            """, needle);
    }

    private static string MapGatewayToPaymentType(string gateway)
    {
        var g = gateway ?? string.Empty;
        if (g.Contains("Card", StringComparison.OrdinalIgnoreCase)
            || g.Contains("Netbanking", StringComparison.OrdinalIgnoreCase)
            || g.Contains("Wallet", StringComparison.OrdinalIgnoreCase)
            || g.Contains("Credit", StringComparison.OrdinalIgnoreCase))
        {
            return "Credit & Debit cards / Net Banking / Wallets";
        }

        if (g.Contains("eWallet", StringComparison.OrdinalIgnoreCase))
        {
            return "IRCTC eWallet";
        }

        return "BHIM/UPI";
    }

    private static string MapGatewayToBankType(string gateway)
    {
        var g = gateway ?? string.Empty;
        if (g.Contains("eWallet", StringComparison.OrdinalIgnoreCase))
        {
            return "IRCTC eWallet";
        }

        if (g.Contains("Card", StringComparison.OrdinalIgnoreCase)
            || g.Contains("Netbanking", StringComparison.OrdinalIgnoreCase)
            || g.Contains("Wallet", StringComparison.OrdinalIgnoreCase))
        {
            return "Credit";
        }

        // Extension default: "BHIM/ UPI/ USSD"
        return "BHIM";
    }

    private static string MapProviderNeedle(string provider)
    {
        if (string.IsNullOrWhiteSpace(provider))
        {
            return "PAYTM";
        }

        var p = provider.Trim();
        if (p.Contains("PayTM", StringComparison.OrdinalIgnoreCase) || p.Contains("Paytm", StringComparison.OrdinalIgnoreCase))
        {
            return "PAYTM";
        }

        if (p.Contains("PhonePe", StringComparison.OrdinalIgnoreCase))
        {
            return "PHONEPE";
        }

        if (p.Contains("Amazon", StringComparison.OrdinalIgnoreCase))
        {
            return "AMAZON";
        }

        // Strip aliases like PayTM-QR_paytm@qr → PAYTM
        var at = p.IndexOf('@');
        if (at > 0)
        {
            p = p[..at];
        }

        var dash = p.IndexOf('-');
        if (dash > 0)
        {
            p = p[..dash];
        }

        return p.ToUpperInvariant();
    }

    private static async Task CapturePaymentAndWaitForConfirmationAsync(
        IPage page,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        if (await IsSessionErrorPageAsync(page))
        {
            progress?.Report(SessionErrorMessage());
            progress?.Report("Not a payment QR — IRCTC showed an error page. Screenshot skipped.");
            await HandleSessionErrorAsync(page, progress, cancellationToken);
            return;
        }

        progress?.Report("Waiting for real payment page / QR...");

        // Wait until we look like payment (or timeout) — don't screenshot error pages
        var paymentDeadline = DateTime.UtcNow.AddMinutes(3);
        var onPayment = false;
        while (DateTime.UtcNow < paymentDeadline)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report(SessionErrorMessage());
                await HandleSessionErrorAsync(page, progress, cancellationToken);
                return;
            }

            var url = page.Url;
            var body = "";
            try { body = await page.InnerTextAsync("body"); } catch { /* ignore */ }

            onPayment =
                url.Contains("payment", StringComparison.OrdinalIgnoreCase)
                || url.Contains("bankpay", StringComparison.OrdinalIgnoreCase)
                || url.Contains("pay", StringComparison.OrdinalIgnoreCase)
                || body.Contains("BHIM", StringComparison.OrdinalIgnoreCase)
                || body.Contains("UPI", StringComparison.OrdinalIgnoreCase)
                || body.Contains("Scan", StringComparison.OrdinalIgnoreCase)
                || await page.Locator("img[src*='qr'], canvas, img[alt*='QR'], img[alt*='qr']").CountAsync() > 0;

            if (onPayment)
            {
                break;
            }

            progress?.Report("Not on payment page yet — waiting (complete captcha/continue if needed)...");
            await page.WaitForTimeoutAsync(2000);
        }

        if (!onPayment)
        {
            progress?.Report("Payment page not detected. Complete payment in the browser if shown. No QR image saved.");
        }
        else
        {
            try
            {
                var screenshotPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "payment_qr.png");
                await page.ScreenshotAsync(new PageScreenshotOptions { Path = screenshotPath, FullPage = false });
                progress?.Report($"Payment screenshot saved: {screenshotPath}");

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = screenshotPath,
                    UseShellExecute = true
                });
                progress?.Report("Scan QR / pay in browser. Waiting for confirmation (up to 10 min)...");
            }
            catch (Exception ex)
            {
                progress?.Report($"Screenshot note: {ex.Message}. Pay in the browser window.");
            }
        }

        try
        {
            await page.WaitForURLAsync(
                url => url.Contains("bookingConfirm", StringComparison.OrdinalIgnoreCase)
                       || url.Contains("printTicket", StringComparison.OrdinalIgnoreCase)
                       || url.Contains("confirmation", StringComparison.OrdinalIgnoreCase),
                new PageWaitForURLOptions { Timeout = 600_000 });

            cancellationToken.ThrowIfCancellationRequested();

            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report(SessionErrorMessage());
                return;
            }

            progress?.Report("BOOKING CONFIRMED — saving ticket screenshot...");
            await page.WaitForTimeoutAsync(2000);

            var ticketPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ticket.png");
            await page.ScreenshotAsync(new PageScreenshotOptions { Path = ticketPath, FullPage = true });
            progress?.Report($"Ticket saved: {ticketPath}");

            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = ticketPath,
                UseShellExecute = true
            });
            progress?.Report("Done. Check IRCTC email/SMS for e-ticket.");
        }
        catch (TimeoutException)
        {
            progress?.Report("Payment still pending. Browser stays open — finish payment there.");
            try
            {
                await Task.Delay(Timeout.Infinite, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                // stop
            }
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

