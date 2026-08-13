using OlympiadQuizzer.Domain.Grading;
using OlympiadQuizzer.Domain.Questions;

namespace OlympiadQuizzer.Domain.Session;

public sealed class QuizSessionState
{
    public int SchemaVersion { get; set; } = 1;
    public string ModeId { get; set; }
    public bool Timed { get; set; }
    public int TimeLimitMinutes { get; set; }
    public DateTimeOffset StartedAtUtc { get; set; }
    public int CurrentIndex { get; set; }
    public List<Question> Questions { get; set; } = new();
    public List<SubmittedAnswer> Answers { get; set; } = new();
}
