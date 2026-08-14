using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.L0.Builders;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GraderMultiTests
{
    [Fact]
    public void Grade_MultiWithAllExpectedValues_ReturnsCorrectAndFullPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .Build();
        SubmittedAnswer answer = MakeAnswer("A", "C");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
        Assert.Equal(1.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_MultiWithExpectedValuesInDifferentOrder_ReturnsCorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .Build();
        SubmittedAnswer answer = MakeAnswer("C", "A");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_MultiWithDuplicateSubmittedValues_ReturnsCorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .Build();
        SubmittedAnswer answer = MakeAnswer("A", "A", "C");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_MultiWithSubsetOfExpectedValues_ReturnsIncorrectAndZeroPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .Build();
        SubmittedAnswer answer = MakeAnswer("A");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_MultiWithExtraValue_ReturnsIncorrectAndZeroPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .Build();
        SubmittedAnswer answer = MakeAnswer("A", "B", "C");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_MultiWithPartialCreditEnabled_StillReturnsAllOrNothing()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Multi)
            .WithOptions("A", "B", "C")
            .WithCorrectAnswer("A", "C")
            .WithPoints(4)
            .WithPartialCredit(true)
            .Build();
        SubmittedAnswer answer = MakeAnswer("A");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
        Assert.Equal(4.0, result.MaxPoints);
    }

    private static SubmittedAnswer MakeAnswer(params string[] values)
    {
        return new SubmittedAnswer { Values = new List<string>(values) };
    }
}
