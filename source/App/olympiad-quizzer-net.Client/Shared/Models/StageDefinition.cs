namespace OlympiadQuizzer.Client.Shared.Models;

public sealed class StageDefinition
{
    public string StageId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int? QuestionCount { get; set; }
    public int? TimeLimitMinutes { get; set; }
    public List<string> AllowedQuestionTypes { get; set; } = new();
    public bool AnswerStrictCasing { get; set; }
    public bool StrictCasing { get; set; }
    public bool PartialPoints { get; set; }
    public int? PassingThreshold { get; set; }
    public int? PointsPerQuestion { get; set; }
}
