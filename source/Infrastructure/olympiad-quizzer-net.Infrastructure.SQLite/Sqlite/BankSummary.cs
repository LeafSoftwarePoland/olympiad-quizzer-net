using OlympiadQuizzer.Core.Domain.Queries;

namespace OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

public sealed class BankSummary
{
    public int TotalCount { get; set; }
    public IReadOnlyList<FilterOption> Stages { get; set; } = [];
    public IReadOnlyList<FilterOption> Years { get; set; } = [];
    public IReadOnlyList<string> CategoryJsons { get; set; } = [];
    public IReadOnlyList<string> AlgorithmJsons { get; set; } = [];
}
