using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OlympiadQuizzer.App.Api.Endpoints;
using OlympiadQuizzer.App.Api.Extensions;
using OlympiadQuizzer.Infrastructure.SQLite.DependencyInjection;

namespace OlympiadQuizzer.App.Api;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Render injects PORT. 10000 is Render's free-tier default and the local fallback.
        var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

        builder.Services.AddApiJsonOptions();
        builder.Services.AddFrontendCors();
        builder.Services.AddProblemDetails();
        builder.Services.AddQuestionBankInfrastructure(builder.Configuration);

        WebApplication app = builder.Build();

        // Fail-fast bank load: singleton registration is lazy, so without this the bank is read on the
        // first request and a health check that skips the bank endpoint would not catch a broken deploy.
        app.Services.WarmQuestionBank();

        app.UseFrontendCors();
        app.UseQuestionImages();

        QuestionsEndpoints.Map(app);
        FiltersEndpoints.Map(app);
        HealthEndpoints.Map(app);
        RobotsEndpoints.Map(app);

        app.Run();
    }
}
