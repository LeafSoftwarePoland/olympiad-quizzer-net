using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using OlympiadQuizzer.Api.L1.Harness;
using OlympiadQuizzer.Domain.Abstractions;
using OlympiadQuizzer.Domain.Queries;
using OlympiadQuizzer.Domain.Questions;
using OlympiadQuizzer.Infrastructure.SQLite.DependencyInjection;
using OlympiadQuizzer.Infrastructure.SQLite.Json;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;

namespace OlympiadQuizzer.Api.L1.Infrastructure;

[Trait("Tier", "L1")]
public sealed class InfrastructureRegistrationTests
{
    private const string FilteringBank = "filtering-bank.json";

    private static ServiceProvider BuildProvider(string fixturePath)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [QuestionBankOptions.SectionName + ":FilePath"] = fixturePath
            })
            .Build();

        ServiceCollection services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddQuestionBankInfrastructure(configuration);

        return services.BuildServiceProvider();
    }

    [Fact]
    public void AddQuestionBankInfrastructure_ResolvesIQuestionRepository_ReturnsJsonQuestionRepository()
    {
        ServiceProvider provider = BuildProvider(FixturePath.Resolve(FilteringBank));

        IQuestionRepository repository = provider.GetRequiredService<IQuestionRepository>();

        Assert.IsType<JsonQuestionRepository>(repository);
    }

    [Fact]
    public async Task AddQuestionBankInfrastructure_BindsFilePathFromConfiguration_LoadsThatBank()
    {
        ServiceProvider provider = BuildProvider(FixturePath.Resolve(FilteringBank));

        IQuestionRepository repository = provider.GetRequiredService<IQuestionRepository>();
        IReadOnlyList<Question> result = await repository.GetAsync(new QuestionQuery(), CancellationToken.None);

        Assert.Equal(15, result.Count);
    }

    [Fact]
    public void AddQuestionBankInfrastructure_RegistersFisherYatesShufflerAsDefault()
    {
        ServiceProvider provider = BuildProvider(FixturePath.Resolve(FilteringBank));

        IShuffler shuffler = provider.GetRequiredService<IShuffler>();

        Assert.IsType<FisherYatesShuffler>(shuffler);
    }

    [Fact]
    public void AddQuestionBankInfrastructure_ResolvesQuestionBankLoaderTwice_ReturnsSameInstance()
    {
        ServiceProvider provider = BuildProvider(FixturePath.Resolve(FilteringBank));

        QuestionBankLoader first  = provider.GetRequiredService<QuestionBankLoader>();
        QuestionBankLoader second = provider.GetRequiredService<QuestionBankLoader>();

        Assert.Same(first, second);
    }

    [Fact]
    public void WarmQuestionBank_WithMissingBankFile_ThrowsFileNotFoundException()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                [QuestionBankOptions.SectionName + ":FilePath"] = FixturePath.Resolve("does-not-exist.json")
            })
            .Build();

        ServiceCollection services = new ServiceCollection();
        services.AddSingleton(typeof(ILogger<>), typeof(NullLogger<>));
        services.AddQuestionBankInfrastructure(configuration);

        ServiceProvider provider = services.BuildServiceProvider();

        Assert.Throws<FileNotFoundException>(() => provider.WarmQuestionBank());
    }
}
