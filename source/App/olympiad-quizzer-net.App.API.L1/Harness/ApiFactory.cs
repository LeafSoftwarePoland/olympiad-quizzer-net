using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OlympiadQuizzer.Infrastructure.SQLite.Randomization;

namespace OlympiadQuizzer.App.Api.L1.Harness;

public class ApiFactory : WebApplicationFactory<Program>
{
    public const int Seed = 20260813;

    private readonly string _bankPath;

    public ApiFactory() : this(DefaultBankPath()) { }

    // Protected so derived factories can specify the bank without exposing a second
    // public constructor (xUnit class fixtures must have exactly one public constructor).
    protected ApiFactory(string bankPath)
    {
        _bankPath = bankPath;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseContentRoot(AppContext.BaseDirectory);
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string>
            {
                ["QuestionBank:FilePath"] = _bankPath
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IShuffler>();
            services.AddSingleton<IShuffler>(new SeededShuffler(Seed));
        });
    }

    private static string DefaultBankPath()
    {
        string realBank = Path.Combine(AppContext.BaseDirectory, "Data", "questions.json");
        if (File.Exists(realBank))
        {
            return realBank;
        }

        return FixturePath.Resolve("filtering-bank.json");
    }
}
