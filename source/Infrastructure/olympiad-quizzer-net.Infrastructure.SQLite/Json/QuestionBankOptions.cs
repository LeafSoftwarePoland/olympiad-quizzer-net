namespace OlympiadQuizzer.Infrastructure.SQLite.Json;

public sealed class QuestionBankOptions
{
    public const string SectionName = "QuestionBank";

    /// Absolute, or relative to AppContext.BaseDirectory rather than to the content root.
    public string FilePath { get; set; } = Path.Combine("Data", "questions.json");

    /// Absolute, or relative to AppContext.BaseDirectory. Mapped to /images for static-file serving.
    public string ImagesPath { get; set; } = Path.Combine("data", "images");

    // Absolute, or relative to AppContext.BaseDirectory. Path to the SQLite database.
    public string DatabasePath { get; set; } = Path.Combine("data", "questions.db");
}
