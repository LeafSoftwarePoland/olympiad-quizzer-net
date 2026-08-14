using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OlympiadQuizzer.Core.Domain.Serialization;

public static class JsonOptions
{
    public static readonly JsonSerializerOptions Default = Create();

    private static JsonSerializerOptions Create()
    {
        JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy        = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition      = JsonIgnoreCondition.Never,
            // Polish diacritics and mathematical Unicode would otherwise become \uXXXX sequences,
            // roughly doubling the payload. Safe because rendered text never reaches a raw-HTML sink.
            Encoder                     = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            ReadCommentHandling         = JsonCommentHandling.Skip,
            AllowTrailingCommas         = true
        };

        // JsonStringEnumConverter intentionally omitted: QuestionType carries a type-level
        // [JsonConverter] attribute pointing at QuestionTypeConverter. Adding a generic factory
        // here for enum types causes it to win over the attribute in .NET 10 and breaks the
        // Unknown-on-unrecognised-string semantics. Add per-type attributes for any future enum.

        return options;
    }
}
