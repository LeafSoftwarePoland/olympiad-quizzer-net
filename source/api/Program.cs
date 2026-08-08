using System.Text.Json;
using OlympiadQuizzer.Shared;

var builder = WebApplication.CreateBuilder(args);

var port = Environment.GetEnvironmentVariable("PORT") ?? "10000";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.ConfigureHttpJsonOptions(o =>
{
    o.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    foreach (var c in JsonOptions.Default.Converters) o.SerializerOptions.Converters.Add(c);
});

const string CorsPolicy = "poc";
builder.Services.AddCors(o => o.AddPolicy(CorsPolicy, p => p
    .WithOrigins("https://leafsoftwarepoland.github.io")
    .SetIsOriginAllowed(origin =>
        origin == "https://leafsoftwarepoland.github.io" ||
        (Uri.TryCreate(origin, UriKind.Absolute, out var u) &&
         (u.Host == "localhost" || u.Host == "127.0.0.1")))
    .AllowAnyHeader()
    .AllowAnyMethod()));

var dataPath = Path.Combine(AppContext.BaseDirectory, "Data", "questions.json");
var questions = JsonSerializer.Deserialize<List<Question>>(File.ReadAllText(dataPath), JsonOptions.Default)
                ?? throw new InvalidOperationException($"questions.json empty or unreadable: {dataPath}");
if (questions.Count == 0) throw new InvalidOperationException("questions.json contains no questions.");

var app = builder.Build();

app.UseCors(CorsPolicy);

app.MapGet("/healthz", () => Results.Ok(new { ok = true }));
app.MapGet("/api/questions", () => Results.Ok(questions));

app.Run();

public partial class Program;
