namespace train_automation;

public static class HardcodedStations
{
    public static IReadOnlyList<StationInfo> All { get; } =
    [
        new() { Code = "NDLS", Name = "NEW DELHI" },
        new() { Code = "DLI", Name = "DELHI" },
        new() { Code = "NZM", Name = "HAZRAT NIZAMUDDIN" },
        new() { Code = "ANVT", Name = "ANAND VIHAR TRM" },
        new() { Code = "CSTM", Name = "MUMBAI CST" },
        new() { Code = "BCT", Name = "MUMBAI CENTRAL" },
        new() { Code = "LTT", Name = "LOKMANYA TILAK T" },
        new() { Code = "BDTS", Name = "BANDRA TERMINUS" },
        new() { Code = "PNVL", Name = "PANVEL" },
        new() { Code = "HWH", Name = "HOWRAH JN" },
        new() { Code = "KOAA", Name = "KOLKATA" },
        new() { Code = "SDAH", Name = "SEALDAH" },
        new() { Code = "MAS", Name = "CHENNAI CENTRAL" },
        new() { Code = "MS", Name = "CHENNAI EGMORE" },
        new() { Code = "SBC", Name = "KSR BENGALURU" },
        new() { Code = "YPR", Name = "YESVANTPUR JN" },
        new() { Code = "HYB", Name = "HYDERABAD DECAN" },
        new() { Code = "SC", Name = "SECUNDERABAD JN" },
        new() { Code = "PNBE", Name = "PATNA JN" },
        new() { Code = "DNR", Name = "DANAPUR" },
        new() { Code = "LKO", Name = "LUCKNOW NR" },
        new() { Code = "LJN", Name = "LUCKNOW NE" },
        new() { Code = "JP", Name = "JAIPUR" },
        new() { Code = "ADI", Name = "AHMEDABAD JN" },
        new() { Code = "PUNE", Name = "PUNE JN" },
        new() { Code = "CNB", Name = "KANPUR CENTRAL" },
        new() { Code = "BPL", Name = "BHOPAL JN" },
        new() { Code = "NGP", Name = "NAGPUR" },
        new() { Code = "VSKP", Name = "VISAKHAPATNAM" },
        new() { Code = "TVC", Name = "TRIVANDRUM CNTL" },
        new() { Code = "ERS", Name = "ERNAKULAM JN" },
        new() { Code = "BSB", Name = "VARANASI JN" },
        new() { Code = "GKP", Name = "GORAKHPUR JN" },
        new() { Code = "RNC", Name = "RANCHI" },
        new() { Code = "JAT", Name = "JAMMU TAWI" },
        new() { Code = "ASR", Name = "AMRITSAR JN" },
        new() { Code = "CDG", Name = "CHANDIGARH" },
        new() { Code = "DBG", Name = "DARBHANGA JN" },
        new() { Code = "MFP", Name = "MUZAFFARPUR JN" },
        new() { Code = "GAYA", Name = "GAYA JN" }
    ];
}
