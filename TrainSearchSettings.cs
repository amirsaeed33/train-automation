namespace train_automation;

public sealed class TrainSearchSettings
{
    public string FromStationCode { get; init; } = string.Empty;
    public string FromStationName { get; init; } = string.Empty;
    public string ToStationCode { get; init; } = string.Empty;
    public string ToStationName { get; init; } = string.Empty;
    public DateTime TravelDate { get; init; } = DateTime.Today.AddDays(1);

    /// <summary>GN=General, TQ=Tatkal, LD=Ladies, SS=Senior, PT=Premium Tatkal.</summary>
    public string Quota { get; init; } = "GN";

    /// <summary>Coach class code: SL, 3A, 2A, 1A, CC, EC, 2S, 3E, etc.</summary>
    public string PreferredClass { get; init; } = "SL";

    public string SiteUrl { get; init; } = "https://etrain.info/trains";
}
