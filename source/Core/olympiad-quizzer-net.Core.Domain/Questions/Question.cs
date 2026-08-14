using System.Text.Json.Serialization;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.Core.Domain.Questions;

public sealed class Question
{
    public int Id { get; set; }

    public List<string> Category   { get; set; } = [];
    public List<string> Algorithms { get; set; } = [];
    public string Olympiad   { get; set; }
    public string Stage      { get; set; }
    public int? Year         { get; set; }
    public int? Difficulty   { get; set; }

    public string Source            { get; set; }
    public string SourceUrl         { get; set; }
    public string SourceRaw         { get; set; }
    public string ExplanationSource { get; set; }

    public QuestionType Type          { get; set; }
    public List<ContentBlock> Content { get; set; } = [];
    public List<ContentBlock> ContentCpp { get; set; }
    public List<string> Options       { get; set; }
    public List<string> MatchOptions  { get; set; }
    public List<ContentBlock> Explanation { get; set; }

    [JsonConverter(typeof(StringOrStringArrayConverter))]
    public List<string> CorrectAnswer { get; set; } = [];

    public int Points { get; set; } = 1;
    public bool PartialCredit { get; set; }
}
