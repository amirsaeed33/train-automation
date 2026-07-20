namespace train_automation;

public sealed class Passenger
{
    public string Name { get; set; } = string.Empty;
    public string Age { get; set; } = string.Empty;
    public string Gender { get; set; } = "Male"; // Male, Female, Transgender
    public string BerthPreference { get; set; } = "No Preference"; // Lower, Middle, Upper, Side Lower, Side Upper, Window
}
