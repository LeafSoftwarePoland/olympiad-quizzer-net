namespace OlympiadQuizzer.Domain.Questions;

public sealed class ContentBlock
{
    public string Type { get; set; } = ContentBlockType.Text;
    public string Text { get; set; }
    public string File { get; set; }
    public string Alt  { get; set; }
}
