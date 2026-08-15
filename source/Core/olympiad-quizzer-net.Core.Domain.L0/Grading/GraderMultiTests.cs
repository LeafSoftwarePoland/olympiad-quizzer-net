using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common.Builders;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GraderMultiTests
{
    private readonly GraderMulti _sut = new();

    [Fact]
    public void Grade_ReturnsCorrectAndFullPoints_WhenMultiCarriesAllExpectedValues()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .Build();
        SubmittedAnswer answer = MakeAnswer("A", "C");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
        Assert.Equal(1.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsCorrect_WhenMultiValuesAreInDifferentOrder()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .Build();
        SubmittedAnswer answer = MakeAnswer("C", "A");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsCorrect_WhenMultiSubmissionHasDuplicates()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .Build();
        SubmittedAnswer answer = MakeAnswer("A", "A", "C");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ReturnsIncorrectAndZeroPoints_WhenMultiSubmissionIsSubset()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .Build();
        SubmittedAnswer answer = MakeAnswer("A");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsIncorrectAndZeroPoints_WhenMultiSubmissionHasExtraValue()
    {
        // Arrange
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .Build();
        SubmittedAnswer answer = MakeAnswer("A", "B", "C");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ReturnsAllOrNothing_WhenMultiHasPartialCreditEnabled()
    {
        // Arrange
        const int questionPoints = 4;
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .WithPoints(questionPoints)
            .WithPartialCredit(true)
            .Build();
        SubmittedAnswer answer = MakeAnswer("A");

        // Act
        GradeResult result = _sut.Grade(question, answer);

        // Assert
        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
        Assert.Equal(4.0, result.MaxPoints);
    }

    [Fact]
    public void QuestionType_ReturnsMulti_WhenGraderIsInstantiated()
        => Assert.Equal(QuestionType.Multi, _sut.QuestionType);

    private static SubmittedAnswer MakeAnswer(params string[] values)
    {
        return new SubmittedAnswer { Values = new List<string>(values) };
    }
}
