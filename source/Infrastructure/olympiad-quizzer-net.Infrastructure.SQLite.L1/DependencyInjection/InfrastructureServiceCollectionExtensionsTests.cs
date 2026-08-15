using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Infrastructure.SQLite.DependencyInjection;
using OlympiadQuizzer.Infrastructure.SQLite.Json;
using OlympiadQuizzer.Infrastructure.SQLite.L1.Harness;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;
using OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

namespace OlympiadQuizzer.Infrastructure.SQLite.L1.DependencyInjection;

[Trait(TestTiers.Tier, TestTiers.L1)]
public sealed class InfrastructureServiceCollectionExtensionsTests : IDisposable
{
    private readonly SqliteFixtureHarness _harness;

    public InfrastructureServiceCollectionExtensionsTests()
    {
        _harness = new SqliteFixtureHarness("filtering-bank.json");
    }

    public void Dispose() => _harness.Dispose();

    private ServiceProvider BuildProvider()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [QuestionBankOptions.SectionName + ":DatabasePath"] = _harness.DatabasePath
            })
            .Build();

        ServiceCollection services = new();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddQuestionBankInfrastructure(configuration);

        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddQuestionBankInfrastructure_ResolvesIQuestionRepository_ReturnsSqliteQuestionRepository()
    {
        ServiceProvider provider = BuildProvider();

        IQuestionRepository repository = provider.GetRequiredService<IQuestionRepository>();

        Assert.IsType<SqliteQuestionRepository>(repository);
    }

    [Fact]
    public async Task AddQuestionBankInfrastructure_BindsDatabasePathFromConfiguration_DoesLoadThatBank()
    {
        ServiceProvider provider = BuildProvider();

        IQuestionRepository repository = provider.GetRequiredService<IQuestionRepository>();
        IReadOnlyList<Question> result = await repository.GetAsync(new QuestionQuery(), CancellationToken.None);

        Assert.Equal(15, result.Count);
    }

    [Fact]
    public void AddQuestionBankInfrastructure_Default_ReturnsFisherYatesShuffler()
    {
        ServiceProvider provider = BuildProvider();

        IShuffler shuffler = provider.GetRequiredService<IShuffler>();

        Assert.IsType<FisherYatesShuffler>(shuffler);
    }

    [Fact]
    public void WarmQuestionBank_WithMissingDatabaseFile_ThrowsFileNotFoundException()
    {
        string missingPath = Path.Combine(Path.GetTempPath(), "does-not-exist-" + Guid.NewGuid().ToString("N") + ".db");

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [QuestionBankOptions.SectionName + ":DatabasePath"] = missingPath
            })
            .Build();

        ServiceCollection services = new();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddQuestionBankInfrastructure(configuration);

        ServiceProvider provider = services.BuildServiceProvider();

        Assert.Throws<FileNotFoundException>(() => provider.WarmQuestionBank());
    }
}
