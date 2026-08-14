using System.Net.Http.Json;
using System.Text.Json;

namespace OlympiadQuizzer.App.Client.Shared.Services;

public sealed class AppVersionService
{
    private readonly HttpClient _http;
    private readonly ILogger<AppVersionService> _logger;
    private VersionInfo _info;

    public AppVersionService([FromKeyedServices("static")] HttpClient http, ILogger<AppVersionService> logger)
    {
        _http = http;
        _logger = logger;
        _info = new VersionInfo();
    }

    public async Task<string> GetFrontendVersionAsync()
    {
        await EnsureLoadedAsync();
        return _info.Frontend ?? string.Empty;
    }

    public async Task<string> GetBackendVersionAsync()
    {
        await EnsureLoadedAsync();
        return _info.Backend ?? string.Empty;
    }

    private async Task EnsureLoadedAsync()
    {
        if (_info.Frontend != null)
        {
            return;
        }

        try
        {
            VersionInfo loaded = await _http.GetFromJsonAsync<VersionInfo>("version.json");
            if (loaded != null)
            {
                _info = loaded;
            }
        }
        catch (HttpRequestException ex)
        {
            // version.json absent in local dev or first deploy; non-fatal.
            _logger.LogWarning(ex, "Could not fetch version.json.");
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "version.json could not be parsed.");
        }
    }

    private sealed class VersionInfo
    {
        public string Frontend { get; set; }
        public string Backend { get; set; }
    }
}
