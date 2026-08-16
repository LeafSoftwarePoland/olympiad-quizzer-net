using System.Text.Json.Serialization;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.Core.Domain.Questions;

[JsonConverter(typeof(QuestionTypeConverter))]
public enum QuestionType
{
    Single      = 1,
    Multi       = 2,
    ShortAnswer = 3,
    TrueFalse   = 4,
    Ordering    = 5,
    Matching    = 6
}
