using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OlympiadQuizzer.App.Client.Features.Quiz;
using OlympiadQuizzer.App.Client.Features.Settings;
using OlympiadQuizzer.App.Client.Shared.Services;
using OlympiadQuizzer.Core.Domain.Abstractions;

namespace OlympiadQuizzer.App.Client;

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
        builder.Services.AddScoped<QuizSession>();

        await builder.Build().RunAsync();
    }
}
