using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Playwright;

namespace train_automation;

public sealed class GhumoScraperService : IAsyncDisposable
{
    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;

    public async Task<IReadOnlyList<TrainResult>> SearchTrainsAsync(
        TrainSearchSettings settings,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report("Initializing browser for ghumo.live...");
        _playwright ??= await Playwright.CreateAsync();

        try
        {
            _browser ??= await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true, // Set to true as requested to hide the browser window
                Args = ["--disable-blink-features=AutomationControlled"]
            });
        }
        catch (PlaywrightException ex) when (ex.Message.Contains("Executable doesn't exist", StringComparison.OrdinalIgnoreCase))
        {
            progress?.Report("Installing browser for first run (one-time setup)...");
            var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
            if (exitCode != 0)
                throw new InvalidOperationException("Playwright browser install failed. Run playwright.ps1 install chromium.");

            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true,
                Args = ["--disable-blink-features=AutomationControlled"]
            });
        }

        // Fresh context per search to avoid session reuse issues
        if (_context != null) await _context.CloseAsync();

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 }
        });

        // Stealth trick
        await _context.AddInitScriptAsync("Object.defineProperty(navigator, 'webdriver', {get: () => undefined})");

        var page = await _context.NewPageAsync();
        page.SetDefaultTimeout(60_000);
        page.SetDefaultNavigationTimeout(60_000);

        var dateStr = settings.TravelDate.ToString("dd-MM-yyyy");
        var url = $"https://www.ghumo.live/?from={settings.FromStationCode}&to={settings.ToStationCode}&date={dateStr}";

        progress?.Report($"Opening {url} ...");
        
        // Wait until load - user might have to watch it bypass Turnstile
        await page.GotoAsync(url, new PageGotoOptions { WaitUntil = WaitUntilState.DOMContentLoaded });
        progress?.Report("Bypassing security. Waiting for trains to load...");

        // Ghumo populates the fields from the URL.
        // We will perfectly locate the Search button by looking for the large circular icon button in the header.
        try
        {
            await page.WaitForTimeoutAsync(2000); 
            
            var coords = await page.EvaluateAsync<System.Text.Json.JsonElement>(@"() => {
                const clickables = Array.from(document.querySelectorAll('button, div, a, span'));
                let searchBtn = null;
                let maxX = 0;

                for (let el of clickables) {
                    const rect = el.getBoundingClientRect();
                    // 1. Must be in the top search bar area (Y < 120)
                    if (rect.y < 120) {
                        // 2. Search button is typically a large circle/square (Width ~30-60px)
                        if (rect.width >= 30 && rect.width <= 60 && Math.abs(rect.width - rect.height) < 5) {
                            // 3. Must contain an icon (SVG)
                            if (el.innerHTML.includes('svg')) {
                                // 4. The search button is the furthest right circular button in the main center area (X between 500 and 1000)
                                // This ignores the smaller 'clear' (x) buttons which are < 30px, 
                                // ignores the 'Quota' and 'Date' pills which are rectangles, 
                                // and picks the Search button over the 'Swap' button because Search has a higher X.
                                if (rect.x > 300 && rect.x < 1100) {
                                    if (rect.x > maxX) {
                                        maxX = rect.x;
                                        searchBtn = el;
                                    }
                                }
                            }
                        }
                    }
                }

                if (searchBtn) {
                    const rect = searchBtn.getBoundingClientRect();
                    return { x: rect.x + (rect.width / 2), y: rect.y + (rect.height / 2) };
                }
                return null;
            }");

            if (coords.ValueKind != System.Text.Json.JsonValueKind.Null && coords.TryGetProperty("x", out var xProp) && coords.TryGetProperty("y", out var yProp))
            {
                float x = (float)xProp.GetDouble();
                float y = (float)yProp.GetDouble();
                progress?.Report("Found Search button using spatial scanning. Clicking...");
                
                // Move mouse slowly to simulate human behavior
                await page.Mouse.MoveAsync(x, y, new Microsoft.Playwright.MouseMoveOptions { Steps = 15 });
                await page.WaitForTimeoutAsync(500);
                await page.Mouse.DownAsync();
                await page.WaitForTimeoutAsync(100);
                await page.Mouse.UpAsync();
            }
        }
        catch { }

        try
        {
            // Soft wait for the result cards
            await page.WaitForSelectorAsync("text=/", new PageWaitForSelectorOptions { Timeout = 15_000 });
        }
        catch { } 

        await page.WaitForTimeoutAsync(5000); // give React/Nextjs time to render

        progress?.Report("If trains didn't load, please click the Search icon manually in the browser...");
        // Extra wait in case user needs to click it manually
        await page.WaitForTimeoutAsync(4000);

        await page.WaitForTimeoutAsync(5000); // give React/Nextjs time to render

        progress?.Report("Extracting train details...");

        var results = await page.EvaluateAsync<TrainResultDto[]>("""
            () => {
                const elements = Array.from(document.querySelectorAll('*'));
                const trainNoRegex = /^\d{5}$/;
                const trainCards = new Set();
                
                elements.forEach(el => {
                    const text = el.innerText?.trim();
                    if (text && trainNoRegex.test(text)) {
                        let parent = el.parentElement;
                        for(let i=0; i<5 && parent; i++) {
                            if (parent.innerText.length > 50 && parent.innerText.includes(':')) {
                                trainCards.add(parent);
                                break;
                            }
                            parent = parent.parentElement;
                        }
                    }
                });

                const parsed = [];
                trainCards.forEach(card => {
                    const textContent = card.innerText;
                    const trainNoMatch = textContent.match(/(\d{5})/);
                    const timeMatch = [...textContent.matchAll(/(\d{1,2}:\d{2})/g)];
                    
                    if (trainNoMatch) {
                        parsed.push({
                            trainNumber: trainNoMatch[1],
                            trainName: textContent.split('\n').find(l => !/^\d{5}$/.test(l) && l.length > 5) || "Unknown Train",
                            departure: timeMatch[0] ? timeMatch[0][1] : "",
                            arrival: timeMatch[1] ? timeMatch[1][1] : "",
                            fromStation: "",
                            toStation: "",
                            travelTime: "",
                            availableClasses: "SL, 3A, 2A" // Fallback since UI parsing is highly variant
                        });
                    }
                });

                return parsed;
            }
        """);

        progress?.Report($"Found {results.Length} train(s) via ghumo.live.");
        
        return results.Select(dto => new TrainResult
        {
            TrainNumber = dto.TrainNumber ?? string.Empty,
            TrainName = dto.TrainName ?? "Unknown",
            Departure = dto.Departure ?? string.Empty,
            Arrival = dto.Arrival ?? string.Empty,
            FromStation = settings.FromStationCode,
            ToStation = settings.ToStationCode,
            AvailableClasses = dto.AvailableClasses ?? "SL",
            ClassLinkKeys = new Dictionary<string, string>()
        }).ToList();
    }

    public Task<ClassAvailabilityResult> GetClassAvailabilityAsync(
        string quotaCode,
        string trainNumber,
        string travelClass,
        string? classLinkKey = null,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        // For now, return dummy since we just replaced the 'Find' button
        return Task.FromResult(new ClassAvailabilityResult
        {
            TotalFare = "Check IRCTC",
            AvailabilityDays = new List<AvailabilityDay>
            {
                new() { Date = DateTime.Today.ToString("dd-MM-yyyy"), Status = "See IRCTC" }
            }
        });
    }

    public async ValueTask DisposeAsync()
    {
        if (_context != null) await _context.CloseAsync();
        if (_browser != null) await _browser.CloseAsync();
    }

    private class TrainResultDto
    {
        public string? TrainNumber { get; set; }
        public string? TrainName { get; set; }
        public string? Departure { get; set; }
        public string? Arrival { get; set; }
        public string? FromStation { get; set; }
        public string? ToStation { get; set; }
        public string? TravelTime { get; set; }
        public string? AvailableClasses { get; set; }
    }
}
