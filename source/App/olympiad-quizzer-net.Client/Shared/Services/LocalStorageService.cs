using System;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using OlympiadQuizzer.Domain.Serialization;

namespace OlympiadQuizzer.Client.Shared.Services;

public sealed class LocalStorageService
{
    private readonly IJSRuntime _js;
    private readonly ILogger<LocalStorageService> _logger;

    public LocalStorageService(IJSRuntime js, ILogger<LocalStorageService> logger)
    {
        _js = js;
        _logger = logger;
    }

    /// Returns the raw string value stored under the key, or null when absent.
    /// The value is returned as-is — the caller is responsible for deserialization and validation.
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
