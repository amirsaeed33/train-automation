namespace train_automation;

public sealed class StationInfo
{
    public required string Code { get; init; }
    public required string Name { get; init; }

    public string DisplayText => $"{Code} - {Name}";

    public override string ToString() => DisplayText;
}
