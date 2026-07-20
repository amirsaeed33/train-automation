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

            progress?.Report("Opening IRCTC...");
            await page.GotoAsync("https://www.irctc.co.in/nget/train-search", new PageGotoOptions
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
                    "Auto Book Now failed. In browser: click class tab → date card → Book Now. Waiting for login...");
            }

            await LoginAsync(page, config, progress, cancellationToken);
            await FillPassengersAsync(page, config, progress, cancellationToken);
            await SelectPaymentAndContinueAsync(page, config, progress, cancellationToken);
            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report(SessionErrorMessage());
                await HandleSessionErrorAsync(page, progress, cancellationToken);
                return;
            }

            await HandleReviewAndPayAsync(page, progress, cancellationToken);
            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report(SessionErrorMessage());
                await HandleSessionErrorAsync(page, progress, cancellationToken);
                return;
            }

            await CapturePaymentAndWaitForConfirmationAsync(page, progress, cancellationToken);
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
    /// IRCTC list UI (before enquire): class boxes ".pre-avl" with "Sleeper (SL)" + Refresh.
    /// After clicking a class / Refresh, date cards appear; then Book Now enables.
    /// </summary>
    private static async Task<bool> SelectTrainClassAndBookAsync(
        IPage page,
        TrainResult selectedTrain,
        TrainSearchSettings settings,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        var preferredClass = string.IsNullOrWhiteSpace(settings.PreferredClass)
            ? config.PreferredClass
            : settings.PreferredClass;
        var dateHint = $"{settings.TravelDate.Day} {settings.TravelDate:MMM}";
        var trainNum = selectedTrain.TrainNumber;

        progress?.Report($"Waiting for train list / {trainNum}...");

        // Wait for train number text anywhere (don't depend only on app-train-avl-enq)
        try
        {
            await page.GetByText(trainNum, new PageGetByTextOptions { Exact = false })
                .First
                .WaitForAsync(new LocatorWaitForOptions
                {
                    Timeout = 45_000,
                    State = WaitForSelectorState.Visible
                });
        }
        catch (TimeoutException)
        {
            progress?.Report($"Train {trainNum} text not found on IRCTC results page.");
            return false;
        }

        // Scroll train into view
        await page.EvaluateAsync("""
            (trainNum) => {
                const walk = document.querySelectorAll('app-train-avl-enq, .bull-back, .train-heading, strong, div');
                for (const el of walk) {
                    const t = el.textContent || '';
                    if (t.includes(trainNum) && (t.includes('Refresh') || t.includes('Book Now') || el.classList?.contains('train-heading'))) {
                        el.scrollIntoView({ block: 'center', behavior: 'instant' });
                        return true;
                    }
                }
                return false;
            }
            """, trainNum);

        await page.WaitForTimeoutAsync(800);
        progress?.Report($"Found {trainNum}. Clicking class {preferredClass} (or Refresh)...");

        // Click the class box (.pre-avl with SL) or its Refresh link — this loads availability
        var classClicked = await page.EvaluateAsync<bool>("""
            ({ trainNum, classCode }) => {
                const findRoot = () => {
                    const candidates = [
                        ...document.querySelectorAll('app-train-avl-enq'),
                        ...document.querySelectorAll('.bull-back')
                    ];
                    for (const el of candidates) {
                        const t = el.textContent || '';
                        if (t.includes(trainNum) && (t.includes('Refresh') || t.includes('Book Now') || t.includes('Sleeper') || t.includes('AC '))) {
                            return el;
                        }
                    }
                    // Broad fallback
                    for (const el of document.querySelectorAll('div')) {
                        const t = el.textContent || '';
                        if (t.includes(trainNum) && t.includes('Refresh') && t.includes('Book Now') && t.length < 4000) {
                            return el;
                        }
                    }
                    return null;
                };

                const root = findRoot();
                if (!root) return false;
                root.scrollIntoView({ block: 'center' });

                const needle = '(' + classCode + ')';
                const boxes = root.querySelectorAll('.pre-avl');

                // Phase A: class boxes before enquire (contain class name + Refresh, no date)
                for (const box of boxes) {
                    const text = (box.textContent || '').replace(/\s+/g, ' ').trim();
                    const strong = (box.querySelector('strong')?.textContent || '').trim();
                    const label = strong || text;
                    const matches =
                        label.includes(needle) ||
                        label === classCode ||
                        label.endsWith(classCode) ||
                        (classCode === 'SL' && /sleeper/i.test(label));

                    if (!matches) continue;
                    // Skip if this already looks like a date-availability card
                    if (/\d{1,2}\s+(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)/i.test(text) &&
                        /AVAILABLE|WL|RAC|REGRET/i.test(text)) {
                        continue;
                    }

                    // Prefer Refresh link inside the class box (IRCTC enquire)
                    const refresh = [...box.querySelectorAll('a, span, div, button')].find(n =>
                        /^\s*Refresh\s*$/i.test((n.textContent || '').trim()) ||
                        (n.textContent || '').trim().toLowerCase() === 'refresh'
                    );
                    if (refresh) {
                        refresh.click();
                        return true;
                    }
                    box.click();
                    return true;
                }

                // Phase B: click any element in root that looks like "Sleeper (SL)" short label
                for (const node of root.querySelectorAll('strong, span, div, a, li')) {
                    const text = (node.textContent || '').replace(/\s+/g, ' ').trim();
                    if (!text || text.length > 28) continue;
                    if (!(text.includes(needle) || (classCode === 'SL' && /^sleeper/i.test(text)))) continue;
                    if (/AVAILABLE|WL|RAC|REGRET|Book Now|₹/i.test(text)) continue;
                    const clickable = node.closest('.pre-avl') || node;
                    // Try refresh sibling
                    const parent = node.closest('.pre-avl, div, li') || node.parentElement;
                    const refresh = parent && [...parent.querySelectorAll('a, span')].find(n =>
                        /refresh/i.test(n.textContent || ''));
                    (refresh || clickable).click();
                    return true;
                }
                return false;
            }
            """, new { trainNum, classCode = preferredClass });

        if (!classClicked)
        {
            // Playwright locator fallback
            try
            {
                var trainRoot = page.Locator("app-train-avl-enq, .bull-back")
                    .Filter(new LocatorFilterOptions { HasText = trainNum })
                    .First;
                var classBox = trainRoot.Locator(".pre-avl")
                    .Filter(new LocatorFilterOptions { HasText = preferredClass })
                    .First;
                var refresh = classBox.Locator("text=Refresh").First;
                if (await refresh.CountAsync() > 0)
                {
                    await refresh.ClickAsync(new LocatorClickOptions { Force = true });
                    classClicked = true;
                    progress?.Report($"Clicked Refresh on {preferredClass}.");
                }
                else
                {
                    await classBox.ClickAsync(new LocatorClickOptions { Force = true });
                    classClicked = true;
                    progress?.Report($"Clicked class box {preferredClass}.");
                }
            }
            catch (Exception ex)
            {
                progress?.Report($"Class click failed: {ex.Message}");
            }
        }
        else
        {
            progress?.Report($"Clicked {preferredClass} / Refresh. Waiting for availability...");
        }

        if (!classClicked)
        {
            progress?.Report("Could not click class. Click Sleeper (SL) Refresh manually.");
        }

        // Wait until date availability cards appear for this train
        try
        {
            await page.WaitForFunctionAsync(
                """
                (trainNum) => {
                    const roots = [
                        ...document.querySelectorAll('app-train-avl-enq'),
                        ...document.querySelectorAll('.bull-back')
                    ];
                    let root = null;
                    for (const el of roots) {
                        if ((el.textContent || '').includes(trainNum)) { root = el; break; }
                    }
                    if (!root) return false;
                    const text = root.textContent || '';
                    // Availability loaded when we see WL/AVAILABLE/RAC/REGRET with a month date
                    const hasStatus = /AVAILABLE|\bWL\s*\d*|\bRAC\b|REGRET/i.test(text);
                    const hasDate = /\d{1,2}\s+(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)/i.test(text);
                    // Also accept tab strip after class select
                    const hasTabs = root.querySelectorAll('li[role="tab"], .ui-tabview-nav li').length >= 2;
                    return (hasStatus && hasDate) || (hasTabs && hasStatus);
                }
                """,
                trainNum,
                new PageWaitForFunctionOptions { Timeout = 20_000 });
            progress?.Report("Availability loaded.");
        }
        catch (TimeoutException)
        {
            progress?.Report("Availability slow to load — retrying class Refresh...");
            await page.EvaluateAsync("""
                ({ trainNum, classCode }) => {
                    const roots = [...document.querySelectorAll('app-train-avl-enq, .bull-back')];
                    const root = roots.find(el => (el.textContent || '').includes(trainNum));
                    if (!root) return;
                    const needle = '(' + classCode + ')';
                    for (const box of root.querySelectorAll('.pre-avl')) {
                        const t = box.textContent || '';
                        if (t.includes(needle) || (classCode === 'SL' && /sleeper/i.test(t))) {
                            const r = [...box.querySelectorAll('a, span')].find(n => /refresh/i.test(n.textContent || ''));
                            (r || box).click();
                            return;
                        }
                    }
                }
                """, new { trainNum, classCode = preferredClass });
            await page.WaitForTimeoutAsync(3000);
        }

        await page.WaitForTimeoutAsync(1000);

        // Click date card + Book Now (retry loop)
        var deadline = DateTime.UtcNow.AddSeconds(Math.Max(60, config.AvailabilityTimeoutSeconds));
        var refreshMs = Math.Max(800, config.RefreshIntervalMs);
        var attempt = 0;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;

            var result = await page.EvaluateAsync<BookAttemptResult>("""
                ({ trainNum, dateHint, confirmOnly }) => {
                    const roots = [...document.querySelectorAll('app-train-avl-enq'), ...document.querySelectorAll('.bull-back')];
                    let target = roots.find(el => (el.textContent || '').includes(trainNum));
                    if (!target) {
                        target = [...document.querySelectorAll('div')].find(el => {
                            const t = el.textContent || '';
                            return t.includes(trainNum) && t.includes('Book Now') && t.length < 5000;
                        }) || null;
                    }
                    if (!target) return { ok: false, reason: 'train-not-found' };

                    let cards = [...target.querySelectorAll('.pre-avl')].filter(el => {
                        const t = (el.textContent || '').replace(/\s+/g, ' ').trim();
                        if (/REGRET/i.test(t)) return false;
                        const hasDate = /\d{1,2}\s+(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)/i.test(t);
                        const hasStatus = /AVAILABLE|RAC|\bWL\s*\d*|WAITING/i.test(t);
                        return hasDate && hasStatus;
                    });

                    if (confirmOnly) {
                        cards = cards.filter(el => /AVAILABLE/i.test(el.textContent || ''));
                    }
                    if (!cards.length) return { ok: false, reason: 'no-date-card' };

                    let chosen = cards.find(el => (el.textContent || '').includes(dateHint)) || null;
                    if (!chosen) chosen = cards.find(el => /AVAILABLE/i.test(el.textContent || '')) || cards[0];
                    chosen.scrollIntoView({ block: 'center' });
                    chosen.click();
                    // Date only — Book Now clicked once from C# after wait (avoids IRCTC double-click error)
                    return { ok: false, reason: 'date-selected' };
                }
                """, new
            {
                trainNum,
                dateHint,
                confirmOnly = config.ConfirmBerthsOnly
            });

            if (string.Equals(result?.Reason, "date-selected", StringComparison.OrdinalIgnoreCase))
            {
                progress?.Report($"Selected date ({dateHint}). Waiting before Book Now...");
                await page.WaitForTimeoutAsync(1200);

                if (await IsSessionErrorPageAsync(page))
                {
                    progress?.Report(SessionErrorMessage());
                    return false;
                }

                if (await TryClickBookNowOnceAsync(page, trainNum, progress))
                {
                    return true;
                }
            }
            else if (result?.Ok == true)
            {
                progress?.Report("Clicked Book Now (once). Waiting...");
                await page.WaitForTimeoutAsync(2000);

                if (await IsSessionErrorPageAsync(page))
                {
                    progress?.Report(SessionErrorMessage());
                    return false;
                }

                await AcceptContinueDialogsAsync(page, progress);
                return true;
            }

            // Playwright fallback — date then single Book Now
            try
            {
                var root = page.Locator("app-train-avl-enq, .bull-back")
                    .Filter(new LocatorFilterOptions { HasText = trainNum })
                    .First;

                var dateCards = root.Locator(".pre-avl");
                var n = await dateCards.CountAsync();
                ILocator? pick = null;
                for (var i = 0; i < n; i++)
                {
                    var c = dateCards.Nth(i);
                    var text = await c.InnerTextAsync();
                    if (text.Contains("REGRET", StringComparison.OrdinalIgnoreCase)) continue;
                    if (!System.Text.RegularExpressions.Regex.IsMatch(text, @"\d{1,2}\s+(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)",
                            System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                    {
                        continue;
                    }

                    if (config.ConfirmBerthsOnly &&
                        !text.Contains("AVAILABLE", StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    if (text.Contains(dateHint, StringComparison.OrdinalIgnoreCase))
                    {
                        pick = c;
                        break;
                    }

                    pick ??= c;
                }

                if (pick is not null)
                {
                    await pick.ClickAsync(new LocatorClickOptions { Force = true });
                    progress?.Report($"Clicked date ({dateHint}).");
                    await page.WaitForTimeoutAsync(1200);

                    if (await TryClickBookNowOnceAsync(page, trainNum, progress))
                    {
                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                progress?.Report($"Fallback: {ex.Message}");
            }

            if (await IsSessionErrorPageAsync(page))
            {
                progress?.Report(SessionErrorMessage());
                return false;
            }

            progress?.Report($"Attempt {attempt}: {result?.Reason ?? "pending"}...");

            // Limit refreshes — repeated Refresh/Book Now causes IRCTC session errors
            if (attempt <= 2)
            {
                await page.EvaluateAsync("""
                    ({ trainNum, classCode }) => {
                        const roots = [...document.querySelectorAll('app-train-avl-enq, .bull-back')];
                        const root = roots.find(el => (el.textContent || '').includes(trainNum));
                        if (!root) return;
                        const needle = '(' + classCode + ')';
                        const tab = root.querySelector('li[aria-selected="true"] a, .ui-state-active a');
                        if (tab) { tab.click(); return; }
                        for (const box of root.querySelectorAll('.pre-avl')) {
                            const t = box.textContent || '';
                            if (t.includes(needle) || (classCode === 'SL' && /sleeper/i.test(t) && !/\d{1,2}\s+Jul/i.test(t))) {
                                const r = [...box.querySelectorAll('a, span')].find(n => /refresh/i.test(n.textContent || ''));
                                (r || box).click();
                                return;
                            }
                        }
                    }
                    """, new { trainNum, classCode = preferredClass });
            }

            await page.WaitForTimeoutAsync(refreshMs);
        }

        progress?.Report("Timed out. Manually: Refresh class → date → Book Now (single click only).");
        return false;
    }

    private static async Task<bool> TryClickBookNowOnceAsync(
        IPage page,
        string trainNum,
        IProgress<string>? progress)
    {
        var root = page.Locator("app-train-avl-enq, .bull-back")
            .Filter(new LocatorFilterOptions { HasText = trainNum })
            .First;

        var book = root.Locator("button:has-text('Book Now')").First;
        if (await book.CountAsync() == 0)
        {
            progress?.Report("Book Now button not found.");
            return false;
        }

        if (!await book.IsEnabledAsync())
        {
            progress?.Report("Book Now still disabled.");
            return false;
        }

        await book.ClickAsync(new LocatorClickOptions { Timeout = 5_000 });
        progress?.Report("Clicked Book Now (single click).");
        await page.WaitForTimeoutAsync(2000);

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
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static string SessionErrorMessage() =>
        "IRCTC session error (Sorry!!! Please Try again). Usually double-click or Back/Refresh. Close browser and book again — one click only.";

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
        CancellationToken cancellationToken)
    {
        progress?.Report("Waiting for Login popup (up to 3 min — click Book Now if needed)...");
        var userInput = page.Locator(IrctcSelectors.LoginUserId).First;

        try
        {
            await userInput.WaitForAsync(new LocatorWaitForOptions { Timeout = 180_000 });
        }
        catch (TimeoutException)
        {
            progress?.Report("Login popup did not appear. Click Book Now in the browser, then wait.");
            await userInput.WaitForAsync(new LocatorWaitForOptions { Timeout = 180_000 });
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

    private static async Task SelectPaymentAndContinueAsync(
        IPage page,
        BookingConfiguration config,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        progress?.Report($"Selecting payment: {config.PaymentMethod}...");

        try
        {
            await page.EvaluateAsync("window.scrollTo(0, document.body.scrollHeight)");
            await page.WaitForTimeoutAsync(600);

            var paymentLabel = page.Locator($"label:has-text('{config.PaymentMethod}')").First;
            if (await paymentLabel.CountAsync() > 0 && await paymentLabel.IsVisibleAsync())
            {
                await paymentLabel.ClickAsync();
                progress?.Report($"Selected {config.PaymentMethod}.");
                await page.WaitForTimeoutAsync(400);
            }
            else
            {
                var upi = page.Locator("label:has-text('BHIM/UPI'), label:has-text('UPI')").First;
                if (await upi.CountAsync() > 0 && await upi.IsVisibleAsync())
                {
                    await upi.ClickAsync();
                    progress?.Report("Selected BHIM/UPI (fallback).");
                }
                else
                {
                    progress?.Report("Payment option not visible — using IRCTC default.");
                }
            }
        }
        catch (Exception ex)
        {
            progress?.Report($"Payment selection note: {ex.Message}");
        }

        progress?.Report("Clicking Continue...");
        var urlBefore = page.Url;
        var continueBtn = page.Locator(IrctcSelectors.PassengerContinue).Last;
        if (await continueBtn.CountAsync() > 0 && await continueBtn.IsVisibleAsync())
        {
            await continueBtn.ClickAsync();
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

    private static async Task HandleReviewAndPayAsync(
        IPage page,
        IProgress<string>? progress,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (page.Url.Contains("review", StringComparison.OrdinalIgnoreCase))
        {
            progress?.Report("On review page — captcha may be required. Fill if prompted, then Continue.");
            await page.WaitForTimeoutAsync(1200);

            var urlBefore = page.Url;
            var deadline = DateTime.UtcNow.AddMinutes(3);
            while (DateTime.UtcNow < deadline && page.Url == urlBefore)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var captchaVisible = await page.Locator(IrctcSelectors.CaptchaInput).CountAsync() > 0;
                if (captchaVisible)
                {
                    progress?.Report("ACTION REQUIRED: Enter review captcha, then click Continue.");
                }

                var reviewBtn = page.Locator(IrctcSelectors.ReviewContinue).Last;
                if (await reviewBtn.CountAsync() > 0 && await reviewBtn.IsVisibleAsync())
                {
                    try
                    {
                        await reviewBtn.ClickAsync();
                        await page.WaitForTimeoutAsync(1500);
                        if (page.Url != urlBefore)
                        {
                            break;
                        }
                    }
                    catch
                    {
                        // button may be disabled until captcha
                    }
                }

                await page.WaitForTimeoutAsync(2000);
            }
        }

        if (!page.Url.Contains("payment", StringComparison.OrdinalIgnoreCase) &&
            !page.Url.Contains("bankpay", StringComparison.OrdinalIgnoreCase))
        {
            var urlBeforePay = page.Url;
            var payBtn = page.Locator("button:has-text('Continue'):visible, button:has-text('Proceed'):visible, button:has-text('Pay'):visible").Last;
            if (await payBtn.CountAsync() > 0 && await payBtn.IsVisibleAsync())
            {
                try
                {
                    await payBtn.ClickAsync();
                    progress?.Report("Clicked proceed/pay...");
                    await page.WaitForURLAsync(url => url != urlBeforePay, new PageWaitForURLOptions { Timeout = 20_000 });
                }
                catch
                {
                    // user may need to click
                }
            }
        }
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
