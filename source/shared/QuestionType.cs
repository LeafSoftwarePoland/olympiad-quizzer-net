using System.Text.Json.Serialization;

namespace OlympiadQuizzer.Shared;

[JsonConverter(typeof(JsonStringEnumConverter<QuestionType>))]
public enum QuestionType
{
    Unknown = 0,
    MultiSelect,
    SingleAbcd,
    ShortAnswer,
    TrueFalse,
    Ordering,
    Matching
}
