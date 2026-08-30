namespace train_automation;

public sealed class PassengerInfo
{
    public string Name { get; init; } = string.Empty;
    public int Age { get; init; }
    public string Gender { get; init; } = string.Empty;
    public string Berth { get; init; } = string.Empty;
    public string Food { get; init; } = string.Empty;
    public string Nationality { get; init; } = string.Empty;
    public string Passport { get; init; } = string.Empty;
    public bool IsChild { get; init; }
    public bool IsSenior { get; init; }
    public bool BedRoll { get; init; }
}

public sealed class BookingPreferences
{
    public bool AutoUpgradation { get; init; }
    public bool BookOnlyIfConfirmBerths { get; init; }
    public string TicketSlot { get; init; } = string.Empty;
    public string Gateway { get; init; } = string.Empty;
    public string PriorBank { get; init; } = string.Empty;
    public string BackupBank { get; init; } = string.Empty;
    public string TicketName { get; init; } = string.Empty;
}

public sealed class TrainBookingRecord
{
    public DateTime SavedAt { get; init; } = DateTime.Now;
    public string TrainNumber { get; init; } = string.Empty;
    public string TrainName { get; init; } = string.Empty;
    public string TrainType { get; init; } = string.Empty;
    public string FromStation { get; init; } = string.Empty;
    public string ToStation { get; init; } = string.Empty;
    public string BoardingPoint { get; init; } = string.Empty;
    public string TravelClass { get; init; } = string.Empty;
    public string Quota { get; init; } = string.Empty;
    public string SelectedDay { get; init; } = string.Empty;
    public DateTime TravelDate { get; init; }
    public string Mobile { get; init; } = string.Empty;
    public string Fare { get; init; } = string.Empty;
    public List<PassengerInfo> Passengers { get; init; } = [];
    public BookingPreferences Preferences { get; init; } = new();
}

public sealed class BankDetails
{
    public string Gateway { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string NameToSave { get; set; } = string.Empty;
}

public sealed class CardDetails
{
    public string CardCategory { get; set; } = "Debit";
    public string Gateway { get; set; } = string.Empty;
    public string BankName { get; set; } = string.Empty;
    public string CardType { get; set; } = string.Empty;
    public string CardNumber { get; set; } = string.Empty;
    public string ExpiryMonth { get; set; } = string.Empty;
    public string ExpiryYear { get; set; } = string.Empty;
    public string NameOnCard { get; set; } = string.Empty;
    public string Pin { get; set; } = string.Empty;
    public string Cvv { get; set; } = string.Empty;
    public string ThreeDPassword { get; set; } = string.Empty;
    public string NameToSave { get; set; } = string.Empty;
}
