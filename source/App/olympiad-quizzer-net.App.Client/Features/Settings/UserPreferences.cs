namespace OlympiadQuizzer.App.Client.Features.Settings;

public sealed class UserPreferences
{
    public string Theme { get; set; } = "dark";
    public float FontScale { get; set; } = 1.0f;
    public bool PrivacyNoticeAcknowledged { get; set; }
}
