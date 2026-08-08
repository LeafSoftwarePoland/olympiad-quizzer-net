namespace OlympiadQuizzer.Shared;

public sealed class AnswerSubmission
{
    public List<int> SelectedIndices { get; set; } = new();
    public string? Text { get; set; }
    public List<bool?> Booleans { get; set; } = new();
    public List<int> Order { get; set; } = new();
    public List<int> Matches { get; set; } = new();
}
