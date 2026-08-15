using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Infrastructure.SQLite.Json;
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
        QuestionBankOptions options = new() { DatabasePath = harness.DatabasePath };
        return new SqliteQuestionRepository(
            Options.Create(options),
            shuffler,
            NullLogger<SqliteQuestionRepository>.Instance);
    }

    internal static QuestionBankLoader Loader(string fixtureName)
    {
        return Loader(fixtureName, NullLogger<QuestionBankLoader>.Instance);
    }

    internal static QuestionBankLoader Loader(string fixtureName, ILogger<QuestionBankLoader> logger)
    {
        QuestionBankOptions options = new() { FilePath = FixturePath.Resolve(fixtureName) };
        return new QuestionBankLoader(Options.Create(options), logger);
    }

    internal static QuestionBankLoader LoaderForPath(string path, ILogger<QuestionBankLoader> logger)
    {
        QuestionBankOptions options = new() { FilePath = path };
        return new QuestionBankLoader(Options.Create(options), logger);
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
