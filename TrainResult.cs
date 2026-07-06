namespace train_automation;

public sealed class TrainResult
{
    public string TrainNumber { get; init; } = string.Empty;
    public string TrainName { get; init; } = string.Empty;
    public string FromStation { get; init; } = string.Empty;
    public string Departure { get; init; } = string.Empty;
    public string ToStation { get; init; } = string.Empty;
    public string Arrival { get; init; } = string.Empty;
    public string Duration { get; init; } = string.Empty;
    public string RunsOn { get; init; } = string.Empty;
    public string Availability { get; init; } = string.Empty;
}
