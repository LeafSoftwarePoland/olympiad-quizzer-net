namespace OlympiadQuizzer.Domain.Queries;

public sealed class FilterOptions
{
    public List<FilterOption> Categories { get; set; } = new();
    public List<FilterOption> Algorithms { get; set; } = new();
    public List<FilterOption> Years      { get; set; } = new();
    public List<FilterOption> Stages     { get; set; } = new();
    public int TotalQuestions { get; set; }
}
