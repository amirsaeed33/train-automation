namespace train_automation;

public static class EtrainScraperDefaults
{
    public const string TrainsUrl = "https://etrain.info/trains";

    public const string UserAgent =
        "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0.0.0 Safari/537.36";

    public const string Locale = "en-IN";

    public const int ViewportWidth = 1280;
    public const int ViewportHeight = 900;

    public const float NavigationTimeoutMs = 10_000;
    public const float StationDataTimeoutMs = 10_000;
    public const float ResultsTimeoutMs = 10_000;
    public const float AvailabilityResponseTimeoutMs = 5_000;
    public const float PostSearchSettleDelayMs = 500;

    public const bool Headless = true;
}
