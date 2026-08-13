namespace OlympiadQuizzer.Domain.Grading;

public sealed class SubmittedAnswer
{
    public List<string> Values { get; set; } = new();

    public static SubmittedAnswer Empty => new SubmittedAnswer();

    public bool IsEmpty => Values == null || Values.Count == 0;
}
