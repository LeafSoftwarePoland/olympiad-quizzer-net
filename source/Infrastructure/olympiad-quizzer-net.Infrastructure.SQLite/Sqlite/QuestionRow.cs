namespace OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

public sealed class QuestionRow
{
    public int Id { get; set; }
    public string Olympiad { get; set; }
    public string Stage { get; set; }
    public int? Year { get; set; }
    public int? Difficulty { get; set; }
    public string Source { get; set; }
    public string SourceUrl { get; set; }
    public string SourceRaw { get; set; }
    public string ExplanationSource { get; set; }
    public string Type { get; set; }
    public string Content { get; set; }
    public string ContentCpp { get; set; }
    public string Options { get; set; }
    public string MatchOptions { get; set; }
    public string CorrectAnswer { get; set; }
    public string Category { get; set; }
    public string Algorithms { get; set; }
    public string Explanation { get; set; }
    public int Points { get; set; }
    public int PartialCredit { get; set; }
    public string ContentHash { get; set; }
}
