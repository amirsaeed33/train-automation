using System.Net.Http;
using System.Text.RegularExpressions;

namespace train_automation;

public sealed class EtrainStationService
{
    private const string SiteUrl = "https://m.etrain.info/trains";
    private const string DefaultStationVersion = "20260301";

    private static readonly HttpClient HttpClient = new()
    {
        Timeout = TimeSpan.FromSeconds(60)
    };

    public async Task<IReadOnlyList<StationInfo>> GetStationsAsync(
        IProgress<string>? progress = null,
        CancellationToken cancellationToken = default)
    {
        progress?.Report("Loading station list...");
        var version = await GetStationVersionAsync(cancellationToken);
        var scriptUrl = $"https://etrain.info/jscss/station_code.js?ver={version}";
        var script = await HttpClient.GetStringAsync(scriptUrl, cancellationToken);

        var match = Regex.Match(script, @"slist=""([^""]+)""");
        if (!match.Success)
        {
            throw new InvalidOperationException("Could not read station list from etrain.info.");
        }

        var stations = match.Groups[1].Value
            .Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(ParseStation)
            .Where(station => station is not null)
            .Cast<StationInfo>()
            .OrderBy(station => station.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();

        progress?.Report($"Loaded {stations.Count} stations.");
        return stations;
    }

    private static async Task<string> GetStationVersionAsync(CancellationToken cancellationToken)
    {
        try
        {
            var html = await HttpClient.GetStringAsync(SiteUrl, cancellationToken);
            var match = Regex.Match(html, @"STNCNVER\s*=\s*'(\d+)'");
            if (match.Success)
            {
                return match.Groups[1].Value;
            }
        }
        catch
        {
            // Fall back to the known version if the page cannot be reached.
        }

        return DefaultStationVersion;
    }

    private static StationInfo? ParseStation(string entry)
    {
        var separatorIndex = entry.IndexOf(',');
        if (separatorIndex <= 0 || separatorIndex >= entry.Length - 1)
        {
            return null;
        }

        return new StationInfo
        {
            Code = entry[..separatorIndex].Trim(),
            Name = entry[(separatorIndex + 1)..].Trim()
        };
    }
}
