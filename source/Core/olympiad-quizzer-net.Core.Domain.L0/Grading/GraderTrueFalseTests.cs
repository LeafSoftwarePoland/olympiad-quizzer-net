using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common.Builders;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GraderTrueFalseTests
{
    private const string _true  = "true";
    private const string _false = "false";

    private readonly GraderTrueFalse _sut = new();

    [Fact]
    public void Grade_ReturnsCorrect_WhenAllPositionsMatch()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.TrueFalse)
            .WithOptions(_true, _false, _true)
            .WithCorrectAnswer(_true, _false, _true)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_true, _false, _true);

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
        Assert.Equal(1.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_ReturnsZeroPoints_WhenOnePositionIsWrongAndNoPartialCredit()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.TrueFalse)
            .WithOptions(_true, _false, _true)
            .WithCorrectAnswer(_true, _false, _true)
            .WithPartialCredit(false)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_true, _true, _true);

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsZeroPoints_WhenAnswerLengthDoesNotMatchOptionsCount()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.TrueFalse)
            .WithOptions(_true, _false)
            .WithCorrectAnswer(_true, _false)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_true);

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsProportionalPoints_WhenOneOfThreePositionsMatchesAndPartialCredit()
    {
        // Arrange
        // 3 positions, 3 points, 1 correct: awarded = 3 * 1/3 = 1.0
        const int questionPoints = 3;
        const double expectedPointsAwarded = 1.0;
        const double expectedMaxPoints = 3.0;
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.TrueFalse)
            .WithOptions(_true, _false, _true)
            .WithCorrectAnswer(_true, _false, _true)
            .WithPoints(questionPoints)
            .WithPartialCredit(true)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_true, _true, _false);

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(expectedPointsAwarded, result.PointsAwarded, precision: 9);
        Assert.Equal(expectedMaxPoints, result.MaxPoints);
    }

    [Fact]
    public void Grade_ReturnsIncorrectAndDoesNotThrow_WhenSubmissionIsEmpty()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.TrueFalse)
            .WithOptions(_true, _false)
            .WithCorrectAnswer(_true, _false)
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
            .WithType(QuestionType.TrueFalse)
            .WithOptions(_true, _false)
            .WithCorrectAnswer(_true, _false)
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
        SubmittedAnswer answer = MakeAnswer(_true, _false);

        // Act
        GradeResult result = _sut.Grade(null, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
        Assert.Equal(0.0, result.MaxPoints);
    }

    private static SubmittedAnswer MakeAnswer(params string[] values)
    {
        return new SubmittedAnswer { Values = new List<string>(values) };
    }
}
