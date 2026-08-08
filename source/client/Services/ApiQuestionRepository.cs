using System.Net.Http.Json;
using OlympiadQuizzer.Shared;

namespace OlympiadQuizzer.Client.Services;

public sealed class ApiQuestionRepository(HttpClient http) : IQuestionRepository
{
    public async Task<List<Question>> GetAsync(QuizFilter filter)
    {
        var all = await http.GetFromJsonAsync<List<Question>>("api/questions", JsonOptions.Default) ?? new();
        return filter.Limit is int n && n > 0 ? all.Take(n).ToList() : all;
    }
}
