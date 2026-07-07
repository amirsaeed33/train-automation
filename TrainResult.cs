namespace train_automation;

public sealed class TrainResult
{
    public string TrainNumber { get; init; } = string.Empty;
    public string TrainName { get; init; } = string.Empty;
    public string FromStation { get; init; } = string.Empty;
    public string Departure { get; init; } = string.Empty;
    public string ToStation { get; init; } = string.Empty;
    public string Arrival { get; init; } = string.Empty;
    public string TravelTime { get; init; } = string.Empty;
    public string Sunday { get; init; } = string.Empty;
    public string Monday { get; init; } = string.Empty;
    public string Tuesday { get; init; } = string.Empty;
    public string Wednesday { get; init; } = string.Empty;
    public string Thursday { get; init; } = string.Empty;
    public string Friday { get; init; } = string.Empty;
    public string Saturday { get; init; } = string.Empty;
    public string AvailableClasses { get; init; } = string.Empty;
}
