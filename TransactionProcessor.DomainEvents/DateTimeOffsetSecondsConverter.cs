using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TransactionProcessor.DomainEvents;

/// <summary>
/// Writes event timestamps with second precision while accepting standard ISO 8601 input.
/// </summary>
public sealed class DateTimeOffsetSecondsConverter : JsonConverter<DateTimeOffset>
{
    private const string Format = "yyyy-MM-dd'T'HH:mm:sszzz";

    public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        string value = reader.GetString() ?? throw new JsonException("Expected a date/time string.");
        return DateTimeOffset.Parse(value, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
    }

    public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
    {
        string timestamp = value.ToString(Format, CultureInfo.InvariantCulture);
        writer.WriteRawValue($"\"{timestamp}\"");
    }
}
