using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common.Builders;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GraderMatchingTests
{
    private readonly GraderMatching _sut = new();

    [Fact]
    public void Grade_ReturnsCorrect_WhenAllPairsMatchCorrectly()
    {
        // Arrange
        // For matching, correctAnswer holds matchOptions values in the expected order
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Matching)
            .WithMatchOptions("Match A", "Match B")
            .WithCorrectAnswer("Match A", "Match B")
            .Build();
        SubmittedAnswer answer = MakeAnswer("Match A", "Match B");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsZeroPoints_WhenAllPairsAreWrongAndNoPartialCredit()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Matching)
            .WithMatchOptions("Match A", "Match B")
            .WithCorrectAnswer("Match A", "Match B")
            .WithPartialCredit(false)
            .Build();
        SubmittedAnswer answer = MakeAnswer("Match B", "Match A");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsProportionalPoints_WhenOnePairIsWrongAndPartialCredit()
    {
        // Arrange
        // 2 pairs, 2 points, 1 pair correct: awarded = 2 * 1/2 = 1.0
        const int questionPoints = 2;
        const double expectedPointsAwarded = 1.0;
        const double expectedMaxPoints = 2.0;
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Matching)
            .WithMatchOptions("Match A", "Match B")
            .WithCorrectAnswer("Match A", "Match B")
            .WithPoints(questionPoints)
            .WithPartialCredit(true)
            .Build();
        SubmittedAnswer answer = MakeAnswer("Match A", "Match C");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(expectedPointsAwarded, result.PointsAwarded, precision: 9);
        Assert.Equal(expectedMaxPoints, result.MaxPoints);
    }

    [Fact]
    public void Grade_ReturnsZeroPoints_WhenAnswerCountDoesNotMatchPairCount()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Matching)
            .WithMatchOptions("Match A", "Match B", "Match C")
            .WithCorrectAnswer("Match A", "Match B", "Match C")
            .Build();
        SubmittedAnswer answer = MakeAnswer("Match A", "Match B");

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
            .WithType(QuestionType.Matching)
            .WithMatchOptions("Match A", "Match B")
            .WithCorrectAnswer("Match A", "Match B")
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
            .WithType(QuestionType.Matching)
            .WithMatchOptions("Match A", "Match B")
            .WithCorrectAnswer("Match A", "Match B")
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
        SubmittedAnswer answer = MakeAnswer("Match A", "Match B");

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
