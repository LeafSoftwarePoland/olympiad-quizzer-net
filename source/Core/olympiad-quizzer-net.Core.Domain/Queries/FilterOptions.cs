namespace OlympiadQuizzer.Core.Domain.Queries;

public sealed class FilterOptions
{
    public List<FilterOption> Categories { get; set; } = [];
    public List<FilterOption> Algorithms { get; set; } = [];
    public List<FilterOption> Years      { get; set; } = [];
    public List<FilterOption> Stages     { get; set; } = [];
    public int TotalQuestions { get; set; }
}
