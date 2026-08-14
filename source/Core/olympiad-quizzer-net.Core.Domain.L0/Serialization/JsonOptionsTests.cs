using System.Text.Json;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.Core.Domain.L0.Serialization;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class JsonOptionsTests
{
    // ż is U+017C (z with dot above) — Polish diacritic that UnsafeRelaxedJsonEscaping must not escape
    private const string _polishChar = "ż";
    private const string _escapedPolishChar = "\\u017c";

    [Fact]
    public void Serialize_TextWithPolishDiacritics_DoesEmitLiteralCharacters()
    {
        string json = JsonSerializer.Serialize(_polishChar, JsonOptions.Default);

        Assert.Contains(_polishChar, json);
        Assert.DoesNotContain(_escapedPolishChar, json);
    }

    [Fact]
    public void Deserialize_JsonWithTrailingComma_DoesDeserializeSuccessfully()
    {
        string json = "{\"value\":\"x\",}";

        JsonElement element = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions.Default);

        Assert.Equal("x", element.GetProperty("value").GetString());
    }

    [Fact]
    public void Deserialize_JsonWithComment_DoesDeserializeSuccessfully()
    {
        string json = "{/* comment */ \"value\":\"x\"}";

        JsonElement element = JsonSerializer.Deserialize<JsonElement>(json, JsonOptions.Default);

        Assert.Equal("x", element.GetProperty("value").GetString());
    }

    [Fact]
    public void Deserialize_JsonWithPascalCaseKeys_DoesBindProperties()
    {
        // PropertyNameCaseInsensitive must be verified against a POCO — JsonElement preserves the
        // original key verbatim and ignores the option entirely.
        string json = "{\"Id\":7,\"Type\":\"single\",\"CorrectAnswer\":\"a\"}";

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Equal(7, question.Id);
    }
}
