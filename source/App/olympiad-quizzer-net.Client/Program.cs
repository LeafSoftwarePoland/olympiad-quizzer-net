using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OlympiadQuizzer.Client.Features.Quiz;
using OlympiadQuizzer.Client.Features.Settings;
using OlympiadQuizzer.Client.Shared.Services;
using OlympiadQuizzer.Domain.Abstractions;

namespace OlympiadQuizzer.Client;

public class Program
{
    public static async Task Main(string[] args)
    {
        WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

        builder.RootComponents.Add<App>("#app");
        builder.RootComponents.Add<HeadOutlet>("head::after");

        string apiBase = builder.Configuration["ApiBaseUrl"];
        if (string.IsNullOrWhiteSpace(apiBase))
        {
            throw new InvalidOperationException("ApiBaseUrl missing from wwwroot/appsettings.json");
        }

        builder.Services.AddScoped(_ => new HttpClient
        {
            BaseAddress = new Uri(apiBase.TrimEnd('/') + "/"),
            Timeout = TimeSpan.FromSeconds(90)
        });

        builder.Services.AddKeyedScoped("static", (_, _) => new HttpClient
        {
            BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)
        });

        builder.Services.AddScoped<IQuestionRepository, ApiQuestionRepository>();
        builder.Services.AddScoped<LocalStorageService>();
        builder.Services.AddScoped<ToastService>();
        builder.Services.AddScoped<AppVersionService>();
        builder.Services.AddScoped<ModeCatalog>();
        builder.Services.AddScoped<PreferencesService>();
        builder.Services.AddScoped<QuizSessionStore>();
        builder.Services.AddSingleton<QuizSession>();

        await builder.Build().RunAsync();
    }
}
