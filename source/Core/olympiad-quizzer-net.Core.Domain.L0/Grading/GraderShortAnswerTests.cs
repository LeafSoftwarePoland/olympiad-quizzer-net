using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common.Builders;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GraderShortAnswerTests
{
    // U+2081 SUBSCRIPT ONE, U+2086 SUBSCRIPT SIX in stored answer; student types ASCII
    private const string _storedSubscriptSixteen = "AF₁₆";
    // U+00B2 SUPERSCRIPT TWO, U+2076 SUPERSCRIPT SIX in stored answer
    private const string _storedSuperscriptTwoSix = "2²⁶";
    // U+1D465 MATHEMATICAL ITALIC SMALL X in stored answer
    private const string _storedMathItalicX = "\U0001D465";
    // U+00A0 NO-BREAK SPACE in stored answer; student types regular space
    private const string _storedNonBreakingSpace = "a b";

    private readonly GraderShortAnswer _sut = new();

    [Fact]
    public void Grade_ReturnsCorrect_WhenShortAnswerTextMatchesExactly()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer("hello")
            .Build();
        SubmittedAnswer answer = MakeAnswer("hello");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsCorrect_WhenShortAnswerTextDiffersInCasingOnly()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer("hello")
            .Build();
        SubmittedAnswer answer = MakeAnswer("HELLO");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsCorrect_WhenShortAnswerHasSubscriptDigits()
    {
        // Arrange
        // Stored answer from scraped PDF uses subscript U+2081 and U+2086;
        // student types ASCII 1 and 6. NormalizeFreeText folds subscripts via FormKC.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer(_storedSubscriptSixteen)
            .Build();
        SubmittedAnswer answer = MakeAnswer("af16");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsCorrect_WhenShortAnswerHasSuperscriptDigits()
    {
        // Arrange
        // Stored answer from scraped PDF uses U+00B2 and U+2076; student types ASCII.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer(_storedSuperscriptTwoSix)
            .Build();
        SubmittedAnswer answer = MakeAnswer("226");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsCorrect_WhenShortAnswerHasMathematicalItalicLetters()
    {
        // Arrange
        // Stored answer contains U+1D465 mathematical italic x; student types ASCII x.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer(_storedMathItalicX)
            .Build();
        SubmittedAnswer answer = MakeAnswer("x");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsCorrect_WhenShortAnswerHasNonBreakingSpace()
    {
        // Arrange
        // Stored answer contains U+00A0 between a and b; student types regular space.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer(_storedNonBreakingSpace)
            .Build();
        SubmittedAnswer answer = MakeAnswer("a b");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsCorrect_WhenShortAnswerHasCollapsibleInternalWhitespace()
    {
        // Arrange
        // Stored answer has double space; student types single space.
        // NormalizeFreeText collapses both sides to single space.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer("a  b")
            .Build();
        SubmittedAnswer answer = MakeAnswer("a b");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsIncorrect_WhenShortAnswerTextIsWrong()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer("hello")
            .Build();
        SubmittedAnswer answer = MakeAnswer("world");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsIncorrect_WhenShortAnswerTextIsEmpty()
    {
        // Arrange
        // Submitting [""] is NOT IsEmpty (Count == 1); must travel the comparison path.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer("hello")
            .Build();
        SubmittedAnswer answer = MakeAnswer("");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Theory]
    [InlineData("5", "6")]
    [InlineData("kajak", "kajaki")]
    public void Grade_ReturnsIncorrect_WhenShortAnswerHasNonFoldableCharacterDifference(string submitted, string correct)
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer(correct)
            .Build();
        SubmittedAnswer answer = MakeAnswer(submitted);

        GradeResult result = _sut.Grade(question, answer);

        Assert.False(result.IsCorrect);
    }

    private static SubmittedAnswer MakeAnswer(params string[] values)
    {
        return new SubmittedAnswer { Values = new List<string>(values) };
    }
}
