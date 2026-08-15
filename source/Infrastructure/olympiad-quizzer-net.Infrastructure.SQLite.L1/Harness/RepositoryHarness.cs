using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;
using OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;

internal static class RepositoryHarness
{
    internal const int DefaultSeed = 20260813;

    internal static SqliteQuestionRepository Repository(SqliteFixtureHarness harness)
    {
        return Repository(harness, new SeededShuffler(DefaultSeed));
    }

    internal static SqliteQuestionRepository Repository(SqliteFixtureHarness harness, IShuffler shuffler)
    {
        var options = Options.Create(new QuestionBankOptions { DatabasePath = harness.DatabasePath });
        var store = new SqliteQuestionStore(options);
        return new SqliteQuestionRepository(store, shuffler, NullLogger<SqliteQuestionRepository>.Instance);
    }

    internal static int[] SortedIds(IReadOnlyList<Question> questions)
    {
        return [.. questions.Select(q => q.Id).OrderBy(id => id)];
    }

    internal static int[] IdsInOrder(IReadOnlyList<Question> questions)
    {
        return [.. questions.Select(q => q.Id)];
    }
}
