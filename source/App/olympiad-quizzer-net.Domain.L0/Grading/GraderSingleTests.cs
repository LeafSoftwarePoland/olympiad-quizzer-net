using OlympiadQuizzer.Domain.Grading;
using OlympiadQuizzer.Domain.Questions;
using OlympiadQuizzer.Domain.L0.Builders;

namespace OlympiadQuizzer.Domain.L0.Grading;

[Trait("Tier", "L0")]
public sealed class GraderSingleTests
{
    private const string AnswerYGreaterEqualZ = "y >= z";
    // Precomposed U+017C LATIN SMALL LETTER Z WITH DOT ABOVE
    private const string ComposedZWithDot = "\u017c";
    // Decomposed: z (U+007A) + combining dot above (U+0307)
    private const string DecomposedZWithDot = "z\u0307";

    [Fact]
    public void Grade_SingleWithExactMatchingValue_ReturnsCorrectAndFullPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(AnswerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer(AnswerYGreaterEqualZ);

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
        Assert.Equal(1.0, result.PointsAwarded);
        Assert.Equal(1.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_SingleWithDifferentCasing_ReturnsCorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(AnswerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer("Y >= Z");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_SingleWithSurroundingWhitespace_ReturnsCorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(AnswerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer("  y >= z  ");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_SingleWithWrongValue_ReturnsIncorrectAndZeroPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(AnswerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer("y > z");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_SingleWithTwoSubmittedValues_ReturnsIncorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(AnswerYGreaterEqualZ)
            .Build();
        SubmittedAnswer answer = MakeAnswer(AnswerYGreaterEqualZ, "x >= z");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_SingleWithEmptySubmission_ReturnsIncorrectAndZeroPoints()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(AnswerYGreaterEqualZ)
            .WithPoints(2)
            .Build();

        GradeResult result = Grader.Grade(question, SubmittedAnswer.Empty);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
        Assert.Equal(2.0, result.MaxPoints);
    }

    [Fact]
    public void Grade_SingleWithNullSubmission_ReturnsIncorrectAndDoesNotThrow()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(AnswerYGreaterEqualZ)
            .Build();

        GradeResult result = Grader.Grade(question, null);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_SingleWhereCorrectAnswerHasTwoValues_ReturnsIncorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.Single)
            .WithCorrectAnswer(AnswerYGreaterEqualZ, "x >= z")
            .Build();
        SubmittedAnswer answer = MakeAnswer(AnswerYGreaterEqualZ);

        GradeResult result = Grader.Grade(question, answer);

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
            .WithCorrectAnswer(ComposedZWithDot)
            .Build();
        SubmittedAnswer answer = MakeAnswer(DecomposedZWithDot);

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_WithNullQuestion_ReturnsIncorrectAndDoesNotThrow()
    {
        SubmittedAnswer answer = MakeAnswer(AnswerYGreaterEqualZ);

        GradeResult result = Grader.Grade(null, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
        Assert.Equal(0.0, result.MaxPoints);
    }

    private static SubmittedAnswer MakeAnswer(params string[] values)
    {
        return new SubmittedAnswer { Values = new List<string>(values) };
    }
}
