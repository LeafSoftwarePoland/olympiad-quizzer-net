using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using OlympiadQuizzer.Client;
using OlympiadQuizzer.Client.Services;
using OlympiadQuizzer.Shared;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBase = builder.Configuration["ApiBaseUrl"]
              ?? throw new InvalidOperationException("ApiBaseUrl missing from wwwroot/appsettings.json");

builder.Services.AddScoped(_ => new HttpClient
{
    BaseAddress = new Uri(apiBase.TrimEnd('/') + "/"),
    Timeout = TimeSpan.FromSeconds(90)
});
builder.Services.AddScoped<IQuestionRepository, ApiQuestionRepository>();
builder.Services.AddSingleton<QuizSession>();

await builder.Build().RunAsync();
