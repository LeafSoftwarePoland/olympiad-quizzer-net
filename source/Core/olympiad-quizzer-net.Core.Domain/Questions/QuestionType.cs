using System.Text.Json.Serialization;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.Core.Domain.Questions;

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
