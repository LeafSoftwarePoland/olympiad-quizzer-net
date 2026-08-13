using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OlympiadQuizzer.Domain.Questions;
using OlympiadQuizzer.Infrastructure.SQLite.Json;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;

namespace OlympiadQuizzer.Api.L1.Harness;

internal static class RepositoryHarness
{
    internal const int DefaultSeed = 20260813;

    internal static QuestionBankLoader Loader(string fixtureName)
    {
        return Loader(fixtureName, NullLogger<QuestionBankLoader>.Instance);
    }

    internal static QuestionBankLoader Loader(string fixtureName, ILogger<QuestionBankLoader> logger)
    {
        return LoaderForPath(FixturePath.Resolve(fixtureName), logger);
    }

    internal static QuestionBankLoader LoaderForPath(string path, ILogger<QuestionBankLoader> logger)
    {
        QuestionBankOptions options = new QuestionBankOptions { FilePath = path };
        return new QuestionBankLoader(Options.Create(options), logger);
    }

    internal static JsonQuestionRepository Repository(string fixtureName)
    {
        return Repository(fixtureName, new SeededShuffler(DefaultSeed));
    }

    internal static JsonQuestionRepository Repository(string fixtureName, IShuffler shuffler)
    {
        return new JsonQuestionRepository(
            Loader(fixtureName), shuffler, NullLogger<JsonQuestionRepository>.Instance);
    }

    internal static (QuestionBankLoader Loader, JsonQuestionRepository Repository) Pair(
        string fixtureName, IShuffler shuffler)
    {
        QuestionBankLoader loader = Loader(fixtureName);
        JsonQuestionRepository repository = new JsonQuestionRepository(
            loader, shuffler, NullLogger<JsonQuestionRepository>.Instance);
        return (loader, repository);
    }

    internal static int[] SortedIds(IReadOnlyList<Question> questions)
    {
        return questions.Select(question => question.Id).OrderBy(id => id).ToArray();
    }

    internal static int[] IdsInOrder(IReadOnlyList<Question> questions)
    {
        return questions.Select(question => question.Id).ToArray();
    }
}
