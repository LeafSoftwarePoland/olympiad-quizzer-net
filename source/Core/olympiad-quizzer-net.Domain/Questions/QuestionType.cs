using System.Text.Json.Serialization;
using OlympiadQuizzer.Domain.Serialization;

namespace OlympiadQuizzer.Domain.Questions;

[JsonConverter(typeof(QuestionTypeConverter))]
public enum QuestionType
{
    Unknown = 0,
    Single,
    Multi,
    ShortAnswer,
    TrueFalse,
    Ordering,
    Matching
}
