using OlympiadQuizzer.App.Client.Shared.Services;

namespace OlympiadQuizzer.App.Client.Features.Settings;

public sealed class PreferencesService
{
    private const string _storageKey = "oqn.prefs.v1";
    private const float _minFontScale = 0.8f;
    private const float _maxFontScale = 1.4f;

    private readonly LocalStorageService _storage;
    private UserPreferences _current;

    public PreferencesService(LocalStorageService storage)
    {
        _storage = storage;
        _current = new UserPreferences();
    }

    public string Theme => _current.Theme;
    public float FontScale => _current.FontScale;
    public bool PrivacyNoticeAcknowledged => _current.PrivacyNoticeAcknowledged;

    public async Task LoadAsync()
    {
        UserPreferences loaded = await _storage.GetItemAsync<UserPreferences>(_storageKey);
        if (loaded == null)
        {
            return;
        }

        _current.Theme = SanitizeTheme(loaded.Theme);
        _current.FontScale = ClampFontScale(loaded.FontScale);
        _current.PrivacyNoticeAcknowledged = loaded.PrivacyNoticeAcknowledged;
    }

    public async Task AcknowledgePrivacyNoticeAsync()
    {
        _current.PrivacyNoticeAcknowledged = true;
        await SaveAsync();
    }

    public async Task SetThemeAsync(string theme)
    {
        _current.Theme = SanitizeTheme(theme);
        await SaveAsync();
    }

    public async Task SetFontScaleAsync(float scale)
    {
        _current.FontScale = ClampFontScale(scale);
        await SaveAsync();
    }

    private async Task SaveAsync()
    {
        await _storage.SetItemAsync(_storageKey, _current);
    }

    private static string SanitizeTheme(string theme)
    {
        if (theme == "ps")
        {
            return "ps";
        }

        return "dark";
    }

    private static float ClampFontScale(float scale)
    {
        if (scale < _minFontScale)
        {
            return _minFontScale;
        }

        if (scale > _maxFontScale)
        {
            return _maxFontScale;
        }

        return scale;
    }
}
