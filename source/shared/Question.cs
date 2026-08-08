using System.Text.Json;
using System.Text.Json.Serialization;

namespace OlympiadQuizzer.Shared;

public sealed class Question
{
    public string Id { get; set; } = "";
    public string Source { get; set; } = "other";
    public string Competition { get; set; } = "";
    public string? Voivodeship { get; set; }
    public int? Stage { get; set; }
    public string Year { get; set; } = "";
    public QuestionType Type { get; set; }
    public List<ContentBlock> Content { get; set; } = new();
    public List<ContentBlock>? ContentCpp { get; set; }

    public List<string>? Options { get; set; }

    public List<string>? MatchOptions { get; set; }

    public JsonElement CorrectAnswer { get; set; }

    public int Points { get; set; } = 1;
    public bool PartialCredit { get; set; }
    public List<string> Tags { get; set; } = new();
    public List<string> SourceUrls { get; set; } = new();
    public List<ContentBlock>? Explanation { get; set; }

    public int[]    CorrectIndices()  => Read<int[]>()    ?? Array.Empty<int>();
    public string[] CorrectStrings()  => Read<string[]>() ?? Array.Empty<string>();
    public bool[]   CorrectBooleans() => Read<bool[]>()   ?? Array.Empty<bool>();

    private T? Read<T>() =>
        CorrectAnswer.ValueKind == JsonValueKind.Undefined ? default
                                                           : CorrectAnswer.Deserialize<T>(JsonOptions.Default);
}
