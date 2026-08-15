using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using OlympiadQuizzer.App.Api.Extensions;
using OlympiadQuizzer.App.Api.Middleware;
using OlympiadQuizzer.Infrastructure.SQLite.DependencyInjection;

namespace OlympiadQuizzer.App.Api;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

        builder.Host.UseDefaultServiceProvider(o =>
        {
            o.ValidateOnBuild = true;
            o.ValidateScopes  = true;
        });

        builder.Services.AddApiJsonOptions();
        builder.Services.AddFrontendCors();
        builder.Services.AddProblemDetails();
        builder.Services.AddControllers();
        builder.Services.AddQuestionBankInfrastructure(builder.Configuration);

        WebApplication app = builder.Build();

        app.Services.WarmQuestionBank();

        app.UseMiddleware<GlobalExceptionMiddleware>();
        app.UseFrontendCors();
        app.UseQuestionImages();
        app.MapControllers();

        app.Run();
    }
}
