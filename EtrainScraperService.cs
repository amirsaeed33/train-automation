using Microsoft.Playwright;

namespace train_automation;

public sealed class EtrainScraperService : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;

    public async Task<IReadOnlyList<TrainResult>> SearchTrainsAsync(
        TrainSearchSettings settings,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report("Starting browser...");
        await EnsureBrowserAsync();

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
            await page.GotoAsync(settings.SiteUrl, new PageGotoOptions
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

            progress?.Report($"Setting route: {settings.FromStation} → {settings.ToStation}...");
            var stationsReady = await page.EvaluateAsync<bool>(
                """
                ({ fromQuery, toQuery }) => {
                    const findStation = (query) => {
                        const q = query.trim().toLowerCase();
                        if (!q) return null;

                        for (const [code, name] of Object.entries(stnname)) {
                            if (code.toLowerCase() === q) {
                                return { code, name };
                            }
                        }

                        for (const [code, name] of Object.entries(stnname)) {
                            if (name.toLowerCase().includes(q)) {
                                return { code, name };
                            }
                        }

                        return null;
                    };

                    const from = findStation(fromQuery);
                    const to = findStation(toQuery);
                    if (!from || !to) {
                        return false;
                    }

                    const setField = (visibleName, hiddenName, station) => {
                        const visible = document.querySelector(`input[name="${visibleName}"]`);
                        const hidden = document.querySelector(`input[name="${hiddenName}"]`);
                        if (!visible || !hidden) {
                            return false;
                        }

                        visible.value = station.name;
                        hidden.value = station.code;
                        visible.classList.remove('error');
                        return true;
                    };

                    return setField('station1', 'stn1', from) && setField('station2', 'stn2', to);
                }
                """,
                new { fromQuery = settings.FromStation, toQuery = settings.ToStation });

            if (!stationsReady)
            {
                throw new InvalidOperationException(
                    $"Could not resolve stations '{settings.FromStation}' and/or '{settings.ToStation}'.");
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
            var availabilityResponse = page.WaitForResponseAsync(
                response => response.Url.Contains("ajax.php", StringComparison.OrdinalIgnoreCase)
                    && response.Request.PostData?.Contains("avdata", StringComparison.OrdinalIgnoreCase) == true,
                new PageWaitForResponseOptions { Timeout = 90_000 });

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

            progress?.Report("Loading seat availability...");
            try
            {
                await availabilityResponse;
            }
            catch (TimeoutException)
            {
                await page.WaitForTimeoutAsync(3000);
            }

            var results = await page.EvaluateAsync<TrainResultDto[]>(
                """
                () => {
                    const table = document.querySelector('.myTable[contain="tbs"]');
                    if (!table) return [];

                    const dayCells = [7, 8, 9, 10, 11, 12, 13];
                    const dayNames = ['Sun', 'Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat'];
                    const rows = Array.from(table.querySelectorAll('tr'));
                    const results = [];

                    for (const row of rows) {
                        const cells = row.querySelectorAll('td');
                        if (cells.length < 6) continue;

                        const trainNumber = cells[0]?.textContent?.trim() ?? '';
                        if (!/^\d/.test(trainNumber)) continue;

                        const runsOn = dayCells
                            .map((index, dayIndex) => cells[index]?.textContent?.trim() === 'X' ? dayNames[dayIndex] : null)
                            .filter(Boolean)
                            .join(', ');

                        const availability = Array.from(row.querySelectorAll('a.cavlink'))
                            .map((link) => {
                                const cls = link.textContent.trim();
                                const status = link.classList.contains('avl') ? 'AVL'
                                    : link.classList.contains('rac') ? 'RAC'
                                    : link.classList.contains('wl') ? 'WL'
                                    : link.classList.contains('nill') ? 'NA'
                                    : '';
                                return status ? `${cls}:${status}` : cls;
                            })
                            .join(', ');

                        results.push({
                            trainNumber,
                            trainName: cells[1]?.textContent?.trim() ?? '',
                            fromStation: cells[2]?.textContent?.trim() ?? '',
                            departure: cells[3]?.textContent?.trim() ?? '',
                            toStation: cells[4]?.textContent?.trim() ?? '',
                            arrival: cells[5]?.textContent?.trim() ?? '',
                            duration: cells[6]?.textContent?.trim() ?? '',
                            runsOn,
                            availability
                        });
                    }

                    return results;
                }
                """);

            progress?.Report($"Found {results.Length} train(s).");
            return results.Select(MapResult).ToList();
        }
        finally
        {
            await context.CloseAsync();
        }
    }

    private static TrainResult MapResult(TrainResultDto dto) => new()
    {
        TrainNumber = dto.TrainNumber ?? string.Empty,
        TrainName = dto.TrainName ?? string.Empty,
        FromStation = dto.FromStation ?? string.Empty,
        Departure = dto.Departure ?? string.Empty,
        ToStation = dto.ToStation ?? string.Empty,
        Arrival = dto.Arrival ?? string.Empty,
        Duration = dto.Duration ?? string.Empty,
        RunsOn = dto.RunsOn ?? string.Empty,
        Availability = dto.Availability ?? string.Empty
    };

    private async Task EnsureBrowserAsync()
    {
        if (_browser is not null)
        {
            return;
        }

        _playwright = await Playwright.CreateAsync();
        _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });
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

    private sealed class TrainResultDto
    {
        public string? TrainNumber { get; set; }
        public string? TrainName { get; set; }
        public string? FromStation { get; set; }
        public string? Departure { get; set; }
        public string? ToStation { get; set; }
        public string? Arrival { get; set; }
        public string? Duration { get; set; }
        public string? RunsOn { get; set; }
        public string? Availability { get; set; }
    }
}
