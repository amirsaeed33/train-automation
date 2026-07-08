namespace train_automation;

public sealed class PassengerInfo
{
    public string Name { get; init; } = string.Empty;
    public int Age { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string Phone { get; init; } = string.Empty;
}

public sealed class BookingPreferences
{
    public bool AutoUpgradation { get; init; }
    public bool BookOnlyIfConfirmBerths { get; init; }
    public string ReservationChoice { get; init; } = string.Empty;
    public string PreferredCoachNo { get; init; } = string.Empty;
}

public sealed class TrainBookingRecord
{
    public DateTime SavedAt { get; init; } = DateTime.Now;
    public string TrainNumber { get; init; } = string.Empty;
    public string TrainName { get; init; } = string.Empty;
    public string FromStation { get; init; } = string.Empty;
    public string ToStation { get; init; } = string.Empty;
    public DateTime TravelDate { get; init; }
    public List<PassengerInfo> Passengers { get; init; } = [];
    public BookingPreferences Preferences { get; init; } = new();
    public string TravelInsurance { get; init; } = string.Empty;
    public string PaymentMode { get; init; } = string.Empty;
}
