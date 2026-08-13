using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OlympiadQuizzer.Domain.Abstractions;
using OlympiadQuizzer.Domain.Queries;
using OlympiadQuizzer.Domain.Questions;
using OlympiadQuizzer.Domain.Serialization;
using OlympiadQuizzer.Infrastructure.SQLite.DependencyInjection;

namespace OlympiadQuizzer.Api;

public partial class Program
{
    private const string CorsPolicyName = "frontend";

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        // Render injects PORT. 10000 is Render's free-tier default and the local fallback.
        string port = Environment.GetEnvironmentVariable("PORT");
        if (string.IsNullOrWhiteSpace(port))
        {
            port = "10000";
        }

        builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

        builder.Services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.PropertyNamingPolicy = JsonOptions.Default.PropertyNamingPolicy;
            o.SerializerOptions.PropertyNameCaseInsensitive = JsonOptions.Default.PropertyNameCaseInsensitive;
            o.SerializerOptions.DefaultIgnoreCondition = JsonOptions.Default.DefaultIgnoreCondition;
            o.SerializerOptions.Encoder = JsonOptions.Default.Encoder;

            foreach (System.Text.Json.Serialization.JsonConverter converter in JsonOptions.Default.Converters)
            {
                o.SerializerOptions.Converters.Add(converter);
            }
        });

        builder.Services.AddCors(o => o.AddPolicy(CorsPolicyName, ConfigureCors));
        builder.Services.AddProblemDetails();
        builder.Services.AddQuestionBankInfrastructure(builder.Configuration);

        WebApplication app = builder.Build();

        // Fail-fast bank load: singleton registration is lazy, so without this the bank is read on the
        // first request and a health check that skips the bank endpoint would not catch a broken deploy.
        app.Services.WarmQuestionBank();

        app.UseCors(CorsPolicyName);

        string imagesPath = System.IO.Path.Combine(AppContext.BaseDirectory, "Data", "images");
        if (System.IO.Directory.Exists(imagesPath))
        {
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(imagesPath),
                RequestPath = "/images"
            });
        }

        MapEndpoints(app);

        app.Run();
    }
}
