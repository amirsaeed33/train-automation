namespace train_automation;

public sealed class StationInfo
{
    public required string Code { get; init; }
    public required string Name { get; init; }

    public string DisplayText => $"{Name} ({Code})";

    public override string ToString() => DisplayText;
}
