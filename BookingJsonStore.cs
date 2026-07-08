using System.Text.Json;

namespace train_automation;

public static class BookingJsonStore
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true
    };

    public static string FilePath =>
        Path.Combine(AppContext.BaseDirectory, "bookings.json");

    public static void Append(TrainBookingRecord booking)
    {
        var bookings = LoadAll();
        bookings.Add(booking);
        var json = JsonSerializer.Serialize(bookings, JsonOptions);
        File.WriteAllText(FilePath, json);
    }

    private static List<TrainBookingRecord> LoadAll()
    {
        if (!File.Exists(FilePath))
        {
            return [];
        }

        var json = File.ReadAllText(FilePath);
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<TrainBookingRecord>>(json, JsonOptions) ?? [];
    }
}
