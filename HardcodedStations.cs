namespace train_automation;

public static class HardcodedStations
{
    public static IReadOnlyList<StationInfo> All { get; } =
    [
        new() { Code = "NDLS", Name = "NEW DELHI" },
        new() { Code = "DLI", Name = "DELHI" },
        new() { Code = "NZM", Name = "H NIZAMUDDIN" },
        new() { Code = "DEC", Name = "DELHI CANTT" },
        new() { Code = "CSTM", Name = "Mumbai CST" },
        new() { Code = "CSMT", Name = "CSMT MUMBAI" },
        new() { Code = "BCT", Name = "Mumbai Central" },
        new() { Code = "MMCT", Name = "MUMBAI CENTRAL" },
        new() { Code = "LTT", Name = "Lokmanya Tilak Terminus" },
        new() { Code = "BDTS", Name = "Bandra Terminus" },
        new() { Code = "KYN", Name = "KALYAN JN" },
        new() { Code = "BSR", Name = "VASAI ROAD" },
        new() { Code = "BVI", Name = "BORIVALI" },
        new() { Code = "HWH", Name = "Howrah" },
        new() { Code = "KOAA", Name = "Kolkata" },
        new() { Code = "MAS", Name = "Chennai Central" },
        new() { Code = "SBC", Name = "Bangalore City" },
        new() { Code = "HYB", Name = "Hyderabad" },
        new() { Code = "PNBE", Name = "PATNA JN" },
        new() { Code = "LKO", Name = "Lucknow" },
        new() { Code = "JP", Name = "Jaipur" },
        new() { Code = "ADI", Name = "Ahmedabad" },
        new() { Code = "PUNE", Name = "Pune" },
        new() { Code = "CNB", Name = "Kanpur Central" },
        new() { Code = "BPL", Name = "Bhopal" },
        new() { Code = "NGP", Name = "Nagpur" },
        new() { Code = "VSKP", Name = "Visakhapatnam" },
        new() { Code = "TVC", Name = "Thiruvananthapuram" },
        new() { Code = "ERS", Name = "Ernakulam Jn" },
        new() { Code = "CDG", Name = "Chandigarh" },
        new() { Code = "ASR", Name = "Amritsar" },
        new() { Code = "HW", Name = "Haridwar" }
    ];

    public static StationInfo? Find(string? codeOrName)
    {
        if (string.IsNullOrWhiteSpace(codeOrName))
        {
            return null;
        }

        var text = codeOrName.Trim();
        return All.FirstOrDefault(s => s.Code.Equals(text, StringComparison.OrdinalIgnoreCase))
               ?? All.FirstOrDefault(s => s.Name.Equals(text, StringComparison.OrdinalIgnoreCase))
               ?? All.FirstOrDefault(s => s.Name.Contains(text, StringComparison.OrdinalIgnoreCase))
               ?? All.FirstOrDefault(s => text.Contains(s.Code, StringComparison.OrdinalIgnoreCase));
    }
}
