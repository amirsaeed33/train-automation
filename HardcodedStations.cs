namespace train_automation;

public static class HardcodedStations
{
    public static IReadOnlyList<StationInfo> All { get; } =
    [
        new() { Code = "NDLS", Name = "NEW DELHI" },
        new() { Code = "DLI", Name = "DELHI" },
        new() { Code = "CSTM", Name = "Mumbai CST" },
        new() { Code = "BCT", Name = "Mumbai Central" },
        new() { Code = "LTT", Name = "Lokmanya Tilak Terminus" },
        new() { Code = "BDTS", Name = "Bandra Terminus" },
        new() { Code = "HWH", Name = "Howrah" },
        new() { Code = "KOAA", Name = "Kolkata" },
        new() { Code = "MAS", Name = "Chennai Central" },
        new() { Code = "SBC", Name = "Bangalore City" },
        new() { Code = "HYB", Name = "Hyderabad" },
        new() { Code = "PNBE", Name = "Patna" },
        new() { Code = "LKO", Name = "Lucknow" },
        new() { Code = "JP", Name = "Jaipur" },
        new() { Code = "ADI", Name = "Ahmedabad" },
        new() { Code = "PUNE", Name = "Pune" },
        new() { Code = "CNB", Name = "Kanpur Central" },
        new() { Code = "BPL", Name = "Bhopal" },
        new() { Code = "NGP", Name = "Nagpur" },
        new() { Code = "VSKP", Name = "Visakhapatnam" },
        new() { Code = "TVC", Name = "Thiruvananthapuram" }
    ];
}
