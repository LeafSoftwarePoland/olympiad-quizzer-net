using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common.Builders;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GraderSingleTests
{
    private const string _answerYGreaterEqualZ = "y >= z";
    // Precomposed U+017C LATIN SMALL LETTER Z WITH DOT ABOVE
    private const string _composedZWithDot = "ż";
    // Decomposed: z (U+007A) + combining dot above (U+0307)
    private const string _decomposedZWithDot = "ż";

    private readonly GraderSingle _sut = new();

    [Fact]
    public void Grade_ReturnsCorrectAndFullPoints_WhenSingleValueMatchesExactly()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_answerYGreaterEqualZ);

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
        Assert.Equal(1.0, result.PointsAwarded);
        Assert.Equal(1.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_ReturnsCorrect_WhenSingleValuesDifferOnlyInCasing()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer("Y >= Z");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsCorrect_WhenSingleValueHasSurroundingWhitespace()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer("  y >= z  ");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsIncorrectAndZeroPoints_WhenSingleValueIsWrong()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer("y > z");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsIncorrect_WhenSingleSubmissionHasTwoValues()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_answerYGreaterEqualZ, "x >= z");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsIncorrectAndZeroPoints_WhenSubmissionIsEmpty()
    {
        // Arrange
        const int questionPoints = 2;
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .WithPoints(questionPoints)
            .Build();

        // Act
        GradeResult result = _sut.Grade(question, SubmittedAnswer.Empty);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
        Assert.Equal(2.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_ReturnsIncorrectAndDoesNotThrow_WhenSubmissionIsNull()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();

        // Act
        GradeResult result = _sut.Grade(question, null);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsIncorrect_WhenCorrectAnswerHasTwoValues()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ, "x >= z")
            .Build();
        SubmittedAnswer answer = MakeAnswer(_answerYGreaterEqualZ);

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsCorrect_WhenSingleValueHasDecomposedAccent()
    {
        // Arrange
        // Stored answer uses precomposed U+017C; submitted uses decomposed z + U+0307.
        // NormalizeChoice applies FormC, so both sides compose to the same character.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_composedZWithDot)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_decomposedZWithDot);

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsIncorrectAndDoesNotThrow_WhenQuestionIsNull()
    {
        // Arrange
        SubmittedAnswer answer = MakeAnswer(_answerYGreaterEqualZ);

        // Act
        GradeResult result = _sut.Grade(null, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
        Assert.Equal(0.0, result.MaxPoints);
    }

    [Fact]
    public void QuestionType_ReturnsSingle_WhenGraderIsInstantiated()
        => Assert.Equal(QuestionType.Single, _sut.QuestionType);

    private static SubmittedAnswer MakeAnswer(params string[] values)
    {
        return new SubmittedAnswer { Values = new List<string>(values) };
    }
}
