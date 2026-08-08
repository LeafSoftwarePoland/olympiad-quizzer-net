namespace OlympiadQuizzer.Shared;

public sealed class QuizFilter
{
    public string? Source { get; set; }
    public string? Competition { get; set; }
    public QuestionType? Type { get; set; }
    public int? Limit { get; set; }
    public static QuizFilter None { get; } = new();
}
