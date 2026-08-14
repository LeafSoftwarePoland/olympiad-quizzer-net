using System.Text.Json;
using System.Text.Json.Serialization;
using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.Core.Domain.Serialization;

public sealed class QuestionTypeConverter : JsonConverter<QuestionType>
{
    public override QuestionType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.Null)
        {
            return QuestionType.Unknown;
        }

        if (reader.TokenType != JsonTokenType.String)
        {
            throw new JsonException("type must be a string.");
        }

        string value = reader.GetString();

        return value switch
        {
            string s when string.Equals(s, "single",      StringComparison.OrdinalIgnoreCase) => QuestionType.Single,
            string s when string.Equals(s, "multi",       StringComparison.OrdinalIgnoreCase) => QuestionType.Multi,
            string s when string.Equals(s, "shortAnswer", StringComparison.OrdinalIgnoreCase) => QuestionType.ShortAnswer,
            string s when string.Equals(s, "trueFalse",   StringComparison.OrdinalIgnoreCase) => QuestionType.TrueFalse,
            string s when string.Equals(s, "ordering",    StringComparison.OrdinalIgnoreCase) => QuestionType.Ordering,
            string s when string.Equals(s, "matching",    StringComparison.OrdinalIgnoreCase) => QuestionType.Matching,
            string s when string.Equals(s, "unknown",     StringComparison.OrdinalIgnoreCase) => QuestionType.Unknown,
            _ => QuestionType.Unknown
        };
    }

    public override void Write(Utf8JsonWriter writer, QuestionType value, JsonSerializerOptions options)
    {
        string wireValue = value switch
        {
            QuestionType.Single      => "single",
            QuestionType.Multi       => "multi",
            QuestionType.ShortAnswer => "shortAnswer",
            QuestionType.TrueFalse   => "trueFalse",
            QuestionType.Ordering    => "ordering",
            QuestionType.Matching    => "matching",
            _                        => "unknown"
        };

        writer.WriteStringValue(wireValue);
    }
}
