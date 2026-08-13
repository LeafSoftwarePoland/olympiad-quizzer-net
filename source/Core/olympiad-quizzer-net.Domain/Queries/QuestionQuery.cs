namespace OlympiadQuizzer.Domain.Queries;

public sealed class QuestionQuery
{
    public const int MaxLimit     = 30;
    public const int DefaultLimit = 30;

    public List<string> Categories { get; set; } = new();
    public List<string> Algorithms { get; set; } = new();
    public List<int>    Years      { get; set; } = new();
    public List<string> Stages     { get; set; } = new();
    public int Limit { get; set; } = DefaultLimit;

    public bool HasAnyFilter => Categories.Count > 0 || Algorithms.Count > 0 || Years.Count > 0 || Stages.Count > 0;
}
