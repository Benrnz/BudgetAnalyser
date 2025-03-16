using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace BudgetAnalyser.Engine.Persistence;

public class DateOnlyJsonConverter(string format = "yyyy-MM-dd") : JsonConverter<DateOnly>
{
    public override DateOnly Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var dateString = reader.GetString();
        if (DateOnly.TryParseExact(dateString, format, null, DateTimeStyles.None, out var date))
        {
            return date;
        }

        // TODO Here for compatibility with previous format.  Once all files are converted this can be removed.
        if (DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.ffffffZ", null, DateTimeStyles.AssumeUniversal, out var dateTime1))
        {
            return DateOnly.FromDateTime(dateTime1);
        }

        if (DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ss.fffffffZ", null, DateTimeStyles.AssumeUniversal, out var dateTime2))
        {
            return DateOnly.FromDateTime(dateTime2);
        }

        if (DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ssZ", null, DateTimeStyles.AssumeUniversal, out var dateTime3))
        {
            return DateOnly.FromDateTime(dateTime3);
        }

        if (DateTime.TryParseExact(dateString, "yyyy-MM-ddTHH:mm:ss", null, DateTimeStyles.AssumeUniversal, out var dateTime4))
        {
            return DateOnly.FromDateTime(dateTime4);
        }

        throw new JsonException($"Unable to convert \"{dateString}\" to {nameof(DateOnly)}.");
    }

    public override void Write(Utf8JsonWriter writer, DateOnly value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.ToString(format));
    }
}
