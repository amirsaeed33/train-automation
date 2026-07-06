namespace train_automation;

public sealed class TrainSearchSettings
{
    public string FromStation { get; init; } = "New Delhi";
    public string ToStation { get; init; } = "Mumbai";
    public DateTime TravelDate { get; init; } = DateTime.Today.AddDays(1);
    public string Quota { get; init; } = "GN";
    public string SiteUrl { get; init; } = "https://m.etrain.info/trains";
}
