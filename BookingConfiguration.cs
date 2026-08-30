using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace train_automation;

public sealed class BookingConfiguration
{
    public IrctcCredentials Credentials { get; set; } = new();
    public List<IrctcCredentials> SavedAccounts { get; set; } = new();
    public List<BankDetails> SavedBanks { get; set; } = new();
    public List<CardDetails> SavedCards { get; set; } = new();
    public List<Passenger> Passengers { get; set; } = new();

    /// <summary>Preferred coach class code, e.g. SL, 3A, 2A, CC, 2S.</summary>
    public string PreferredClass { get; set; } = "SL";

    /// <summary>IRCTC quota: GN, TQ, LD, SS, PT.</summary>
    public string Quota { get; set; } = "GN";

    /// <summary>Payment type radio on passenger page, e.g. BHIM/UPI.</summary>
    public string PaymentMethod { get; set; } = "BHIM/UPI";

    /// <summary>Bank/UPI provider on payment page (.bank-text), e.g. PAYTM, PhonePe.</summary>
    public string PaymentProvider { get; set; } = "PAYTM";

    /// <summary>When true, click Pay &amp; Book once after selecting method/provider.</summary>
    public bool AutoPay { get; set; } = true;

    public string MobileNumber { get; set; } = string.Empty;
    public bool ConfirmBerthsOnly { get; set; }
    public bool AutoUpgrade { get; set; }

    /// <summary>
    /// When true, book on the new IRCTC beta site (https://www.irctc.co.in/eticket/).
    /// When false, use the classic nget train-search UI.
    /// </summary>
    public bool UseBetaView { get; set; }

    /// <summary>
    /// Attach to a real user-started Chrome via CDP (port 9222) instead of Playwright Launch.
    /// Highest chance of surviving Calculate Fare — same Chrome you use manually.
    /// </summary>
    public bool UseRealChrome { get; set; } = true;

    /// <summary>
    /// When true (beta), bot fills UPI then waits — YOU click Calculate Fare once.
    /// Playwright-launched Chrome is often killed on that submit even with a perfect single click.
    /// </summary>
    public bool HandOffCalculateFare { get; set; } = false;

    /// <summary>CDP endpoint, e.g. http://127.0.0.1:9222</summary>
    public string ChromeCdpUrl { get; set; } = "http://127.0.0.1:9222";

    /// <summary>Milliseconds between availability refresh attempts.</summary>
    public int RefreshIntervalMs { get; set; } = 1500;

    /// <summary>Max seconds to keep refreshing for availability (Tatkal).</summary>
    public int AvailabilityTimeoutSeconds { get; set; } = 120;

    /// <summary>
    /// If set (HH:mm:ss local), wait until that clock time before clicking Search on IRCTC.
    /// Empty = search immediately.
    /// </summary>
    public string ScheduledSearchTime { get; set; } = string.Empty;

    private static readonly string ConfigFilePath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public static BookingConfiguration Load()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<BookingConfiguration>(json, JsonOptions);
                if (config != null)
                {
                    config.Credentials.Password = CredentialProtector.Unprotect(config.Credentials.Password);
                    if (config.SavedAccounts != null)
                    {
                        foreach (var acc in config.SavedAccounts)
                            acc.Password = CredentialProtector.Unprotect(acc.Password);
                    }
                    if (config.SavedBanks != null)
                    {
                        foreach (var bank in config.SavedBanks)
                            bank.Password = CredentialProtector.Unprotect(bank.Password);
                    }
                    if (config.SavedCards != null)
                    {
                        foreach (var card in config.SavedCards)
                        {
                            card.Pin = CredentialProtector.Unprotect(card.Pin);
                            card.Cvv = CredentialProtector.Unprotect(card.Cvv);
                            card.ThreeDPassword = CredentialProtector.Unprotect(card.ThreeDPassword);
                        }
                    }
                    return config;
                }
            }
        }
        catch
        {
            // Ignore load errors and return default
        }

        return new BookingConfiguration();
    }

    public void Save()
    {
        try
        {
            var toSave = new BookingConfiguration
            {
                Credentials = new IrctcCredentials
                {
                    Username = Credentials.Username,
                    Password = CredentialProtector.Protect(Credentials.Password)
                },
                SavedAccounts = SavedAccounts.Select(a => new IrctcCredentials
                {
                    Username = a.Username,
                    Password = CredentialProtector.Protect(a.Password)
                }).ToList(),
                SavedBanks = SavedBanks.Select(b => new BankDetails
                {
                    Gateway = b.Gateway,
                    BankName = b.BankName,
                    UserName = b.UserName,
                    Password = CredentialProtector.Protect(b.Password),
                    NameToSave = b.NameToSave
                }).ToList(),
                SavedCards = SavedCards.Select(c => new CardDetails
                {
                    CardCategory = c.CardCategory,
                    Gateway = c.Gateway,
                    BankName = c.BankName,
                    CardType = c.CardType,
                    CardNumber = c.CardNumber,
                    ExpiryMonth = c.ExpiryMonth,
                    ExpiryYear = c.ExpiryYear,
                    NameOnCard = c.NameOnCard,
                    Pin = CredentialProtector.Protect(c.Pin),
                    Cvv = CredentialProtector.Protect(c.Cvv),
                    ThreeDPassword = CredentialProtector.Protect(c.ThreeDPassword),
                    NameToSave = c.NameToSave
                }).ToList(),
                Passengers = Passengers.Select(p => new Passenger
                {
                    Name = p.Name,
                    Age = p.Age,
                    Gender = p.Gender,
                    BerthPreference = p.BerthPreference,
                    FoodPreference = p.FoodPreference
                }).ToList(),
                PreferredClass = PreferredClass,
                Quota = Quota,
                PaymentMethod = PaymentMethod,
                PaymentProvider = PaymentProvider,
                AutoPay = AutoPay,
                MobileNumber = MobileNumber,
                ConfirmBerthsOnly = ConfirmBerthsOnly,
                AutoUpgrade = AutoUpgrade,
                UseBetaView = UseBetaView,
                UseRealChrome = UseRealChrome,
                HandOffCalculateFare = HandOffCalculateFare,
                ChromeCdpUrl = ChromeCdpUrl,
                RefreshIntervalMs = RefreshIntervalMs,
                AvailabilityTimeoutSeconds = AvailabilityTimeoutSeconds,
                ScheduledSearchTime = ScheduledSearchTime
            };

            var json = JsonSerializer.Serialize(toSave, JsonOptions);
            File.WriteAllText(ConfigFilePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }
}

/// <summary>Protects secrets at rest with Windows DPAPI (CurrentUser).</summary>
internal static class CredentialProtector
{
    private const string Prefix = "dpapi:";

    public static string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return string.Empty;
        }

        if (plainText.StartsWith(Prefix, StringComparison.Ordinal))
        {
            return plainText;
        }

        try
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var protectedBytes = ProtectedData.Protect(bytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
            return Prefix + Convert.ToBase64String(protectedBytes);
        }
        catch
        {
            return plainText;
        }
    }

    public static string Unprotect(string stored)
    {
        if (string.IsNullOrEmpty(stored))
        {
            return string.Empty;
        }

        if (!stored.StartsWith(Prefix, StringComparison.Ordinal))
        {
            // Legacy plaintext config — leave as-is until next save
            return stored;
        }

        try
        {
            var protectedBytes = Convert.FromBase64String(stored[Prefix.Length..]);
            var bytes = ProtectedData.Unprotect(protectedBytes, optionalEntropy: null, DataProtectionScope.CurrentUser);
            return Encoding.UTF8.GetString(bytes);
        }
        catch
        {
            return string.Empty;
        }
    }
}

