namespace train_automation;

public sealed class ClassAvailabilityResult
{
    public string TrainNumber { get; init; } = string.Empty;
    public string TravelClass { get; init; } = string.Empty;
    public string TravelDate { get; init; } = string.Empty;
    public string BaseFare { get; init; } = string.Empty;
    public string ReservationCharges { get; init; } = string.Empty;
    public string SuperfastCharges { get; init; } = string.Empty;
    public string OtherCharges { get; init; } = string.Empty;
    public string TatkalCharges { get; init; } = string.Empty;
    public string GoodsServiceTax { get; init; } = string.Empty;
    public string CateringCharge { get; init; } = string.Empty;
    public string DynamicFare { get; init; } = string.Empty;
    public string TotalFare { get; init; } = string.Empty;
    public IReadOnlyList<AvailabilityDay> AvailabilityDays { get; init; } = [];
}

public sealed class AvailabilityDay
{
    public string Date { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}
