using Newtonsoft.Json;

namespace Skua.Core.Models.Converters;

public class StringBoolConverter : JsonConverter
{
    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer serializer)
    {
        writer.WriteValue((value is bool b && b) ? "1" : "0");
    }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        string? val = reader.Value?.ToString();
        return val == "1" || val == "true";
    }

    public override bool CanConvert(Type objectType)
    {
        return objectType == typeof(bool);
    }
}