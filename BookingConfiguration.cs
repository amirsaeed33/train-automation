using System.Text.Json;

namespace train_automation;

public sealed class BookingConfiguration
{
    public IrctcCredentials Credentials { get; set; } = new();
    public List<Passenger> Passengers { get; set; } = new();

    private static readonly string ConfigFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.json");

    public static BookingConfiguration Load()
    {
        try
        {
            if (File.Exists(ConfigFilePath))
            {
                var json = File.ReadAllText(ConfigFilePath);
                var config = JsonSerializer.Deserialize<BookingConfiguration>(json);
                if (config != null)
                {
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
            var json = JsonSerializer.Serialize(this, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(ConfigFilePath, json);
        }
        catch
        {
            // Ignore save errors
        }
    }
}
