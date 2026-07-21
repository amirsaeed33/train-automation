namespace train_automation;

/// <summary>
/// Central DOM selectors for IRCTC (aligned with common extension selector maps).
/// </summary>
public static class IrctcSelectors
{
    public const string LoginUserId = "input[formcontrolname='userid']:visible, input[placeholder='User Name']:visible";
    public const string LoginPassword = "input[formcontrolname='password']:visible, input[placeholder='Password']:visible";
    public const string LoginSignIn = "button:has-text('SIGN IN'):visible, button:has-text('Sign In'):visible";
    public const string CaptchaImage = "app-captcha .captcha-img, .captcha-img";
    public const string CaptchaInput = "app-captcha #captcha, input[formcontrolname='captcha']:visible, input[placeholder='Enter Captcha']:visible";

    public const string OriginInput = "#origin input, .ui-autocomplete-input";
    public const string DestinationInput = "#destination input";
    public const string StationListItem = ".ui-autocomplete-items li, ul.ui-autocomplete li";
    public const string JourneyQuota = "#journeyQuota > div, #journeyQuota";
    public const string JourneyQuotaItems = "#journeyQuota p-dropdownitem span, #journeyQuota li[role='option']";
    public const string JourneyDate = "#jDate input, p-calendar input";
    public const string SearchButton = "app-jp-input button[type='submit'].search_btn, button.search_btn:has-text('Search')";

    public const string TrainList = "app-train-list";
    public const string TrainComponent = "app-train-avl-enq";
    public const string TrainHeading = "app-train-avl-enq .train-heading";
    public const string AvailableClass = ".pre-avl";
    public const string BookNowButton = "button.btnDefault.train_Search:has-text('Book'), button:has-text('Book Now')";
    public const string BookDisabledClass = "disable-book";
    public const string SelectedClassTab = "p-tabmenu li[role='tab'][aria-selected='true'] a";

    public const string ConfirmDialogAccept = ".ui-confirmdialog-acceptbutton, button:has-text('Yes')";

    public const string PassengerName = "input[placeholder='Full Name as per Govt. ID'], p-autocomplete input, input[placeholder='Passenger Name']";
    public const string PassengerAge = "input[formcontrolname='passengerAge'], input[placeholder='Age']";
    public const string PassengerGender = "select[formcontrolname='passengerGender']";
    public const string PassengerBerth = "select[formcontrolname='passengerBerthChoice']";
    public const string PassengerFood = "select[formcontrolname='passengerFoodChoice']";
    public const string AddPassenger = "a:has-text('Add Passenger'), span:has-text('+ Add Passenger'), .prenext:has-text('Add Passenger')";
    public const string MobileNumber = "input[formcontrolname='mobileNumber'], #mobileNumber, input[name='mobileNumber']";
    public const string ConfirmBerths = "input[formcontrolname='confirmberths'], input#confirmberths, label:has-text('Book only if confirm berths') input";
    public const string AutoUpgrade = "input[formcontrolname='autoUpgradation'], input#autoUpgradation";
    public const string PassengerContinue = "app-passenger-input button.btnDefault.train_Search, button:has-text('Continue'):visible";

    public const string ReviewContinue = "app-review-booking button.btnDefault.train_Search, button:has-text('Continue'):visible";

    // Passenger-page payment type radios (before Continue)
    public const string PaymentTypeRadio = "input[type='radio'][name='paymentType'], p-radiobutton[name='paymentType'] input";

    // Payment gateway page (after review) — mirrors chrome-extension PAYMENT_SELECTORS
    public const string PaymentComponent = "app-payment-options";
    public const string PaymentMethodBankType = ".bank-type.ng-star-inserted, #pay-type .bank-type, .bank-type";
    public const string PaymentProviderBankText = ".bank-text, #bank-type .bank-text, #bank-type .pay_tax_text";
    public const string PayAndBookButton = "button.btn-primary.ng-star-inserted:has-text('Pay'), button:has-text('Pay & Book')";
    public const string EwalletComponent = "app-ewallet-confirm";
    public const string EwalletConfirm = "app-ewallet-confirm button.mob-bot-btn.search_btn:has-text('CONFIRM'), app-ewallet-confirm button:has-text('CONFIRM')";

    public const string PaymentOptions = "app-payment-options, label:has-text('BHIM/UPI')";
    public const string PayButton = "button:has-text('Pay & Book'), button:has-text('Pay'), .btn-primary";
}

/// <summary>Maps UI quota codes to IRCTC dropdown labels.</summary>
public static class IrctcQuotaLabels
{
    public static string ToDisplayLabel(string code) => code.ToUpperInvariant() switch
    {
        "TQ" => "TATKAL",
        "PT" => "PREMIUM TATKAL",
        "LD" => "LADIES",
        "SS" => "SENIOR CITIZEN",
        "HP" => "PHYSICALLY HANDICAPPED",
        _ => "GENERAL"
    };

    public static readonly (string Code, string Label)[] Options =
    [
        ("GN", "General (GN)"),
        ("TQ", "Tatkal (TQ)"),
        ("PT", "Premium Tatkal (PT)"),
        ("LD", "Ladies (LD)"),
        ("SS", "Senior Citizen (SS)")
    ];
}

/// <summary>Common IRCTC coach class codes.</summary>
public static class IrctcClassOptions
{
    public static readonly (string Code, string Label)[] Options =
    [
        ("SL", "Sleeper (SL)"),
        ("3A", "AC 3 Tier (3A)"),
        ("2A", "AC 2 Tier (2A)"),
        ("1A", "AC First Class (1A)"),
        ("3E", "AC 3 Economy (3E)"),
        ("CC", "Chair Car (CC)"),
        ("EC", "Exec. Chair Car (EC)"),
        ("2S", "Second Sitting (2S)"),
        ("EA", "Anubhuti (EA)")
    ];
}

public static class IrctcPaymentOptions
{
    public static readonly string[] Options =
    [
        "BHIM/UPI",
        "IRCTC eWallet",
        "Credit & Debit cards / Net Banking / Wallets / Bharat Pe / Paytm / UPI"
    ];
}
