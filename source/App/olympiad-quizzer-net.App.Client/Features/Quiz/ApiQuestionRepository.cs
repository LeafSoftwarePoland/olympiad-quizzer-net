using System.Globalization;
using System.Net.Http.Json;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.App.Client.Features.Quiz;

public sealed class ApiQuestionRepository : IQuestionRepository
{
    private readonly HttpClient _http;

    public ApiQuestionRepository(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<Question>> GetAsync(QuestionQuery query, CancellationToken cancellationToken)
    {
        List<string> parts = [];

        foreach (string value in query.Categories)
        {
            parts.Add("category=" + Uri.EscapeDataString(value));
        }

        foreach (string value in query.Algorithms)
        {
            parts.Add("algorithms=" + Uri.EscapeDataString(value));
        }

        foreach (string value in query.Stages)
        {
            parts.Add("stage=" + Uri.EscapeDataString(value));
        }

        foreach (int value in query.Years)
        {
            parts.Add("year=" + value.ToString(CultureInfo.InvariantCulture));
        }

        parts.Add("limit=" + query.Limit.ToString(CultureInfo.InvariantCulture));

        string url = "v1/questions?" + string.Join("&", parts);

        List<Question> questions = await _http.GetFromJsonAsync<List<Question>>(url, JsonOptions.Default, cancellationToken);

        return questions ?? [];
    }

    public async Task<FilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken)
    {
        FilterOptions options = await _http.GetFromJsonAsync<FilterOptions>("v1/filters", JsonOptions.Default, cancellationToken);

        return options ?? new FilterOptions();
    }
}
