using System.Text.Json;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Tests.Common.Builders;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Serialization;

namespace OlympiadQuizzer.Core.Domain.L0.Serialization;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class QuestionSerializationTests
{
    [Fact]
    public void Serialize_DoesUseCamelCaseKeys_WhenQuestionHasPascalCaseProperties()
    {
        // Arrange
        Question question = QuestionBuilder.AFullyPopulatedQuestion().Build();

        // Act
        string json = JsonSerializer.Serialize(question, JsonOptions.Default);

        // Assert
        Assert.Contains("\"correctAnswer\"", json);
        Assert.Contains("\"sourceUrl\"", json);
        Assert.Contains("\"sourceRaw\"", json);
        Assert.Contains("\"explanationSource\"", json);
        Assert.Contains("\"matchOptions\"", json);
        Assert.Contains("\"contentCpp\"", json);
    }

    [Fact]
    public void Serialize_DoesNotEmitSnakeCaseKeys_WhenPropertyNamesArePascalCase()
    {
        // Arrange
        Question question = QuestionBuilder.AFullyPopulatedQuestion().Build();

        // Act
        string json = JsonSerializer.Serialize(question, JsonOptions.Default);

        // Assert
        Assert.DoesNotContain("\"source_raw\"", json);
        Assert.Contains("\"sourceRaw\"", json);
    }

    [Fact]
    public void Serialize_DoesNotEmitRemovedPocFields_WhenQuestionHasOnlyCurrentFields()
    {
        // Arrange
        Question question = QuestionBuilder.AFullyPopulatedQuestion().Build();

        // Act
        string json = JsonSerializer.Serialize(question, JsonOptions.Default);

        // Assert
        Assert.DoesNotContain("\"tags\"", json);
        Assert.DoesNotContain("\"sourceUrls\"", json);
        Assert.DoesNotContain("\"competition\"", json);
        Assert.DoesNotContain("\"voivodeship\"", json);
    }

    [Fact]
    public void Deserialize_ReturnsSingleType_WhenTypeIsSingle()
    {
        // Arrange
        const string typeName = "single";
        string json = BuildMinimalQuestionJson(typeName);

        // Act
        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        // Assert
        Assert.Equal(QuestionType.Single, question.Type);
    }

    [Fact]
    public void Deserialize_ReturnsShortAnswerType_WhenTypeIsShortAnswer()
    {
        // Arrange
        const string typeName = "shortAnswer";
        string json = BuildMinimalQuestionJson(typeName);

        // Act
        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        // Assert
        Assert.Equal(QuestionType.ShortAnswer, question.Type);
    }

    [Fact]
    public void Deserialize_ThrowsJsonException_WhenTypeIsSingleAbcd()
    {
        // Arrange
        const string legacyTypeName = "singleAbcd";
        string json = BuildMinimalQuestionJson(legacyTypeName);

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Question>(json, JsonOptions.Default));
    }

    [Fact]
    public void Deserialize_ThrowsJsonException_WhenTypeIsMultiSelect()
    {
        // Arrange
        const string legacyTypeName = "multiSelect";
        string json = BuildMinimalQuestionJson(legacyTypeName);

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Question>(json, JsonOptions.Default));
    }

    [Fact]
    public void Deserialize_ThrowsJsonException_WhenTypeIsOpen()
    {
        // Arrange
        const string legacyTypeName = "open";
        string json = BuildMinimalQuestionJson(legacyTypeName);

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Question>(json, JsonOptions.Default));
    }

    [Fact]
    public void Deserialize_ReturnsNullOptions_WhenOptionsAreNull()
    {
        // Arrange
        const string json = "{\"id\":1,\"type\":\"single\",\"content\":[],\"correctAnswer\":\"A\",\"options\":null}";

        // Act
        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        // Assert
        Assert.Null(question.Options);
    }

    [Fact]
    public void Deserialize_ReturnsNullableInt_WhenYearIsJsonNumber()
    {
        // Arrange
        const string json = "{\"id\":1,\"type\":\"single\",\"content\":[],\"correctAnswer\":\"A\",\"year\":2024}";
        const int expectedYear = 2024;

        // Act
        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        // Assert
        Assert.Equal(expectedYear, question.Year);
    }

    [Fact]
    public void Deserialize_ReturnsIntId_WhenIdIsJsonNumber()
    {
        // Arrange
        const string json = "{\"id\":42,\"type\":\"single\",\"content\":[],\"correctAnswer\":\"A\"}";
        const int expectedId = 42;

        // Act
        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        // Assert
        Assert.Equal(expectedId, question.Id);
    }

    [Fact]
    public void Deserialize_ThrowsJsonException_WhenTypeIsUnrecognised()
    {
        // Arrange
        const string unrecognisedTypeName = "neverSeenBefore";
        string json = BuildMinimalQuestionJson(unrecognisedTypeName);

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Question>(json, JsonOptions.Default));
    }

    [Fact]
    public void Deserialize_ThrowsJsonException_WhenTypeIsNull()
    {
        // Arrange
        const string json = "{\"id\":1,\"type\":null,\"content\":[],\"correctAnswer\":\"A\"}";

        // Act & Assert
        Assert.Throws<JsonException>(() =>
            JsonSerializer.Deserialize<Question>(json, JsonOptions.Default));
    }

    [Fact]
    public void Deserialize_ReturnsEmptyList_WhenCorrectAnswerIsNull()
    {
        // Arrange
        const string json = "{\"id\":1,\"type\":\"single\",\"content\":[],\"correctAnswer\":null}";

        // Act
        Question question = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        // Assert
        Assert.NotNull(question.CorrectAnswer);
        Assert.Empty(question.CorrectAnswer);
    }

    // Polish diacritics + subscript ₁₆ + superscript ² + mathematical italic x (U+1D465)
    private const string _unicodeRichText = "żółty ₁₆ 2² \U0001D465";

    [Fact]
    public void RoundTrip_DoesPreserveEveryCharacter_WhenTextHasPolishAndMathematicalUnicode()
    {
        // Arrange
        Question original = QuestionBuilder.AFullyPopulatedQuestion()
            .WithContent(new ContentBlock { Type = ContentBlockType.Text, Text = _unicodeRichText })
            .Build();

        // Act
        string json = JsonSerializer.Serialize(original, JsonOptions.Default);
        Question roundTripped = JsonSerializer.Deserialize<Question>(json, JsonOptions.Default);

        // Assert
        Assert.Equal(_unicodeRichText, roundTripped.Content[0].Text);
    }

    private static string BuildMinimalQuestionJson(string typeValue)
    {
        return $"{{\"id\":1,\"type\":\"{typeValue}\",\"content\":[],\"correctAnswer\":\"A\"}}";
    }
}
