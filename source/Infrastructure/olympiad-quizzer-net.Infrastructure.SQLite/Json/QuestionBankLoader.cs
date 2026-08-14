using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.Infrastructure.SQLite.Json;

/// Reads the question bank once, at construction, and throws if it is missing, empty or unreadable.
public sealed class QuestionBankLoader
{
    private readonly QuestionBank _bank;

    public QuestionBankLoader(IOptions<QuestionBankOptions> options, ILogger<QuestionBankLoader> logger)
    {
        var configured = options.Value.FilePath;
        var path = Path.IsPathRooted(configured)
            ? configured
            : Path.Combine(AppContext.BaseDirectory, configured);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Question bank not found at '{path}'.", path);
        }

        // ReadAllText strips a UTF-8 BOM; handing the raw byte stream to the deserialiser would not,
        // and the resulting error never mentions the BOM. The bank is hand-edited, so a BOM happens.
        var json = File.ReadAllText(path);

        var questions = JsonSerializer.Deserialize<List<Question>>(json, JsonOptions.Default);

        if (questions == null || questions.Count == 0)
        {
            throw new InvalidOperationException($"Question bank at '{path}' is empty or unreadable.");
        }

        if (questions.Contains(null))
        {
            // A bare null in the array deserialises without any JsonException, and would then throw
            // inside every filter predicate on every request instead of here, once, at startup.
            throw new InvalidOperationException($"Question bank at '{path}' contains a null entry.");
        }

        _bank = new QuestionBank(questions);

        logger.LogInformation(
            "Question bank loaded: {QuestionCount} questions from {BankPath}",
            questions.Count, path);
    }

    public QuestionBank Bank => _bank;
}
