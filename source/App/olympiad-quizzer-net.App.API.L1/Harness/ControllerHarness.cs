using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OlympiadQuizzer.Core.Tests.Common.Harness;
using OlympiadQuizzer.Infrastructure.SQLite;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;
using OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

namespace OlympiadQuizzer.App.Api.L1.Harness;

internal static class ControllerHarness
{
    internal const int DefaultSeed = 20260813;

    internal static SqliteQuestionRepository RealBankRepository()
    {
        string dbPath = Path.Combine(FixturePath.RepoRoot(), "data", "questions.db");
        var options = Options.Create(new QuestionBankOptions { DatabasePath = dbPath });
        var store = new SqliteQuestionStore(options);
        return new SqliteQuestionRepository(
            store,
            new SeededShuffler(DefaultSeed),
            NullLogger<SqliteQuestionRepository>.Instance);
    }
}
