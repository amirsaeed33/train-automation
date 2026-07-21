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
                    "Stopped on train list: train/class/date not auto-selected. "
                    + "In the browser pick a train that is listed → class Refresh → date → Book Now, "
                    + "or press Stop and choose a train that appears on IRCTC for this route.");
                // Do not run passenger/payment automation — wait until user books manually or presses Stop.
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
        await page.EvaluateAsync("""
            async () => {
                const sleep = (ms) => new Promise(r => setTimeout(r, ms));
                const scroller = document.querySelector('.tnc') || document.scrollingElement || document.body;
                for (let i = 0; i < 6; i++) {
                    window.scrollBy(0, Math.floor(window.innerHeight * 0.85));
                    if (scroller && scroller !== document.body) scroller.scrollTop += 600;
                    await sleep(350);
                }
                window.scrollTo(0, 0);
                await sleep(300);
            }
            """);

        var found = await page.EvaluateAsync<TrainFindResult>("""
            (trainNum) => {
                const headings = [...document.querySelectorAll('app-train-avl-enq .train-heading, .train-heading')];
                const visible = [];
                const pushVisible = (text) => {
                    const matches = text.matchAll(/\((\d{3,5})\)/g);
                    for (const m of matches) visible.push(m[1]);
                    const bare = text.match(/\b(\d{5})\b/g);
                    if (bare) bare.forEach(n => visible.push(n));
                };

                for (const h of headings) {
                    const text = (h.textContent || '').replace(/\s+/g, ' ').trim();
                    pushVisible(text);
                    const digits = text.replace(/\D/g, '');
                    if (digits.includes(trainNum) || text.includes(trainNum) || text.includes('(' + trainNum + ')')) {
                        const root = h.closest('app-train-avl-enq') || h.closest('.bull-back') || h.parentElement;
                        if (root) {
                            root.scrollIntoView({ block: 'center', behavior: 'instant' });
                            return { ok: true, heading: text.slice(0, 80), visible: [] };
                        }
                    }
                }

                for (const el of document.querySelectorAll('app-train-avl-enq')) {
                    const t = (el.textContent || '');
                    pushVisible(t);
                    const digitTokens = t.replace(/\D/g, ' ').split(/\s+/).filter(Boolean);
                    if (t.includes('(' + trainNum + ')') ||
                        new RegExp('(?:^|\\D)' + trainNum + '(?:\\D|$)').test(t) ||
                        digitTokens.includes(trainNum)) {
                        el.scrollIntoView({ block: 'center', behavior: 'instant' });
                        return { ok: true, heading: 'fallback-root', visible: [] };
                    }
                }

                // Last resort: full page body may list "NAME (12345)"
                pushVisible(document.body?.innerText || '');
                return { ok: false, heading: '', visible: [...new Set(visible)].slice(0, 25) };
            }
            """, trainNum);

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

        await page.WaitForTimeoutAsync(800);

        var deadline = DateTime.UtcNow.AddSeconds(Math.Max(45, config.AvailabilityTimeoutSeconds));
        var refreshMs = Math.Max(800, config.RefreshIntervalMs);
        var attempt = 0;
        var warnedConfirm = false;

        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            attempt++;

            var result = await page.EvaluateAsync<BookAttemptResult>("""
                ({ trainNum, dateDay, dateMonth, dateHint, confirmOnly }) => {
                    const roots = [...document.querySelectorAll('app-train-avl-enq')];
                    let target = roots.find(el => {
                        const t = el.textContent || '';
                        return t.includes(trainNum) || t.replace(/\D/g, ' ').split(/\s+/).includes(trainNum);
                    });
                    if (!target) {
                        return { ok: false, reason: 'train-not-found' };
                    }

                    let cards = [...target.querySelectorAll('.pre-avl')].filter(el => {
                        const t = (el.textContent || '').replace(/\s+/g, ' ').trim();
                        if (/REGRET/i.test(t)) return false;
                        const hasDate = /\d{1,2}\s*,?\s*(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)/i.test(t);
                        const hasStatus = /AVAILABLE|RAC|\bWL\s*\d*|WAITING/i.test(t);
                        if (!hasDate || !hasStatus) return false;
                        return true;
                    });

                    const availableCards = cards.filter(el => /AVAILABLE/i.test(el.textContent || ''));
                    if (confirmOnly) {
                        if (!availableCards.length) {
                            return { ok: false, reason: 'no-confirm-berth' };
                        }
                        cards = availableCards;
                    }
                    if (!cards.length) return { ok: false, reason: 'no-date-card' };

                    let chosen = cards.find(el => {
                        const t = el.textContent || '';
                        return t.includes(dateHint) || (t.includes(dateDay) && new RegExp(dateMonth, 'i').test(t));
                    }) || null;
                    if (!chosen) chosen = availableCards[0] || cards[0];
                    chosen.scrollIntoView({ block: 'center' });
                    chosen.click();
                    return { ok: false, reason: 'date-selected' };
                }
                """, new
            {
                trainNum,
                dateDay,
                dateMonth,
                dateHint,
                confirmOnly = config.ConfirmBerthsOnly
            });

            if (string.Equals(result?.Reason, "no-confirm-berth", StringComparison.OrdinalIgnoreCase))
            {
                if (!warnedConfirm)
                {
                    progress?.Report(
                        "No AVAILABLE seats (only WL/RAC). Uncheck 'Book only if confirm berths' to book waitlist, or pick another class.");
                    warnedConfirm = true;
                }

                if (attempt >= 3)
                {
                    return false;
                }
            }
            else if (string.Equals(result?.Reason, "date-selected", StringComparison.OrdinalIgnoreCase))
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
            else if (string.Equals(result?.Reason, "train-not-found", StringComparison.OrdinalIgnoreCase))
            {
                progress?.Report($"Train {trainNum} lost from DOM — scroll list and retry...");
            }

            try
            {
                var root = page.Locator("app-train-avl-enq")
                    .Filter(new LocatorFilterOptions { HasText = trainNum })
                    .First;

                if (await root.CountAsync() > 0)
                {
                    var dateCards = root.Locator(".pre-avl");
                    var n = await dateCards.CountAsync();
                    ILocator? pick = null;
                    for (var i = 0; i < n; i++)
                    {
                        var c = dateCards.Nth(i);
                        var text = await c.InnerTextAsync();
                        if (text.Contains("REGRET", StringComparison.OrdinalIgnoreCase)) continue;
                        if (!System.Text.RegularExpressions.Regex.IsMatch(
                                text,
                                @"\d{1,2}\s*,?\s*(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)",
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            continue;
                        }

                        if (!System.Text.RegularExpressions.Regex.IsMatch(
                                text, @"AVAILABLE|RAC|\bWL\s*\d*|WAITING",
                                System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                        {
                            continue;
                        }

                        if (config.ConfirmBerthsOnly &&
                            !text.Contains("AVAILABLE", StringComparison.OrdinalIgnoreCase))
                        {
                            continue;
                        }

                        if (text.Contains(dateHint, StringComparison.OrdinalIgnoreCase)
                            || (text.Contains(dateDay) && text.Contains(dateMonth, StringComparison.OrdinalIgnoreCase)))
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

            if (attempt <= 3 && !string.Equals(result?.Reason, "no-confirm-berth", StringComparison.OrdinalIgnoreCase))
            {
                await page.EvaluateAsync("""
                    (trainNum) => {
                        const roots = [...document.querySelectorAll('app-train-avl-enq')];
                        const root = roots.find(el => (el.textContent || '').includes(trainNum));
                        if (!root) return;
                        const tab = root.querySelector(
                            'li[role="tab"][aria-selected="true"] a, li[aria-selected="true"] a, .ui-state-active a');
                        if (tab) { tab.click(); return; }
                        const refresh = [...root.querySelectorAll('a, span, button')]
                            .find(n => /^\s*Refresh\s*$/i.test((n.textContent || '').trim()));
                        if (refresh) refresh.click();
                    }
                    """, trainNum);
            }

            await page.WaitForTimeoutAsync(refreshMs);
        }

        progress?.Report("Timed out on train list. Manually: class → date → Book Now (one click).");
        return false;
    }

    private static async Task<bool> ClickTrainClassAsync(
        IPage page,
        string trainNum,
        string classCode,
        IProgress<string>? progress)
    {
        var clicked = await page.EvaluateAsync<bool>("""
            ({ trainNum, classCode }) => {
                const roots = [...document.querySelectorAll('app-train-avl-enq')];
                const root = roots.find(el => {
                    const t = el.textContent || '';
                    return t.includes(trainNum) || t.replace(/\D/g, ' ').split(/\s+/).includes(trainNum);
                });
                if (!root) return false;
                root.scrollIntoView({ block: 'center' });

                const needle = '(' + classCode + ')';
                const codeRe = new RegExp('\\b' + classCode + '\\b', 'i');

                const boxes = [...root.querySelectorAll('.pre-avl')];
                for (const box of boxes) {
                    const text = (box.textContent || '').replace(/\s+/g, ' ').trim();
                    const strong = (box.querySelector('strong')?.textContent || '').trim();
                    const label = strong || text;
                    if (/\d{1,2}\s*,?\s*(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)/i.test(text) &&
                        /AVAILABLE|WL|RAC|REGRET|WAITING/i.test(text)) {
                        continue;
                    }
                    const matches =
                        label.includes(needle) ||
                        codeRe.test(label) ||
                        (classCode === 'SL' && /sleeper/i.test(label)) ||
                        (classCode === '3A' && /3\s*tier|ac\s*3/i.test(label)) ||
                        (classCode === '2A' && /2\s*tier|ac\s*2/i.test(label)) ||
                        (classCode === '1A' && /first|1\s*tier|ac\s*first/i.test(label)) ||
                        (classCode === 'CC' && /chair/i.test(label)) ||
                        (classCode === '3E' && /economy|3e/i.test(label));

                    if (!matches) continue;

                    const refresh = [...box.querySelectorAll('a, span, div, button')].find(n =>
                        /^\s*Refresh\s*$/i.test((n.textContent || '').trim()));
                    (refresh || box).click();
                    return true;
                }

                for (const tab of root.querySelectorAll('li[role="tab"] a, p-tabmenu a, .ui-tabview-nav a')) {
                    const t = (tab.textContent || '').replace(/\s+/g, ' ').trim();
                    if (t.includes(needle) || codeRe.test(t) || (classCode === 'SL' && /sleeper/i.test(t))) {
                        tab.click();
                        return true;
                    }
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
            var classBox = trainRoot.Locator(".pre-avl")
                .Filter(new LocatorFilterOptions { HasText = classCode })
                .First;
            if (await classBox.CountAsync() > 0)
            {
                var refresh = classBox.GetByText("Refresh", new LocatorGetByTextOptions { Exact = true });
                if (await refresh.CountAsync() > 0)
                {
                    await refresh.First.ClickAsync(new LocatorClickOptions { Force = true });
                }
                else
                {
                    await classBox.ClickAsync(new LocatorClickOptions { Force = true });
                }

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

        var classAttr = (await book.GetAttributeAsync("class")) ?? string.Empty;
        var disabled = await book.IsDisabledAsync()
                       || classAttr.Contains("disable-book", StringComparison.OrdinalIgnoreCase);
        if (disabled)
        {
            progress?.Report("Book Now still disabled — click a date card first.");
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

