using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OlympiadQuizzer.Domain.Abstractions;
using OlympiadQuizzer.Infrastructure.SQLite.Json;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;

namespace OlympiadQuizzer.Infrastructure.SQLite.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddQuestionBankInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<QuestionBankOptions>(
            configuration.GetSection(QuestionBankOptions.SectionName));

        services.AddSingleton<IShuffler, FisherYatesShuffler>();
        services.AddSingleton<QuestionBankLoader>();
        services.AddSingleton<IQuestionRepository, JsonQuestionRepository>();

        return services;
    }

    /// Loads the question bank now. Singleton registration is lazy, so without this call the
    /// fail-fast read happens on the first request instead of at startup — and a health check
    /// that never touches the bank would report a broken process as healthy.
    public static IServiceProvider WarmQuestionBank(this IServiceProvider services)
    {
        services.GetRequiredService<QuestionBankLoader>();
        return services;
    }
}
