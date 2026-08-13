using System.Text.Json;
using OlympiadQuizzer.Domain.L0.Builders;
using OlympiadQuizzer.Domain.Questions;
using OlympiadQuizzer.Domain.Serialization;

namespace OlympiadQuizzer.Domain.L0.Serialization;

[Trait("Tier", "L0")]
public sealed class QuestionSerializationTests
{
    [Fact]
    public void Serialize_Question_UsesCamelCaseKeys()
    {
        Question question = QuestionBuilder.AFullyPopulatedQuestion().Build();

        string json = JsonSerializer.Serialize(question, JsonOptions.Default);

        Assert.Contains("\"correctAnswer\"", json);
        Assert.Contains("\"sourceUrl\"", json);
        Assert.Contains("\"sourceRaw\"", json);
        Assert.Contains("\"explanationSource\"", json);
        Assert.Contains("\"matchOptions\"", json);
        Assert.Contains("\"contentCpp\"", json);
    }

    [Fact]
    public void Serialize_Question_DoesNotEmitSnakeCaseSourceRaw()
    {
        Question question = QuestionBuilder.AFullyPopulatedQuestion().Build();

        string json = JsonSerializer.Serialize(question, JsonOptions.Default);

        Assert.DoesNotContain("\"source_raw\"", json);
        Assert.Contains("\"sourceRaw\"", json);
    }

    [Fact]
    public void Serialize_Question_DoesNotEmitRemovedPocFields()
    {
        Question question = QuestionBuilder.AFullyPopulatedQuestion().Build();

        string json = JsonSerializer.Serialize(question, JsonOptions.Default);

        Assert.DoesNotContain("\"tags\"", json);
        Assert.DoesNotContain("\"sourceUrls\"", json);
        Assert.DoesNotContain("\"competition\"", json);
        Assert.DoesNotContain("\"voivodeship\"", json);
    }

    [Fact]
    public void Deserialize_QuestionTypeSingle_MapsFromCamelCaseWireValue()
    {
        string json = BuildMinimalQuestionJson("single");

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Equal(QuestionType.Single, question.Type);
    }

    [Fact]
    public void Deserialize_QuestionTypeShortAnswer_MapsFromCamelCaseWireValue()
    {
        string json = BuildMinimalQuestionJson("shortAnswer");

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Equal(QuestionType.ShortAnswer, question.Type);
    }

    [Fact]
    public void Deserialize_LegacyTypeNameSingleAbcd_MapsToUnknown()
    {
        string json = BuildMinimalQuestionJson("singleAbcd");

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Equal(QuestionType.Unknown, question.Type);
    }

    [Fact]
    public void Deserialize_LegacyTypeNameMultiSelect_MapsToUnknown()
    {
        string json = BuildMinimalQuestionJson("multiSelect");

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Equal(QuestionType.Unknown, question.Type);
    }

    [Fact]
    public void Deserialize_ScraperTypeOpen_MapsToUnknown()
    {
        string json = BuildMinimalQuestionJson("open");

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Equal(QuestionType.Unknown, question.Type);
    }

    [Fact]
    public void Deserialize_QuestionWithNullOptions_LeavesOptionsNull()
    {
        string json = "{\"id\":1,\"type\":\"single\",\"content\":[],\"correctAnswer\":\"A\",\"options\":null}";

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Null(question.Options);
    }

    [Fact]
    public void Deserialize_YearAsJsonNumber_BindsToNullableInt()
    {
        string json = "{\"id\":1,\"type\":\"single\",\"content\":[],\"correctAnswer\":\"A\",\"year\":2024}";

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Equal(2024, question.Year);
    }

    [Fact]
    public void Deserialize_IdAsJsonNumber_BindsToInt()
    {
        string json = "{\"id\":42,\"type\":\"single\",\"content\":[],\"correctAnswer\":\"A\"}";

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Equal(42, question.Id);
    }

    // Polish diacritics + subscript ₁₆ + superscript ² + mathematical italic x (U+1D465)
    private const string UnicodeRichText = "żółty ₁₆ 2² \U0001D465";

    [Fact]
    public void RoundTrip_QuestionWithPolishAndMathematicalUnicode_PreservesEveryCharacter()
    {
        Question original = QuestionBuilder.AFullyPopulatedQuestion()
            .WithContent(new ContentBlock { Type = ContentBlockType.Text, Text = UnicodeRichText })
            .Build();

        string json = JsonSerializer.Serialize(original, JsonOptions.Default);
        Question roundTripped = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Equal(UnicodeRichText, roundTripped.Content[0].Text);
    }

    [Fact]
    public void Deserialize_UnrecognisedTypeName_MapsToUnknownWithoutThrowing()
    {
        string json = BuildMinimalQuestionJson("neverSeenBefore");

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.Equal(QuestionType.Unknown, question.Type);
    }

    [Fact]
    public void Deserialize_QuestionWithNullCorrectAnswer_YieldsEmptyList()
    {
        string json = "{\"id\":1,\"type\":\"single\",\"content\":[],\"correctAnswer\":null}";

        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        Assert.NotNull(question.CorrectAnswer);
        Assert.Empty(question.CorrectAnswer);
    }

    private static string BuildMinimalQuestionJson(string typeValue)
    {
        return $"{{\"id\":1,\"type\":\"{typeValue}\",\"content\":[],\"correctAnswer\":\"A\"}}";
    }
}
