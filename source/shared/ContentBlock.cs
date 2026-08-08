namespace OlympiadQuizzer.Shared;

public sealed class ContentBlock
{
    public string Type { get; set; } = "text";
    public string? Text { get; set; }
    public string? File { get; set; }
}
