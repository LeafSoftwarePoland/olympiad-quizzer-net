using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Infrastructure.SQLite.Json;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;
using OlympiadQuizzer.Infrastructure.SQLite.Sqlite;

namespace OlympiadQuizzer.Infrastructure.SQLite.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddQuestionBankInfrastructure(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<QuestionBankOptions>(
            configuration.GetSection(QuestionBankOptions.SectionName));

        services.AddSingleton<IShuffler, FisherYatesShuffler>();
        services.AddSingleton<IQuestionRepository, SqliteQuestionRepository>();

        return services;
    }

    public static IServiceProvider WarmQuestionBank(this IServiceProvider services)
    {
        services.GetRequiredService<IQuestionRepository>();
        return services;
    }
}
