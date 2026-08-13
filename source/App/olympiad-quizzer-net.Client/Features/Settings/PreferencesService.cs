using System.Threading.Tasks;
using OlympiadQuizzer.Client.Shared.Services;

namespace OlympiadQuizzer.Client.Features.Settings;

public sealed class PreferencesService
{
    private const string StorageKey = "oqn.prefs.v1";
    private const float MinFontScale = 0.8f;
    private const float MaxFontScale = 1.4f;

    private readonly LocalStorageService _storage;
    private UserPreferences _current;

    public PreferencesService(LocalStorageService storage)
    {
        _storage = storage;
        _current = new UserPreferences();
    }

    public string Theme => _current.Theme;
    public float FontScale => _current.FontScale;

    public async Task LoadAsync()
    {
        UserPreferences loaded = await _storage.GetItemAsync<UserPreferences>(StorageKey);
        if (loaded == null)
        {
            return;
        }

        _current.Theme = SanitizeTheme(loaded.Theme);
        _current.FontScale = ClampFontScale(loaded.FontScale);
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
        await _storage.SetItemAsync(StorageKey, _current);
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
        if (scale < MinFontScale)
        {
            return MinFontScale;
        }

        if (scale > MaxFontScale)
        {
            return MaxFontScale;
        }

        return scale;
    }
}
