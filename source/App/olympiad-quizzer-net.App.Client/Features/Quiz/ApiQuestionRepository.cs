using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using OlympiadQuizzer.App.Client.Shared.Services;
using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.App.Client.Features.Quiz;

public sealed class ApiQuestionRepository : IQuestionRepository
{
    private readonly HttpClient _http;
    private readonly ErrorMessageService _errorMessages;

    public ApiQuestionRepository(HttpClient http, ErrorMessageService errorMessages)
    {
        _http = http;
        _errorMessages = errorMessages;
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

        HttpResponseMessage response = await _http.GetAsync(url, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string code = await TryReadErrorCodeAsync(response);
            throw new QuizDrawException(_errorMessages.GetMessage(code));
        }

        try
        {
            List<Question> questions = await response.Content.ReadFromJsonAsync<List<Question>>(JsonOptions.Default, cancellationToken);
            return questions ?? [];
        }
        catch (JsonException)
        {
            throw new QuizDrawException(_errorMessages.GetMessage(null));
        }
    }

    public async Task<FilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken)
    {
        HttpResponseMessage response = await _http.GetAsync("v1/filters", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            string code = await TryReadErrorCodeAsync(response);
            throw new QuizDrawException(_errorMessages.GetMessage(code));
        }

        try
        {
            FilterOptions options = await response.Content.ReadFromJsonAsync<FilterOptions>(JsonOptions.Default, cancellationToken);
            return options ?? new FilterOptions();
        }
        catch (JsonException)
        {
            throw new QuizDrawException(_errorMessages.GetMessage(null));
        }
    }

    private static async Task<string> TryReadErrorCodeAsync(HttpResponseMessage response)
    {
        try
        {
            ApiErrorBody body = await response.Content.ReadFromJsonAsync<ApiErrorBody>(JsonOptions.Default);
            return body?.Code;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private sealed class ApiErrorBody
    {
        public string Code { get; set; }
    }
}
