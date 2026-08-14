using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.L0.Builders;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GraderScoringTests
{
    [Fact]
    public void Grade_CorrectAnswerWithPointsGreaterThanOne_ReturnsAllPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer("Option A")
            .WithPoints(5)
            .Build();
        SubmittedAnswer answer = MakeAnswer("Option A");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
        Assert.Equal(5.0, result.PointsAwarded);
        Assert.Equal(5.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_QuestionWithZeroPoints_ReturnsIsCorrectFalse()
    {
        // Unscored mode (points_per_question: null => Points = 0).
        // Even a fully correct submission must yield IsCorrect = false when max == 0.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer("Option A")
            .WithPoints(0)
            .Build();
        SubmittedAnswer answer = MakeAnswer("Option A");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_PartialCreditResultEqualToMaxPoints_ReturnsIsCorrectTrue()
    {
        // All positions correct with partial credit enabled -> awarded == max -> isCorrect = true
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Ordering)
            .WithOptions("a", "b", "c")
            .WithCorrectAnswer("a", "b", "c")
            .WithPoints(3)
            .WithPartialCredit(true)
            .Build();
        SubmittedAnswer answer = MakeAnswer("a", "b", "c");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
        Assert.Equal(3.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_PartialCreditProportion_DoesNotSufferFloatingPointDrift()
    {
        // points = 3, 1 of 3 positions correct -> awarded = 3 * 1 / 3 = 1.0
        // Verifying with epsilon rather than exact equality guards against FP drift.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Ordering)
            .WithOptions("b", "a", "c")
            .WithCorrectAnswer("b", "a", "c")
            .WithPoints(3)
            .WithPartialCredit(true)
            .Build();
        SubmittedAnswer answer = MakeAnswer("b", "c", "a");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(Math.Abs(result.PointsAwarded - 1.0) < 1e-9);
    }

    private static SubmittedAnswer MakeAnswer(params string[] values)
    {
        return new SubmittedAnswer { Values = new List<string>(values) };
    }
}
