namespace OlympiadQuizzer.Core.Domain.Grading;

public sealed class SubmittedAnswer
{
    public List<string> Values { get; set; } = [];

    public static SubmittedAnswer Empty => new();

    public bool IsEmpty => Values == null || Values.Count == 0;
}
