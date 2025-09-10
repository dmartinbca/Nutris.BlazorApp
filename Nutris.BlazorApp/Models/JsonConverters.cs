using System.Text.Json;
using System.Text.Json.Serialization;

namespace NutrisBlazor.Models.Converters
{
    /// <summary>
    /// Convierte valores JSON a string, manejando números y booleanos
    /// </summary>
    public class FlexibleStringConverter : JsonConverter<string>
    {
        public override string Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.String:
                    return reader.GetString() ?? "";
                case JsonTokenType.Number:
                    if (reader.TryGetInt64(out long longValue))
                        return longValue.ToString();
                    if (reader.TryGetDouble(out double doubleValue))
                        return doubleValue.ToString();
                    return reader.GetDecimal().ToString();
                case JsonTokenType.True:
                    return "true";
                case JsonTokenType.False:
                    return "false";
                case JsonTokenType.Null:
                    return "";
                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, string value, JsonSerializerOptions options)
        {
            writer.WriteStringValue(value);
        }
    }

    /// <summary>
    /// Convierte valores JSON a int, manejando strings y decimales
    /// </summary>
    public class FlexibleIntConverter : JsonConverter<int>
    {
        public override int Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    if (reader.TryGetInt32(out int intValue))
                        return intValue;
                    if (reader.TryGetDouble(out double doubleValue))
                        return (int)Math.Round(doubleValue);
                    return (int)reader.GetDecimal();
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (string.IsNullOrEmpty(stringValue))
                        return 0;
                    if (int.TryParse(stringValue, out int parsed))
                        return parsed;
                    if (double.TryParse(stringValue, out double parsedDouble))
                        return (int)Math.Round(parsedDouble);
                    return 0;
                case JsonTokenType.Null:
                    return 0;
                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, int value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    /// <summary>
    /// Convierte valores JSON a decimal, manejando strings
    /// </summary>
    public class FlexibleDecimalConverter : JsonConverter<decimal>
    {
        public override decimal Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.Number:
                    if (reader.TryGetDecimal(out decimal decimalValue))
                        return decimalValue;
                    if (reader.TryGetDouble(out double doubleValue))
                        return (decimal)doubleValue;
                    return reader.GetInt32();
                case JsonTokenType.String:
                    var stringValue = reader.GetString();
                    if (string.IsNullOrEmpty(stringValue))
                        return 0;
                    if (decimal.TryParse(stringValue, out decimal parsed))
                        return parsed;
                    if (double.TryParse(stringValue, out double parsedDouble))
                        return (decimal)parsedDouble;
                    return 0;
                case JsonTokenType.Null:
                    return 0;
                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, decimal value, JsonSerializerOptions options)
        {
            writer.WriteNumberValue(value);
        }
    }

    /// <summary>
    /// Convierte valores JSON a bool, manejando strings y números
    /// </summary>
    public class FlexibleBoolConverter : JsonConverter<bool>
    {
        public override bool Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.True:
                    return true;
                case JsonTokenType.False:
                    return false;
                case JsonTokenType.String:
                    var stringValue = reader.GetString()?.ToLower();
                    return stringValue == "true" || stringValue == "1" || stringValue == "yes";
                case JsonTokenType.Number:
                    return reader.GetInt32() != 0;
                case JsonTokenType.Null:
                    return false;
                default:
                    throw new JsonException($"Unexpected token type: {reader.TokenType}");
            }
        }

        public override void Write(Utf8JsonWriter writer, bool value, JsonSerializerOptions options)
        {
            writer.WriteBooleanValue(value);
        }
    }
}