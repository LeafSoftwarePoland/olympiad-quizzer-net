using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common.Builders;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GraderOrderingTests
{
    private readonly GraderOrdering _sut = new();

    [Fact]
    public void Grade_ReturnsCorrect_WhenAllPositionsMatchExpectedSequence()
    {
        // Arrange
        // Ordering answers are option texts in correct order, not indices
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Ordering)
            .WithOptions("b", "a", "c")
            .WithCorrectAnswer("b", "a", "c")
            .Build();
        SubmittedAnswer answer = MakeAnswer("b", "a", "c");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsZeroPoints_WhenSwappedPairAndNoPartialCredit()
    {
        // Arrange
        const int questionPoints = 3;
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Ordering)
            .WithOptions("b", "a", "c")
            .WithCorrectAnswer("b", "a", "c")
            .WithPoints(questionPoints)
            .WithPartialCredit(false)
            .Build();
        SubmittedAnswer answer = MakeAnswer("b", "c", "a");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsProportionalPoints_WhenSwappedPairAndPartialCredit()
    {
        // Arrange
        // 3 positions, 3 points, submitted ["b", "c", "a"]: index 0 matches, 1 and 2 do not
        // awarded = 3 * 1/3 = 1.0
        const int questionPoints = 3;
        const double expectedPointsAwarded = 1.0;
        const double expectedMaxPoints = 3.0;
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Ordering)
            .WithOptions("b", "a", "c")
            .WithCorrectAnswer("b", "a", "c")
            .WithPoints(questionPoints)
            .WithPartialCredit(true)
            .Build();
        SubmittedAnswer answer = MakeAnswer("b", "c", "a");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(expectedPointsAwarded, result.PointsAwarded, precision: 9);
        Assert.Equal(expectedMaxPoints, result.MaxPoints);
    }

    [Fact]
    public void Grade_ReturnsZeroPoints_WhenAnswerLengthDoesNotMatchExpected()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Ordering)
            .WithOptions("a", "b", "c")
            .WithCorrectAnswer("a", "b", "c")
            .Build();
        SubmittedAnswer answer = MakeAnswer("a", "b");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsIncorrectAndDoesNotThrow_WhenSubmissionIsEmpty()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Ordering)
            .WithOptions("a", "b", "c")
            .WithCorrectAnswer("a", "b", "c")
            .Build();

        // Act
        GradeResult result = _sut.Grade(question, SubmittedAnswer.Empty);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsIncorrectAndDoesNotThrow_WhenSubmissionIsNull()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Ordering)
            .WithOptions("a", "b", "c")
            .WithCorrectAnswer("a", "b", "c")
            .Build();

        // Act
        GradeResult result = _sut.Grade(question, null);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsIncorrectAndDoesNotThrow_WhenQuestionIsNull()
    {
        // Arrange
        SubmittedAnswer answer = MakeAnswer("a", "b", "c");

        // Act
        GradeResult result = _sut.Grade(null, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
        Assert.Equal(0.0, result.MaxPoints);
    }

    [Fact]
    public void QuestionType_ReturnsOrdering_WhenGraderIsInstantiated()
        => Assert.Equal(QuestionType.Ordering, _sut.QuestionType);

    private static SubmittedAnswer MakeAnswer(params string[] values)
    {
        return new SubmittedAnswer { Values = new List<string>(values) };
    }
}
