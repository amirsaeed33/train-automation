using System.Text;
using Microsoft.Playwright;

namespace train_automation;

public static class GhumoScraperTest
{
    public static async Task TestAsync(string from = "NDLS", string to = "HWH", string date = "")
    {
        if (string.IsNullOrEmpty(date))
            date = DateTime.Today.AddDays(5).ToString("yyyy-MM-dd");

        var sb = new StringBuilder();
        sb.AppendLine($"[GhumoTest] Searching {from} -> {to} on {date}");

        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            Headless = true
        });

        var page = await browser.NewPageAsync();

        var url = $"https://www.ghumo.live/seat-availability?from={from}&to={to}&date={date}";
        sb.AppendLine($"[GhumoTest] Opening: {url}");

        var apiCalls = new List<string>();
        page.Request += (_, req) =>
        {
            if (req.ResourceType is "fetch" or "xhr" || req.Url.Contains("api"))
            {
                apiCalls.Add($"{req.Method} {req.Url}");
            }
        };

        await page.GotoAsync(url, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.NetworkIdle,
            Timeout = 30_000
        });

        await page.WaitForTimeoutAsync(5000);

        sb.AppendLine($"\n[GhumoTest] API calls intercepted ({apiCalls.Count}):");
        foreach (var call in apiCalls)
            sb.AppendLine($"  -> {call}");

        var bodyText = await page.EvaluateAsync<string>("() => document.body.innerText");
        sb.AppendLine($"\n[GhumoTest] Page visible text (first 2000 chars):");
        sb.AppendLine(bodyText.Length > 2000 ? bodyText[..2000] : bodyText);

        System.IO.File.WriteAllText("D:\\github\\train-automation-desktop\\ghumo_test_output.txt", sb.ToString());
    }
}
