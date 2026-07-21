namespace train_automation;

public sealed class TrainSearchSettings
{
    public string FromStationCode { get; init; } = string.Empty;
    public string FromStationName { get; init; } = string.Empty;
    public string ToStationCode { get; init; } = string.Empty;
    public string ToStationName { get; init; } = string.Empty;
    public DateTime TravelDate { get; init; } = DateTime.Today.AddDays(1);
    public string Quota { get; init; } = "GN";
    /// <summary>Preferred coach class for IRCTC booking, e.g. SL, 3A, 2A.</summary>
    public string PreferredClass { get; init; } = "SL";
    public string SiteUrl { get; init; } = "https://www.indianrail.gov.in/enquiry/TBIS/TrainBetweenImportantStations.html?locale=en";
}
