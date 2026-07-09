using System.Text.Json;
using Microsoft.Playwright;

namespace train_automation;

public sealed class IndianRailScraperService : IAsyncDisposable
{
    private const string SiteUrl =
        "https://www.indianrail.gov.in/enquiry/TBIS/TrainBetweenImportantStations.html?locale=en";

    private IPlaywright? _playwright;
    private IBrowser? _browser;
    private IBrowserContext? _context;
    private IPage? _page;
    private bool _captchaEnabled = true;

    public Func<IWin32Window?, byte[], Task<string?>>? CaptchaProvider { get; set; }

    public IWin32Window? DialogOwner { get; set; }

    private TrainSearchSettings? _activeSessionSettings;

    public bool HasActiveSessionFor(TrainSearchSettings settings) =>
        _page is not null
        && _activeSessionSettings is not null
        && _activeSessionSettings.FromStationCode.Equals(settings.FromStationCode, StringComparison.OrdinalIgnoreCase)
        && _activeSessionSettings.ToStationCode.Equals(settings.ToStationCode, StringComparison.OrdinalIgnoreCase)
        && _activeSessionSettings.TravelDate.Date == settings.TravelDate.Date
        && _activeSessionSettings.Quota.Equals(settings.Quota, StringComparison.OrdinalIgnoreCase);

    public async Task<IReadOnlyList<TrainResult>> SearchTrainsAsync(
        TrainSearchSettings settings,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report("Opening Indian Railways enquiry...");
        await EnsurePageAsync(progress);

        var page = _page!;
        await page.GotoAsync(SiteUrl, new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 120_000
        });

        cancellationToken.ThrowIfCancellationRequested();
        await page.WaitForTimeoutAsync(1500);

        _captchaEnabled = await page.EvaluateAsync<string>(
            "async () => (await fetch('/enquiry/CaptchaConfig?_=' + Date.now())).text()") != "0";

        var fromText = $"{settings.FromStationName} - {settings.FromStationCode}";
        var toText = $"{settings.ToStationName} - {settings.ToStationCode}";
        var travelDate = settings.TravelDate.ToString("dd-MM-yyyy");

        await page.EvaluateAsync(
            """
            ({ fromText, toText, travelDate, quota }) => {
              const dt = document.querySelector('#dt');
              if (dt) {
                dt.removeAttribute('readonly');
                dt.value = travelDate;
              }
              const src = document.querySelector('#sourceStation');
              const dst = document.querySelector('#destinationStation');
              if (src) src.value = fromText;
              if (dst) dst.value = toText;
              const quotaSelect = document.querySelector('#quota');
              if (quotaSelect) quotaSelect.value = quota;
            }
            """,
            new { fromText, toText, travelDate, quota = settings.Quota });

        progress?.Report("Searching trains...");
        var responseJson = await SubmitCaptchaRequestAsync(
            page,
            new Dictionary<string, string>
            {
                ["inputPage"] = "TBIS",
                ["dt"] = travelDate,
                ["sourceStation"] = fromText,
                ["destinationStation"] = toText,
                ["flexiWithDate"] = "y",
                ["language"] = "en"
            },
            triggerFirstCaptcha: true,
            cancellationToken: cancellationToken);

        try
        {
            await page.WaitForSelectorAsync("a#cl", new PageWaitForSelectorOptions { Timeout = 15_000 });
        }
        catch (TimeoutException)
        {
            // Table may still be usable from JSON even if links are not rendered yet.
        }

        var results = ParseTrainList(responseJson, settings);
        var enriched = await EnrichClassLinkKeysFromPageAsync(page, results);
        _activeSessionSettings = settings;
        return enriched;
    }

    public async Task<ClassAvailabilityResult> GetClassAvailabilityAsync(
        string quotaCode,
        string trainNumber,
        string travelClass,
        string? classLinkKey = null,
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        if (_page is null)
        {
            throw new InvalidOperationException("Search trains before loading class availability.");
        }

        progress?.Report("Loading fare and availability...");
        var linkKey = classLinkKey ?? await ResolveClassLinkKeyFromPageAsync(trainNumber, travelClass);
        if (string.IsNullOrWhiteSpace(linkKey))
        {
            throw new InvalidOperationException($"Could not find class {travelClass} for train {trainNumber}.");
        }

        var parts = linkKey.Split('^');
        if (parts.Length < 6)
        {
            throw new InvalidOperationException("Invalid class selection key.");
        }

        await _page.EvaluateAsync(
            """
            ({ quota }) => {
              const quotaSelect = document.querySelector('#quota');
              if (quotaSelect) quotaSelect.value = quota;
            }
            """,
            new { quota = quotaCode });

        await TryClickClassLinkAsync(trainNumber, travelClass, linkKey);

        var responseJson = await SubmitCaptchaRequestAsync(
            _page,
            CreateFarePayload(parts, quotaCode),
            triggerFirstCaptcha: false,
            classLinkKey: linkKey,
            cancellationToken);

        var result = responseJson.ValueKind == JsonValueKind.Object
            ? ParseClassAvailability(responseJson, parts[0], parts[1], parts[4])
            : new ClassAvailabilityResult
            {
                TrainNumber = parts[0],
                TravelClass = parts[1],
                TravelDate = parts[4]
            };
        if (IsEmptyAvailability(result))
        {
            result = await ScrapeFareFromPageAsync(_page, parts[0], parts[1], parts[4]);
        }

        if (IsEmptyAvailability(result))
        {
            throw new InvalidOperationException("Could not load fare and availability. Please check captcha and retry.");
        }

        return result;
    }

    private static bool IsEmptyAvailability(ClassAvailabilityResult result) =>
        string.IsNullOrWhiteSpace(result.TotalFare)
        && string.IsNullOrWhiteSpace(result.BaseFare)
        && result.AvailabilityDays.Count == 0;

    private static async Task<ClassAvailabilityResult> ScrapeFareFromPageAsync(
        IPage page,
        string trainNumber,
        string travelClass,
        string travelDate)
    {
        await page.WaitForTimeoutAsync(1200);

        var scraped = await page.EvaluateAsync<PageFareScrape>(
            """
            () => {
              const result = {
                baseFare: '',
                reservationCharges: '',
                superfastCharges: '',
                otherCharges: '',
                tatkalCharges: '',
                goodsServiceTax: '',
                cateringCharge: '',
                dynamicFare: '',
                totalFare: '',
                days: []
              };

              const tables = Array.from(document.querySelectorAll('table'));
              for (const table of tables) {
                const text = (table.innerText || '').replace(/\\s+/g, ' ');
                if (text.includes('Base Fare') || text.includes('Total Fare')) {
                  const headers = Array.from(table.querySelectorAll('th')).map(th => (th.textContent || '').trim());
                  const valueRow = table.querySelector('tbody tr, tr:nth-child(2)');
                  const values = valueRow
                    ? Array.from(valueRow.querySelectorAll('td')).map(td => (td.textContent || '').trim())
                    : [];
                  const pick = (label) => {
                    const index = headers.findIndex(h => h.toLowerCase().includes(label));
                    return index >= 0 && values[index] ? values[index] : '';
                  };
                  result.baseFare = pick('base');
                  result.reservationCharges = pick('reservation');
                  result.superfastCharges = pick('superfast');
                  result.otherCharges = pick('other');
                  result.tatkalCharges = pick('tatkal');
                  result.goodsServiceTax = pick('tax');
                  result.cateringCharge = pick('catering');
                  result.dynamicFare = pick('dynamic');
                  result.totalFare = pick('total');
                }

                if (text.toLowerCase().includes('availability')) {
                  const rows = Array.from(table.querySelectorAll('tr'));
                  if (rows.length >= 2) {
                    const dateCells = Array.from(rows[0].querySelectorAll('td, th')).slice(1);
                    const avlCells = Array.from(rows[1].querySelectorAll('td, th')).slice(1);
                    for (let i = 0; i < dateCells.length; i++) {
                      const date = (dateCells[i].textContent || '').trim();
                      const status = (avlCells[i]?.textContent || '').trim();
                      if (date) {
                        result.days.push({ date, status });
                      }
                    }
                  }
                }
              }

              return result;
            }
            """);

        return new ClassAvailabilityResult
        {
            TrainNumber = trainNumber,
            TravelClass = travelClass,
            TravelDate = travelDate,
            BaseFare = scraped.BaseFare,
            ReservationCharges = scraped.ReservationCharges,
            SuperfastCharges = scraped.SuperfastCharges,
            OtherCharges = scraped.OtherCharges,
            TatkalCharges = scraped.TatkalCharges,
            GoodsServiceTax = scraped.GoodsServiceTax,
            CateringCharge = scraped.CateringCharge,
            DynamicFare = scraped.DynamicFare,
            TotalFare = scraped.TotalFare,
            AvailabilityDays = scraped.Days.Select(day => new AvailabilityDay
            {
                Date = day.Date,
                Status = day.Status
            }).ToList()
        };
    }

    private static Dictionary<string, string> CreateFarePayload(string[] parts, string quotaCode) =>
        new()
        {
            ["inputPage"] = "TBIS_CALL_FOR_FARE",
            ["trainNo"] = parts[0],
            ["classc"] = parts[1],
            ["destinationStation"] = parts[2],
            ["sourceStation"] = parts[3],
            ["dt"] = parts[4],
            ["traintype"] = parts[5],
            ["quota"] = quotaCode,
            ["language"] = "en"
        };

    private async Task TryClickClassLinkAsync(string trainNumber, string travelClass, string linkKey)
    {
        try
        {
            var clicked = await _page!.EvaluateAsync<bool>(
                """
                ({ trainNo, classCode, linkKey }) => {
                  const links = Array.from(document.querySelectorAll('#cl'));
                  const match = links.find(link => {
                    const name = link.getAttribute('name') || '';
                    if (linkKey && name === linkKey) return true;
                    const parts = name.split('^');
                    return parts[0] === trainNo && parts[1] === classCode;
                  });
                  if (!match) return false;
                  match.click();
                  return true;
                }
                """,
                new { trainNo = trainNumber, classCode = travelClass, linkKey });

            if (!clicked)
            {
                var linkLocator = _page.Locator($"a#cl[name^='{trainNumber}^{travelClass}^']");
                if (await linkLocator.CountAsync() > 0)
                {
                    await linkLocator.First.ClickAsync(new LocatorClickOptions { Timeout = 3000 });
                }
            }
        }
        catch
        {
            // Click is optional; fare is loaded via API using the class link key.
        }
    }

    private async Task<string?> ResolveClassLinkKeyFromPageAsync(string trainNumber, string travelClass)
    {
        return await _page!.EvaluateAsync<string?>(
            """
            ({ trainNo, classCode }) => {
              const links = Array.from(document.querySelectorAll('#cl'));
              const match = links.find(link => {
                const name = link.getAttribute('name') || '';
                const parts = name.split('^');
                return parts[0] === trainNo && parts[1] === classCode;
              });
              return match ? match.getAttribute('name') : null;
            }
            """,
            new { trainNo = trainNumber, classCode = travelClass });
    }

    private static async Task<IReadOnlyList<TrainResult>> EnrichClassLinkKeysFromPageAsync(
        IPage page,
        IReadOnlyList<TrainResult> results)
    {
        var pageLinks = await page.EvaluateAsync<PageClassLink[]>(
            """
            () => Array.from(document.querySelectorAll('#cl')).map(link => {
              const name = link.getAttribute('name') || '';
              const parts = name.split('^');
              return { trainNo: parts[0] || '', classCode: parts[1] || '', name };
            })
            """);

        return results.Select(train =>
        {
            var links = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var classCode in train.ClassLinkKeys.Keys)
            {
                var pageLink = pageLinks.FirstOrDefault(link =>
                    link.TrainNo.Equals(train.TrainNumber, StringComparison.OrdinalIgnoreCase)
                    && link.ClassCode.Equals(classCode, StringComparison.OrdinalIgnoreCase));
                links[classCode] = pageLink?.Name ?? train.ClassLinkKeys[classCode];
            }

            return new TrainResult
            {
                TrainNumber = train.TrainNumber,
                TrainName = train.TrainName,
                FromStation = train.FromStation,
                Departure = train.Departure,
                ToStation = train.ToStation,
                Arrival = train.Arrival,
                TravelTime = train.TravelTime,
                Sunday = train.Sunday,
                Monday = train.Monday,
                Tuesday = train.Tuesday,
                Wednesday = train.Wednesday,
                Thursday = train.Thursday,
                Friday = train.Friday,
                Saturday = train.Saturday,
                AvailableClasses = train.AvailableClasses,
                ClassLinkKeys = links
            };
        }).ToList();
    }

    private async Task<JsonElement> SubmitCaptchaRequestAsync(
        IPage page,
        Dictionary<string, string> payload,
        bool triggerFirstCaptcha,
        string? classLinkKey = null,
        CancellationToken cancellationToken = default)
    {
        var preferManualCaptcha = false;

        for (var attempt = 0; attempt < 5; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_captchaEnabled)
            {
                if (triggerFirstCaptcha)
                {
                    if (attempt == 0)
                    {
                        await page.EvaluateAsync("() => document.getElementById('modal1')?.click()");
                        await page.WaitForTimeoutAsync(700);
                    }
                    else
                    {
                        await RefreshCaptchaImageAsync(page);
                    }
                }
                else
                {
                    var modalVisible = await page.Locator("#myModal").IsVisibleAsync();
                    if (!modalVisible || attempt > 0)
                    {
                        await ShowSecondCaptchaModalAsync(page, classLinkKey);
                    }
                }

                var captchaBytes = await page.Locator("#CaptchaImgID").ScreenshotAsync();
                var answer = await ResolveCaptchaAnswerAsync(captchaBytes, allowAuto: !preferManualCaptcha);

                if (string.IsNullOrWhiteSpace(answer))
                {
                    throw new InvalidOperationException("Captcha entry was cancelled.");
                }

                await page.FillAsync("#inputCaptcha", answer);
            }
            else if (triggerFirstCaptcha && attempt == 0)
            {
                await page.EvaluateAsync("() => document.getElementById('modal1')?.click()");
                await page.WaitForTimeoutAsync(700);
            }

            JsonElement json;
            if (triggerFirstCaptcha)
            {
                var responseTask = page.WaitForResponseAsync(
                    response => response.Url.Contains("CommonCaptcha", StringComparison.OrdinalIgnoreCase),
                    new PageWaitForResponseOptions { Timeout = 90_000 });
                await page.ClickAsync("#test");
                var response = await responseTask;
                json = await ReadJsonResponseAsync(response);
            }
            else
            {
                json = await SubmitFareRequestAsync(page, payload, classLinkKey, cancellationToken);
            }

            if (json.ValueKind != JsonValueKind.Object)
            {
                continue;
            }

            if (json.TryGetProperty("flag", out var flag) && flag.GetString() == "NO")
            {
                preferManualCaptcha = true;
                await RefreshCaptchaImageAsync(page);
                continue;
            }

            if (json.TryGetProperty("errorMessage", out var errorMessage)
                && !string.IsNullOrWhiteSpace(errorMessage.GetString()))
            {
                var message = errorMessage.GetString()!;
                if (IsCaptchaMismatch(message))
                {
                    preferManualCaptcha = true;
                    await RefreshCaptchaImageAsync(page);
                    continue;
                }

                throw new InvalidOperationException(message);
            }

            return json;
        }

        if (!triggerFirstCaptcha)
        {
            return default;
        }

        throw new InvalidOperationException("Captcha validation failed. Please try again.");
    }

    private static bool IsCaptchaMismatch(string message) =>
        message.Contains("captcha", StringComparison.OrdinalIgnoreCase);

    private static async Task RefreshCaptchaImageAsync(IPage page)
    {
        await page.EvaluateAsync(
            """
            () => {
              const input = document.getElementById('inputCaptcha');
              if (input) input.value = '';
              const img = document.getElementById('CaptchaImgID');
              if (img) img.src = '../captchaDraw.png?' + Date.now();
            }
            """);
        await page.WaitForTimeoutAsync(500);
    }

    private static async Task ShowSecondCaptchaModalAsync(IPage page, string? classLinkKey)
    {
        await page.EvaluateAsync(
            """
            ({ linkKey }) => {
              const captchaNumber = document.getElementById('captchaNumber');
              if (captchaNumber) captchaNumber.value = 'SECOND';

              const flagValue = document.getElementById('flagValue');
              const flagLabel = document.getElementById('flagLabel');
              if (flagValue && linkKey) flagValue.value = linkKey;
              if (flagLabel) flagLabel.value = 'CLASS';

              const img = document.getElementById('CaptchaImgID');
              if (img) img.src = '../captchaDraw.png?' + Date.now();

              if (window.jQuery) {
                jQuery('#myModal').modal('show');
              }
            }
            """,
            new { linkKey = classLinkKey ?? string.Empty });
        await page.WaitForTimeoutAsync(700);
    }

    private async Task<JsonElement> SubmitFareRequestAsync(
        IPage page,
        Dictionary<string, string> payload,
        string? classLinkKey,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        payload["inputCaptcha"] = await page.InputValueAsync("#inputCaptcha");
        payload["captchaNumber"] = "SECOND";

        if (!string.IsNullOrWhiteSpace(classLinkKey))
        {
            var linkParts = classLinkKey.Split('^');
            if (linkParts.Length >= 6)
            {
                payload["trainNo"] = linkParts[0];
                payload["classc"] = linkParts[1];
                payload["destinationStation"] = linkParts[2];
                payload["sourceStation"] = linkParts[3];
                payload["dt"] = linkParts[4];
                payload["traintype"] = linkParts[5];
            }
        }

        var ajaxJson = await page.EvaluateAsync<string>(
            """
            async (payload) => {
              if (typeof jQuery === 'undefined') {
                return '';
              }

              return await new Promise(resolve => {
                jQuery.ajax({
                  url: '../CommonCaptcha',
                  type: 'POST',
                  data: payload,
                  success: function(resp) {
                    try {
                      resolve(JSON.stringify(resp));
                    } catch (e) {
                      resolve('');
                    }
                  },
                  error: function(xhr) {
                    resolve(xhr.responseText || '');
                  }
                });
              });
            }
            """,
            payload);

        if (!string.IsNullOrWhiteSpace(ajaxJson))
        {
            try
            {
                return ParseJsonElement(ajaxJson);
            }
            catch (InvalidOperationException)
            {
                // Response may have rendered HTML tabs only; scrape from page below.
            }
        }

        try
        {
            var responseTask = page.WaitForResponseAsync(
                response => response.Url.Contains("CommonCaptcha", StringComparison.OrdinalIgnoreCase),
                new PageWaitForResponseOptions { Timeout = 20_000 });

            if (await page.Locator("#running").CountAsync() > 0)
            {
                await page.ClickAsync("#running");
            }
            else if (await page.Locator("#myModal button, #myModal input[type=button]").CountAsync() > 0)
            {
                await page.Locator("#myModal button, #myModal input[type=button]").Last.ClickAsync();
            }

            var response = await responseTask;
            var text = await response.TextAsync();
            if (!string.IsNullOrWhiteSpace(text))
            {
                return ParseJsonElement(text);
            }
        }
        catch (TimeoutException)
        {
            // Fall through to API request and page scrape.
        }

        try
        {
            return await PostCommonCaptchaFormAsync(page, payload, cancellationToken);
        }
        catch (InvalidOperationException)
        {
            return default;
        }
    }

    private static JsonElement ParseJsonElement(string text)
    {
        try
        {
            using var document = JsonDocument.Parse(text);
            return document.RootElement.Clone();
        }
        catch (JsonException)
        {
            var preview = text.Length > 200 ? text[..200] : text;
            throw new InvalidOperationException($"Unexpected response from Indian Railways: {preview}");
        }
    }

    private async Task<JsonElement> PostCommonCaptchaFormAsync(
        IPage page,
        Dictionary<string, string> payload,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        payload["inputCaptcha"] = await page.InputValueAsync("#inputCaptcha");

        var storageState = await page.Context.StorageStateAsync();
        await using var request = await _playwright!.APIRequest.NewContextAsync(new APIRequestNewContextOptions
        {
            StorageState = storageState,
            ExtraHTTPHeaders = new Dictionary<string, string>
            {
                ["Referer"] = page.Url,
                ["X-Requested-With"] = "XMLHttpRequest"
            }
        });

        var form = request.CreateFormData();
        foreach (var entry in payload)
        {
            form.Set(entry.Key, entry.Value);
        }

        var response = await request.PostAsync(
            "https://www.indianrail.gov.in/enquiry/CommonCaptcha",
            new APIRequestContextOptions { Form = form });

        var text = await response.TextAsync();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException(
                $"Indian Railways returned an empty availability response (HTTP {(int)response.Status}). Please retry.");
        }

        return ParseJsonElement(text);
    }

    private static async Task<JsonElement> ReadJsonResponseAsync(IResponse response)
    {
        var text = await response.TextAsync();
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new InvalidOperationException(
                $"Indian Railways returned an empty response (HTTP {response.Status}). Please retry.");
        }

        return ParseJsonElement(text);
    }

    private static IReadOnlyList<TrainResult> ParseTrainList(JsonElement json, TrainSearchSettings settings)
    {
        if (!json.TryGetProperty("trainBtwnStnsList", out var trains) || trains.ValueKind != JsonValueKind.Array)
        {
            return [];
        }

        var fromText = $"{settings.FromStationName} - {settings.FromStationCode}";
        var toText = $"{settings.ToStationName} - {settings.ToStationCode}";
        var travelDate = settings.TravelDate.ToString("dd-MM-yyyy");
        var results = new List<TrainResult>();

        foreach (var train in trains.EnumerateArray())
        {
            var trainNumber = GetString(train, "trainNumber", "trainNo");
            if (string.IsNullOrWhiteSpace(trainNumber))
            {
                continue;
            }

            var classes = ParseClasses(train);
            var trainType = GetString(train, "traintype", "trainType", "type");
            var classLinks = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var travelClass in classes)
            {
                classLinks[travelClass] =
                    $"{trainNumber}^{travelClass}^{toText}^{fromText}^{travelDate}^{trainType}";
            }

            results.Add(new TrainResult
            {
                TrainNumber = trainNumber,
                TrainName = GetString(train, "trainName"),
                FromStation = GetString(train, "fromStnCode", "from", "origin"),
                Departure = GetString(train, "departureTime", "depTime"),
                ToStation = GetString(train, "toStnCode", "to", "destination"),
                Arrival = GetString(train, "arrivalTime", "arrTime"),
                TravelTime = GetString(train, "duration", "travelTime"),
                Sunday = DayFlag(train, "runningSun", "sun"),
                Monday = DayFlag(train, "runningMon", "mon"),
                Tuesday = DayFlag(train, "runningTue", "tue"),
                Wednesday = DayFlag(train, "runningWed", "wed"),
                Thursday = DayFlag(train, "runningThu", "thu"),
                Friday = DayFlag(train, "runningFri", "fri"),
                Saturday = DayFlag(train, "runningSat", "sat"),
                AvailableClasses = string.Join(", ", classes),
                ClassLinkKeys = classLinks
            });
        }

        return results;
    }

    private async Task<string?> ResolveCaptchaAnswerAsync(byte[] captchaBytes, bool allowAuto)
    {
        if (allowAuto)
        {
            var autoAnswer = CaptchaAutoSolver.TrySolve(captchaBytes);
            if (!string.IsNullOrWhiteSpace(autoAnswer))
            {
                return autoAnswer;
            }
        }

        if (CaptchaProvider is null)
        {
            return null;
        }

        return await CaptchaProvider(DialogOwner ?? GetDialogOwner(), captchaBytes);
    }

    private static ClassAvailabilityResult ParseClassAvailability(
        JsonElement json,
        string trainNumber,
        string travelClass,
        string travelDate)
    {
        if (json.ValueKind != JsonValueKind.Object)
        {
            return new ClassAvailabilityResult
            {
                TrainNumber = trainNumber,
                TravelClass = travelClass,
                TravelDate = travelDate
            };
        }

        json = UnwrapFareResponse(json);
        var days = new List<AvailabilityDay>();
        if (json.TryGetProperty("avlDayList", out var dayList) && dayList.ValueKind == JsonValueKind.Array)
        {
            foreach (var day in dayList.EnumerateArray())
            {
                days.Add(new AvailabilityDay
                {
                    Date = GetString(day, "availDate", "date", "day"),
                    Status = GetString(day, "availStatus", "status", "availability")
                });
            }
        }
        else if (json.TryGetProperty("avlList", out var avlList) && avlList.ValueKind == JsonValueKind.Array)
        {
            foreach (var day in avlList.EnumerateArray())
            {
                days.Add(new AvailabilityDay
                {
                    Date = GetString(day, "availDate", "date", "day"),
                    Status = GetString(day, "availStatus", "status", "availability")
                });
            }
        }
        else if (json.TryGetProperty("availability", out var availability))
        {
            days.Add(new AvailabilityDay { Date = travelDate, Status = availability.ToString() });
        }
        else if (json.TryGetProperty("availabilityList", out var availabilityList)
                 && availabilityList.ValueKind == JsonValueKind.Array)
        {
            foreach (var day in availabilityList.EnumerateArray())
            {
                days.Add(new AvailabilityDay
                {
                    Date = GetString(day, "availDate", "date", "day"),
                    Status = GetString(day, "availStatus", "status", "availability")
                });
            }
        }

        return new ClassAvailabilityResult
        {
            TrainNumber = trainNumber,
            TravelClass = travelClass,
            TravelDate = travelDate,
            BaseFare = GetString(json, "baseFare", "baseClassFare"),
            ReservationCharges = GetString(json, "reservationCharges", "reservationCharge"),
            SuperfastCharges = GetString(json, "superfastCharges", "superfastCharge"),
            OtherCharges = GetString(json, "otherCharges", "otherCharge"),
            TatkalCharges = GetString(json, "tatkalCharges", "tatkalCharge"),
            GoodsServiceTax = GetString(json, "serviceTax", "goodsServiceTax"),
            CateringCharge = GetString(json, "cateringCharge"),
            DynamicFare = GetString(json, "dynamicFare"),
            TotalFare = GetString(json, "totalFare", "total"),
            AvailabilityDays = days
        };
    }

    private static List<string> ParseClasses(JsonElement train)
    {
        foreach (var propertyName in new[] { "avlClasses", "availableClasses" })
        {
            if (!train.TryGetProperty(propertyName, out var classesElement))
            {
                continue;
            }

            if (classesElement.ValueKind == JsonValueKind.Array)
            {
                return classesElement.EnumerateArray()
                    .Select(item => item.ValueKind == JsonValueKind.String ? item.GetString() : item.ToString())
                    .Select(value => value?.Trim() ?? string.Empty)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }

            if (classesElement.ValueKind == JsonValueKind.String)
            {
                var text = classesElement.GetString() ?? string.Empty;
                if (text.StartsWith('[') && text.EndsWith(']'))
                {
                    try
                    {
                        var parsed = JsonSerializer.Deserialize<string[]>(text);
                        if (parsed is not null)
                        {
                            return parsed
                                .Select(value => value.Trim())
                                .Where(value => !string.IsNullOrWhiteSpace(value))
                                .Distinct(StringComparer.OrdinalIgnoreCase)
                                .ToList();
                        }
                    }
                    catch (JsonException)
                    {
                        // Fall back to comma split below.
                    }
                }

                return text
                    .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                    .Where(value => !string.IsNullOrWhiteSpace(value))
                    .Distinct(StringComparer.OrdinalIgnoreCase)
                    .ToList();
            }
        }

        return [];
    }

    private static JsonElement UnwrapFareResponse(JsonElement json)
    {
        if (json.ValueKind != JsonValueKind.Object)
        {
            return json;
        }

        foreach (var propertyName in new[] { "fareAvlResponse", "fareResponse", "data", "result" })
        {
            if (json.TryGetProperty(propertyName, out var nested) && nested.ValueKind == JsonValueKind.Object)
            {
                return nested;
            }
        }

        return json;
    }

    private sealed class PageClassLink
    {
        public string TrainNo { get; set; } = string.Empty;
        public string ClassCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
    }

    private sealed class PageFareScrape
    {
        public string BaseFare { get; set; } = string.Empty;
        public string ReservationCharges { get; set; } = string.Empty;
        public string SuperfastCharges { get; set; } = string.Empty;
        public string OtherCharges { get; set; } = string.Empty;
        public string TatkalCharges { get; set; } = string.Empty;
        public string GoodsServiceTax { get; set; } = string.Empty;
        public string CateringCharge { get; set; } = string.Empty;
        public string DynamicFare { get; set; } = string.Empty;
        public string TotalFare { get; set; } = string.Empty;
        public List<PageAvailabilityDay> Days { get; set; } = [];
    }

    private sealed class PageAvailabilityDay
    {
        public string Date { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
    }

    private static string GetString(JsonElement element, params string[] names)
    {
        foreach (var name in names)
        {
            if (!element.TryGetProperty(name, out var value))
            {
                continue;
            }

            return value.ValueKind switch
            {
                JsonValueKind.String => value.GetString() ?? string.Empty,
                JsonValueKind.Number => value.GetRawText(),
                JsonValueKind.True => "Y",
                JsonValueKind.False => "N",
                _ => value.ToString()
            };
        }

        return string.Empty;
    }

    private static string DayFlag(JsonElement train, params string[] names)
    {
        var value = GetString(train, names);
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return value.Equals("Y", StringComparison.OrdinalIgnoreCase)
            || value.Equals("true", StringComparison.OrdinalIgnoreCase)
            || value == "1"
            ? "Y"
            : "N";
    }

    private static IWin32Window? GetDialogOwner() =>
        Form.ActiveForm;

    private async Task EnsurePageAsync(IProgress<string>? progress = null)
    {
        if (_page is not null)
        {
            return;
        }

        _playwright ??= await Playwright.CreateAsync();
        try
        {
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }
        catch (PlaywrightException ex) when (IsMissingBrowserError(ex))
        {
            progress?.Report("Installing browser for first run (one-time setup)...");
            var exitCode = Microsoft.Playwright.Program.Main(["install", "chromium"]);
            if (exitCode != 0)
            {
                throw new InvalidOperationException(
                    "Playwright browser install failed. Run playwright.ps1 install chromium.");
            }

            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
        }

        _context = await _browser.NewContextAsync(new BrowserNewContextOptions
        {
            UserAgent =
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36",
            ViewportSize = new ViewportSize { Width = 1280, Height = 900 },
            Locale = "en-IN"
        });
        _page = await _context.NewPageAsync();
    }

    private static bool IsMissingBrowserError(PlaywrightException ex) =>
        ex.Message.Contains("Executable doesn't exist", StringComparison.OrdinalIgnoreCase)
        || ex.Message.Contains("Please run the following command", StringComparison.OrdinalIgnoreCase);

    public async ValueTask DisposeAsync()
    {
        if (_context is not null)
        {
            await _context.CloseAsync();
            _context = null;
        }

        if (_browser is not null)
        {
            await _browser.CloseAsync();
            _browser = null;
        }

        _playwright?.Dispose();
        _playwright = null;
        _page = null;
    }
}
