using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.Core.Domain.Session;

public sealed class QuizSessionState
{
    public int SchemaVersion { get; set; } = 1;
    public string ModeId { get; set; }
    public bool Timed { get; set; }
    public int TimeLimitMinutes { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public int CurrentIndex { get; set; }
    public List<Question> Questions { get; set; } = [];
    public List<SubmittedAnswer> Answers { get; set; } = [];
}
