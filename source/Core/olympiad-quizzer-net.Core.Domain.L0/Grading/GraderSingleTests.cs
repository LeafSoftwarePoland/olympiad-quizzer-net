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
    private const string _composedZWithDot = "\u017c";
    // Decomposed: z (U+007A) + combining dot above (U+0307)
    private const string _decomposedZWithDot = "z\u0307";

    [Fact]
    public void Grade_SingleWithExactMatchingValue_ReturnsCorrectAndFullPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_answerYGreaterEqualZ);

        GradeResult result = GraderSingle.Grade(question, answer);

        Assert.True(result.IsCorrect);
        Assert.Equal(1.0, result.PointsAwarded);
        Assert.Equal(1.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_SingleWithDifferentCasing_ReturnsCorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer("Y >= Z");

        GradeResult result = GraderSingle.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_SingleWithSurroundingWhitespace_ReturnsCorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer("  y >= z  ");

        GradeResult result = GraderSingle.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_SingleWithWrongValue_ReturnsIncorrectAndZeroPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer("y > z");

        GradeResult result = GraderSingle.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_SingleWithTwoSubmittedValues_ReturnsIncorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_answerYGreaterEqualZ, "x >= z");

        GradeResult result = GraderSingle.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_SingleWithEmptySubmission_ReturnsIncorrectAndZeroPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .WithPoints(2)
            .Build();

        GradeResult result = GraderSingle.Grade(question, SubmittedAnswer.Empty);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
        Assert.Equal(2.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_SingleWithNullSubmission_ReturnsIncorrectAndDoesNotThrow()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ)
            .Build();

        GradeResult result = GraderSingle.Grade(question, null);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_SingleWhereCorrectAnswerHasTwoValues_ReturnsIncorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_answerYGreaterEqualZ, "x >= z")
            .Build();
        SubmittedAnswer answer = MakeAnswer(_answerYGreaterEqualZ);

        GradeResult result = GraderSingle.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_SingleWithDecomposedAccent_ReturnsCorrect()
    {
        // Stored answer uses precomposed U+017C; submitted uses decomposed z + U+0307.
        // NormalizeChoice applies FormC, so both sides compose to the same character.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(_composedZWithDot)
            .Build();
        SubmittedAnswer answer = MakeAnswer(_decomposedZWithDot);

        GradeResult result = GraderSingle.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_WithNullQuestion_ReturnsIncorrectAndDoesNotThrow()
    {
        SubmittedAnswer answer = MakeAnswer(_answerYGreaterEqualZ);

        GradeResult result = GraderSingle.Grade(null, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
        Assert.Equal(0.0, result.MaxPoints);
    }

    private static SubmittedAnswer MakeAnswer(params string[] values)
    {
        return new SubmittedAnswer { Values = new List<string>(values) };
    }
}
