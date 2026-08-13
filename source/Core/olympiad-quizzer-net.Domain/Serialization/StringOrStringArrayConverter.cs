using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OlympiadQuizzer.Domain.Serialization;

public sealed class StringOrStringArrayConverter : JsonConverter<List<string>>
{
    // Without HandleNull => true, System.Text.Json short-circuits null tokens
    // for reference-type converters and assigns null directly, bypassing Read.
    public override bool HandleNull => true;

    public override List<string> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return new List<string>();
        }

        if (reader.TokenType == JsonTokenType.String)
        {
            return new List<string> { reader.GetString() };
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            List<string> result = new List<string>();

            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndArray)
                {
                    return result;
                }

                if (reader.TokenType == JsonTokenType.String)
                {
                    result.Add(reader.GetString());
                }
                else if (reader.TokenType == JsonTokenType.Number)
                {
                    // Numeric and boolean coercion allows a half-migrated bank ([0, 1] indices)
                    // to deserialise and be named by the integrity test rather than killing the parser.
                    result.Add(reader.GetDecimal().ToString(CultureInfo.InvariantCulture));
                }
                else if (reader.TokenType == JsonTokenType.True)
                {
                    result.Add("true");
                }
                else if (reader.TokenType == JsonTokenType.False)
                {
                    result.Add("false");
                }
                else
                {
                    throw new JsonException("correctAnswer array may contain strings only.");
                }
            }

            throw new JsonException("Unterminated correctAnswer array.");
        }

        throw new JsonException("correctAnswer must be a string or an array of strings.");
    }

    public override void Write(Utf8JsonWriter writer, List<string> value, JsonSerializerOptions options)
    {
        if (value != null && value.Count == 1)
        {
            writer.WriteStringValue(value[0]);
            return;
        }

        writer.WriteStartArray();
        if (value != null)
        {
            foreach (string item in value)
            {
                writer.WriteStringValue(item);
            }
        }
        writer.WriteEndArray();
    }
}
