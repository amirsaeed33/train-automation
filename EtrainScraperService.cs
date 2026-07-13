using Microsoft.Playwright;

namespace train_automation;

public sealed class EtrainScraperService : IAsyncDisposable
{
    private const string EtrainTrainsUrl = "https://etrain.info/trains";

    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async Task<IReadOnlyList<TrainResult>> SearchTrainsAsync(
        TrainSearchSettings settings,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report("Starting browser...");
        await EnsureBrowserAsync(progress);

        var context = await _browser!.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 },
            Locale = "en-IN"
        });

        var page = await context.NewPageAsync();
        try
        {
            progress?.Report("Opening etrain.info...");
            await page.GotoAsync(EtrainTrainsUrl, new PageGotoOptions
            {
                WaitUntil = WaitUntilState.DOMContentLoaded,
                Timeout = 90_000
            });

            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report("Waiting for station data...");
            await page.WaitForFunctionAsync(
                "() => typeof stnname !== 'undefined' && Object.keys(stnname).length > 0",
                null,
                new PageWaitForFunctionOptions { Timeout = 90_000 });

            progress?.Report($"Setting route: {settings.FromStationName} → {settings.ToStationName}...");
            var stationsReady = await page.EvaluateAsync<bool>(
                """
                ({ fromCode, fromName, toCode, toName }) => {
                    const setField = (visibleName, hiddenName, code, name) => {
                        const visible = document.querySelector(`input[name="${visibleName}"]`);
                        const hidden = document.querySelector(`input[name="${hiddenName}"]`);
                        if (!visible || !hidden) {
                            return false;
                        }

                        visible.value = name;
                        hidden.value = code;
                        visible.classList.remove('error');
                        return true;
                    };

                    return setField('station1', 'stn1', fromCode, fromName)
                        && setField('station2', 'stn2', toCode, toName);
                }
                """,
                new
                {
                    fromCode = settings.FromStationCode,
                    fromName = settings.FromStationName,
                    toCode = settings.ToStationCode,
                    toName = settings.ToStationName
                });

            if (!stationsReady)
            {
                throw new InvalidOperationException(
                    $"Could not set stations '{settings.FromStationName}' and/or '{settings.ToStationName}'.");
            }

            await page.EvaluateAsync(
                """
                (travelDateIso) => {
                    const input = document.querySelector('#bwstnform input[name="date"]');
                    if (!input || typeof jQuery === 'undefined' || !jQuery.fn.setCalDate) {
                        return;
                    }

                    jQuery(input).setCalDate(new Date(travelDateIso));
                }
                """,
                settings.TravelDate.ToString("o"));

            var quotaSelect = page.Locator("#bwstnform select[name='quota']");
            if (await quotaSelect.CountAsync() > 0)
            {
                await quotaSelect.SelectOptionAsync(settings.Quota);
            }

            cancellationToken.ThrowIfCancellationRequested();

            progress?.Report("Searching trains...");
            await page.ClickAsync("#tbssbmtbtn");

            await page.WaitForFunctionAsync(
                """
                () => {
                    const table = document.querySelector('.myTable[contain="tbs"]');
                    if (!table) return false;

                    const rows = table.querySelectorAll('tr');
                    for (const row of rows) {
                        const firstCell = row.querySelector('td');
                        if (firstCell && /^\d/.test(firstCell.textContent.trim())) {
                            return true;
                        }
                    }

                    return false;
                }
                """,
                null,
                new PageWaitForFunctionOptions { Timeout = 90_000 });

            progress?.Report("Loading train details...");
            try
            {
                await page.WaitForResponseAsync(
                    response => response.Url.Contains("ajax.php", StringComparison.OrdinalIgnoreCase)
                        && response.Request.PostData?.Contains("avdata", StringComparison.OrdinalIgnoreCase) == true,
                    new PageWaitForResponseOptions { Timeout = 8_000 });
            }
            catch (TimeoutException)
            {
                // Short/local routes (e.g. NDLS -> DLI) may only list UNRESERVED classes.
            }

            await page.WaitForTimeoutAsync(1000);

            var results = await page.EvaluateAsync<TrainResultDto[]>(
                """
                () => {
                    const table = document.querySelector('.myTable[contain="tbs"]');
                    if (!table) return [];

                    const rows = Array.from(table.querySelectorAll('tr'));
                    const results = [];

                    const dayValue = (cell) => {
                        const value = cell?.textContent?.trim() ?? '';
                        return value === 'X' || value === 'Y' ? value : '';
                    };

                    for (const row of rows) {
                        const cells = row.querySelectorAll('td');
                        if (cells.length < 14) continue;

                        const trainNumber = cells[0]?.textContent?.trim() ?? '';
                        if (!/^\d/.test(trainNumber)) continue;

                        const availableClasses = Array.from(row.querySelectorAll('a.cavlink, span.inputimg'))
                            .map((element) => element.textContent.trim())
                            .filter(Boolean)
                            .join(', ');

                        results.push({
                            trainNumber,
                            trainName: cells[1]?.textContent?.trim() ?? '',
                            fromStation: cells[2]?.textContent?.trim() ?? '',
                            departure: cells[3]?.textContent?.trim() ?? '',
                            toStation: cells[4]?.textContent?.trim() ?? '',
                            arrival: cells[5]?.textContent?.trim() ?? '',
                            travelTime: cells[6]?.textContent?.trim() ?? '',
                            sunday: dayValue(cells[7]),
                            monday: dayValue(cells[8]),
                            tuesday: dayValue(cells[9]),
                            wednesday: dayValue(cells[10]),
                            thursday: dayValue(cells[11]),
                            friday: dayValue(cells[12]),
                            saturday: dayValue(cells[13]),
                            availableClasses
                        });
                    }

                    return results;
                }
                """);

            progress?.Report($"Found {results.Length} train(s).");
            return results.Select(result => MapResult(result, settings)).ToList();
        }
        finally
        {
            await context.CloseAsync();
        }
    }

    private static TrainResult MapResult(TrainResultDto dto, TrainSearchSettings settings)
    {
        var trainNumber = dto.TrainNumber ?? string.Empty;
        var availableClasses = dto.AvailableClasses ?? string.Empty;

        return new TrainResult
        {
            TrainNumber = trainNumber,
            TrainName = dto.TrainName ?? string.Empty,
            FromStation = dto.FromStation ?? string.Empty,
            Departure = dto.Departure ?? string.Empty,
            ToStation = dto.ToStation ?? string.Empty,
            Arrival = dto.Arrival ?? string.Empty,
            TravelTime = dto.TravelTime ?? string.Empty,
            Sunday = dto.Sunday ?? string.Empty,
            Monday = dto.Monday ?? string.Empty,
            Tuesday = dto.Tuesday ?? string.Empty,
            Wednesday = dto.Wednesday ?? string.Empty,
            Thursday = dto.Thursday ?? string.Empty,
            Friday = dto.Friday ?? string.Empty,
            Saturday = dto.Saturday ?? string.Empty,
            AvailableClasses = availableClasses,
            ClassLinkKeys = CreateClassLinkKeys(trainNumber, availableClasses, settings)
        };
    }

    private static IReadOnlyDictionary<string, string> CreateClassLinkKeys(
        string trainNumber,
        string availableClasses,
        TrainSearchSettings settings)
    {
        var fromText = $"{settings.FromStationName} - {settings.FromStationCode}";
        var toText = $"{settings.ToStationName} - {settings.ToStationCode}";
        var travelDate = settings.TravelDate.ToString("dd-MM-yyyy");

        return availableClasses
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                classCode => classCode,
                classCode => $"{trainNumber}^{classCode}^{toText}^{fromText}^{travelDate}^",
                StringComparer.OrdinalIgnoreCase);
    }

    private async Task EnsureBrowserAsync(IProgress<string>? progress = null)
    {
        if (_browser is not null)
        {
            return;
        }

        _playwright ??= await Playwright.CreateAsync();

        try
        {
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }
        catch (PlaywrightException ex) when (IsMissingBrowserError(ex))
        {
            progress?.Report("Installing browser for first run (one-time setup)...");
            var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    "Playwright browser install failed. Run this in PowerShell:\n" +
                    "powershell -ExecutionPolicy Bypass -File \"bin\\Debug\\net9.0-windows\\playwright.ps1\" install chromium");
            }

            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });
        }
    }

    private static bool IsMissingBrowserError(PlaywrightException ex) =>
        ex.Message.Contains("Executable doesn't exist", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("Please run the following command", StringComparison.OrdinalIgnoreCase);

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

    private sealed class TrainResultDto
    {
        public string? TrainNumber { get; set; }
        public string? TrainName { get; set; }
        public string? FromStation { get; set; }
        public string? Departure { get; set; }
        public string? ToStation { get; set; }
        public string? Arrival { get; set; }
        public string? TravelTime { get; set; }
        public string? Sunday { get; set; }
        public string? Monday { get; set; }
        public string? Tuesday { get; set; }
        public string? Wednesday { get; set; }
        public string? Thursday { get; set; }
        public string? Friday { get; set; }
        public string? Saturday { get; set; }
        public string? AvailableClasses { get; set; }
    }
}
