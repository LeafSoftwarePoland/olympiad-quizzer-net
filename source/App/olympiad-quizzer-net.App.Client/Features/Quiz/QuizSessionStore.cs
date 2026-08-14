using System.Text.Json;
using OlympiadQuizzer.App.Client.Shared.Services;
using OlympiadQuizzer.Core.Domain.Serialization;
using OlympiadQuizzer.Core.Domain.Session;

namespace OlympiadQuizzer.App.Client.Features.Quiz;

public sealed class QuizSessionStore
{
    private const string _sessionKey = "oqn.session.v1";

    private readonly LocalStorageService _storage;
    private readonly ILogger<QuizSessionStore> _logger;

    public QuizSessionStore(LocalStorageService storage, ILogger<QuizSessionStore> logger)
    {
        _storage = storage;
        _logger = logger;
    }

    public async Task<QuizSessionState> TryLoadAsync()
    {
        string raw = await _storage.GetRawStringAsync(_sessionKey);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return null;
        }

        QuizSessionState candidate;
        try
        {
            candidate = JsonSerializer.Deserialize<QuizSessionState>(raw, JsonOptions.Default);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Discarding unreadable quiz session snapshot.");
            await ClearAsync();
            return null;
        }

        if (!QuizSessionValidator.IsValid(candidate, DateTimeOffset.UtcNow))
        {
            _logger.LogWarning("Quiz session snapshot failed validation; discarding.");
            await ClearAsync();
            return null;
        }

        return candidate;
    }

    public async Task SaveAsync(QuizSessionState state)
    {
        try
        {
            await _storage.SetItemAsync(_sessionKey, state);
        }
        catch (Exception ex)
        {
            // Quota exceeded or other JS interop failure; quiz continues in memory.
            _logger.LogWarning(ex, "Could not persist quiz session to localStorage.");
        }
    }

    public async Task ClearAsync()
    {
        await _storage.RemoveItemAsync(_sessionKey);
    }
}
