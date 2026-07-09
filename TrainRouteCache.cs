using System.Text.Json;
using System.Text.Json.Serialization;

namespace train_automation;

public static class TrainRouteCache
{
    private static readonly TimeSpan CacheLifetime = TimeSpan.FromDays(2);
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly object FileLock = new();
    private static readonly string CacheFilePath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "train-automation",
        "route-cache.json");

    public static bool TryGet(TrainSearchSettings settings, out IReadOnlyList<TrainResult> trains)
    {
        trains = Array.Empty<TrainResult>();

        lock (FileLock)
        {
            var store = LoadStore();
            PruneExpired(store);

            var key = BuildCacheKey(settings);
            var entry = store.Routes.FirstOrDefault(route => route.CacheKey.Equals(key, StringComparison.OrdinalIgnoreCase));
            if (entry is null || IsExpired(entry))
            {
                if (entry is not null)
                {
                    store.Routes.Remove(entry);
                    SaveStore(store);
                }

                return false;
            }

            trains = entry.Trains.Select(ToTrainResult).ToList();
            return trains.Count > 0;
        }
    }

    public static void Save(TrainSearchSettings settings, IReadOnlyList<TrainResult> trains)
    {
        if (trains.Count == 0)
        {
            return;
        }

        lock (FileLock)
        {
            var store = LoadStore();
            PruneExpired(store);

            var key = BuildCacheKey(settings);
            store.Routes.RemoveAll(route => route.CacheKey.Equals(key, StringComparison.OrdinalIgnoreCase));
            store.Routes.Add(new TrainRouteCacheEntry
            {
                CacheKey = key,
                FromStationCode = settings.FromStationCode,
                ToStationCode = settings.ToStationCode,
                TravelDate = settings.TravelDate.ToString("yyyy-MM-dd"),
                Quota = settings.Quota,
                CachedAtUtc = DateTime.UtcNow,
                Trains = trains.Select(ToCacheDto).ToList()
            });

            SaveStore(store);
        }
    }

    private static string BuildCacheKey(TrainSearchSettings settings) =>
        $"{settings.FromStationCode}|{settings.ToStationCode}|{settings.TravelDate:yyyy-MM-dd}|{settings.Quota}"
            .ToUpperInvariant();

    private static bool IsExpired(TrainRouteCacheEntry entry) =>
        DateTime.UtcNow - entry.CachedAtUtc > CacheLifetime;

    private static void PruneExpired(TrainRouteCacheStore store)
    {
        store.Routes.RemoveAll(IsExpired);
    }

    private static TrainRouteCacheStore LoadStore()
    {
        if (!File.Exists(CacheFilePath))
        {
            return new TrainRouteCacheStore();
        }

        try
        {
            var json = File.ReadAllText(CacheFilePath);
            return JsonSerializer.Deserialize<TrainRouteCacheStore>(json, JsonOptions) ?? new TrainRouteCacheStore();
        }
        catch (JsonException)
        {
            return new TrainRouteCacheStore();
        }
    }

    private static void SaveStore(TrainRouteCacheStore store)
    {
        var directory = Path.GetDirectoryName(CacheFilePath);
        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(store, JsonOptions);
        File.WriteAllText(CacheFilePath, json);
    }

    private static TrainResult ToTrainResult(TrainResultCacheDto dto) => new()
    {
        TrainNumber = dto.TrainNumber,
        TrainName = dto.TrainName,
        FromStation = dto.FromStation,
        Departure = dto.Departure,
        ToStation = dto.ToStation,
        Arrival = dto.Arrival,
        TravelTime = dto.TravelTime,
        Sunday = dto.Sunday,
        Monday = dto.Monday,
        Tuesday = dto.Tuesday,
        Wednesday = dto.Wednesday,
        Thursday = dto.Thursday,
        Friday = dto.Friday,
        Saturday = dto.Saturday,
        AvailableClasses = dto.AvailableClasses,
        ClassLinkKeys = dto.ClassLinkKeys ?? new Dictionary<string, string>()
    };

    private static TrainResultCacheDto ToCacheDto(TrainResult train) => new()
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
        ClassLinkKeys = train.ClassLinkKeys.ToDictionary(pair => pair.Key, pair => pair.Value)
    };

    private sealed class TrainRouteCacheStore
    {
        public List<TrainRouteCacheEntry> Routes { get; set; } = [];
    }

    private sealed class TrainRouteCacheEntry
    {
        public string CacheKey { get; set; } = string.Empty;
        public string FromStationCode { get; set; } = string.Empty;
        public string ToStationCode { get; set; } = string.Empty;
        public string TravelDate { get; set; } = string.Empty;
        public string Quota { get; set; } = "GN";
        public DateTime CachedAtUtc { get; set; }
        public List<TrainResultCacheDto> Trains { get; set; } = [];
    }

    private sealed class TrainResultCacheDto
    {
        public string TrainNumber { get; set; } = string.Empty;
        public string TrainName { get; set; } = string.Empty;
        public string FromStation { get; set; } = string.Empty;
        public string Departure { get; set; } = string.Empty;
        public string ToStation { get; set; } = string.Empty;
        public string Arrival { get; set; } = string.Empty;
        public string TravelTime { get; set; } = string.Empty;
        public string Sunday { get; set; } = string.Empty;
        public string Monday { get; set; } = string.Empty;
        public string Tuesday { get; set; } = string.Empty;
        public string Wednesday { get; set; } = string.Empty;
        public string Thursday { get; set; } = string.Empty;
        public string Friday { get; set; } = string.Empty;
        public string Saturday { get; set; } = string.Empty;
        public string AvailableClasses { get; set; } = string.Empty;
        public Dictionary<string, string>? ClassLinkKeys { get; set; }
    }
}
