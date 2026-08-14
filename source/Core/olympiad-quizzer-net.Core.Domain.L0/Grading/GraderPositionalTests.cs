using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.L0.Builders;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GraderPositionalTests
{
    private const string _true = "true";
    private const string _false = "false";

    [Fact]
    public void Grade_TrueFalseWithAllPositionsMatching_ReturnsCorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.TrueFalse)
            .WithOptions(_true, _false, _true)
            .WithCorrectAnswer(_true, _false, _true)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_true, _false, _true);

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
        Assert.Equal(1.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_TrueFalseWithOnePositionWrongAndNoPartialCredit_ReturnsZeroPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.TrueFalse)
            .WithOptions(_true, _false, _true)
            .WithCorrectAnswer(_true, _false, _true)
            .WithPartialCredit(false)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_true, _true, _true);

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_TrueFalseWithWrongLength_ReturnsZeroPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.TrueFalse)
            .WithOptions(_true, _false)
            .WithCorrectAnswer(_true, _false)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_true);

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_OrderingWithCorrectSequence_ReturnsCorrect()
    {
        // Ordering answers are option texts in the correct order, not indices
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Ordering)
            .WithOptions("b", "a", "c")
            .WithCorrectAnswer("b", "a", "c")
            .Build();
        SubmittedAnswer answer = MakeAnswer("b", "a", "c");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_OrderingWithSwappedPairAndPartialCredit_ReturnsProportionalPoints()
    {
        // 3 positions, points = 3, partial credit = true
        // Submitted ["b", "c", "a"]: index 0 matches (b=b), indices 1 and 2 do not
        // awarded = 3 * 1 / 3 = 1.0
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Ordering)
            .WithOptions("b", "a", "c")
            .WithCorrectAnswer("b", "a", "c")
            .WithPoints(3)
            .WithPartialCredit(true)
            .Build();
        SubmittedAnswer answer = MakeAnswer("b", "c", "a");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(1.0, result.PointsAwarded, precision: 9);
        Assert.Equal(3.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_OrderingWithSwappedPairAndNoPartialCredit_ReturnsZeroPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Ordering)
            .WithOptions("b", "a", "c")
            .WithCorrectAnswer("b", "a", "c")
            .WithPoints(3)
            .WithPartialCredit(false)
            .Build();
        SubmittedAnswer answer = MakeAnswer("b", "c", "a");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_MatchingWithAllPairsCorrect_ReturnsCorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Matching)
            .WithMatchOptions("Match A", "Match B")
            .WithCorrectAnswer("Match A", "Match B")
            .Build();
        SubmittedAnswer answer = MakeAnswer("Match A", "Match B");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_MatchingWithOnePairWrongAndPartialCredit_ReturnsProportionalPoints()
    {
        // 2 pairs, points = 2, partial credit = true
        // 1 pair correct, 1 wrong -> awarded = 2 * 1 / 2 = 1.0
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Matching)
            .WithMatchOptions("Match A", "Match B")
            .WithCorrectAnswer("Match A", "Match B")
            .WithPoints(2)
            .WithPartialCredit(true)
            .Build();
        SubmittedAnswer answer = MakeAnswer("Match A", "Match C");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(1.0, result.PointsAwarded, precision: 9);
        Assert.Equal(2.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_UnknownQuestionType_ReturnsIncorrectAndZeroPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Unknown)
            .Build();
        SubmittedAnswer answer = MakeAnswer("any");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    private static SubmittedAnswer MakeAnswer(params string[] values)
    {
        return new SubmittedAnswer { Values = new List<string>(values) };
    }
}
