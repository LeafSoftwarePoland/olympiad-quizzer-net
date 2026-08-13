using System.Text.Json;
using OlympiadQuizzer.Domain.Questions;
using OlympiadQuizzer.Domain.Serialization;

namespace OlympiadQuizzer.Domain.L0.Serialization;

[Trait("Tier", "L0")]
public sealed class JsonOptionsTests
{
    // ż is U+017C (z with dot above) — Polish diacritic that UnsafeRelaxedJsonEscaping must not escape
    private const string PolishChar = "ż";
    private const string EscapedPolishChar = "\\u017c";

    [Fact]
    public void Serialize_TextWithPolishDiacritics_EmitsLiteralCharactersNotUnicodeEscapes()
    {
        string json = JsonSerializer.Serialize(PolishChar, JsonOptions.Default);

        Assert.Contains(PolishChar, json);
        Assert.DoesNotContain(EscapedPolishChar, json);
    }

    [Fact]
    public void Deserialize_JsonWithTrailingComma_Succeeds()
    {
        string json = "{\"value\":\"x\",}";

        JsonElement element = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions.Default);

        Assert.Equal("x", element.GetProperty("value").GetString());
    }

    [Fact]
    public void Deserialize_JsonWithComment_Succeeds()
    {
        string json = "{/* comment */ \"value\":\"x\"}";

        JsonElement element = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions.Default);

        Assert.Equal("x", element.GetProperty("value").GetString());
    }

    [Fact]
    public void Deserialize_JsonWithPascalCaseKeys_Succeeds()
    {
        // PropertyNameCaseInsensitive must be verified against a POCO — JsonElement preserves the
        // original key verbatim and ignores the option entirely.
        string json = "{\"Id\":7,\"Type\":\"single\",\"CorrectAnswer\":\"a\"}";

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Equal(7, question.Id);
    }
}
