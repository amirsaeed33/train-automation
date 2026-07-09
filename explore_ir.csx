using Microsoft.Playwright;

var pw = await Playwright.CreateAsync();
var browser = await pw.Chromium.LaunchAsync(new() { Headless = true });
var page = await (await browser.NewContextAsync()).NewPageAsync();
await page.GotoAsync("https://www.indianrail.gov.in/enquiry/TBIS/TrainBetweenImportantStations.html?locale=en", new() { Timeout = 120000, WaitUntil = WaitUntilState.DOMContentLoaded });
await page.WaitForTimeoutAsync(3000);
var html = await page.ContentAsync();
Console.WriteLine("TITLE: " + await page.TitleAsync());
var inputs = await page.EvaluateAsync<string>(@"() => Array.from(document.querySelectorAll('input,select,button')).map(e => e.tagName + ' id=' + e.id + ' name=' + e.name + ' type=' + e.type).join('\n')");
Console.WriteLine(inputs);
await browser.CloseAsync();
pw.Dispose();
