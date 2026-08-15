using System.Text.Json;
using Microsoft.JSInterop;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.App.Client.Shared.Services;

public sealed class LocalStorageService
{
    private readonly IJSRuntime _js;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IJSRuntime js, ILogger<LocalStorageService> logger)
    {
        _js = js;
        _logger = logger;
    }

    // Returned as-is, unvalidated: browser storage is user-writable, so every caller must
    // validate before use rather than trusting what comes back.
    public async Task<string> GetRawStringAsync(string key)
    {
        return await _js.InvokeAsync<string>("localStorage.getItem", key);
    }

    public async Task<T> GetItemAsync<T>(string key)
    {
        string raw = await _js.InvokeAsync<string>("localStorage.getItem", key);
        if (string.IsNullOrWhiteSpace(raw))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(raw, JsonOptions.Default);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Failed to deserialize localStorage key '{Key}'; discarding.", key);
            return default;
        }
    }

    public async Task SetItemAsync<T>(string key, T value)
    {
        string serialized = JsonSerializer.Serialize(value, JsonOptions.Default);
        await _js.InvokeVoidAsync("localStorage.setItem", key, serialized);
    }

    public async Task RemoveItemAsync(string key)
    {
        await _js.InvokeVoidAsync("localStorage.removeItem", key);
    }
}
