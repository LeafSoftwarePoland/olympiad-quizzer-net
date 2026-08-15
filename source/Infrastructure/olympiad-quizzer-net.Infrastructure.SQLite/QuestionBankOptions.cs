namespace OlympiadQuizzer.Infrastructure.SQLite;

public sealed class QuestionBankOptions
{
    public const string SectionName = "QuestionBank";

    public string DatabasePath { get; set; } = Path.Combine("data", "questions.db");

    public string ImagesPath { get; set; } = Path.Combine("data", "images");
}
