using OlympiadQuizzer.Domain.Grading;
using OlympiadQuizzer.Domain.Questions;
using OlympiadQuizzer.Domain.L0.Builders;

namespace OlympiadQuizzer.Domain.L0.Grading;

[Trait("Tier", "L0")]
public sealed class GraderShortAnswerTests
{
    // U+2081 SUBSCRIPT ONE, U+2086 SUBSCRIPT SIX in stored answer; student types ASCII
    private const string StoredSubscriptSixteen = "AF\u2081\u2086";
    // U+00B2 SUPERSCRIPT TWO, U+2076 SUPERSCRIPT SIX in stored answer
    private const string StoredSuperscriptTwoSix = "2\u00b2\u2076";
    // U+1D465 MATHEMATICAL ITALIC SMALL X in stored answer
    private const string StoredMathItalicX = "\U0001D465";
    // U+00A0 NO-BREAK SPACE in stored answer; student types regular space
    private const string StoredNonBreakingSpace = "a\u00a0b";

    [Fact]
    public void Grade_ShortAnswerWithExactText_ReturnsCorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer("hello")
            .Build();
        SubmittedAnswer answer = MakeAnswer("hello");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ShortAnswerWithDifferentCasing_ReturnsCorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer("hello")
            .Build();
        SubmittedAnswer answer = MakeAnswer("HELLO");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ShortAnswerWithSubscriptDigits_ReturnsCorrect()
    {
        // Stored answer from scraped PDF uses subscript U+2081 and U+2086;
        // student types ASCII 1 and 6. NormalizeFreeText folds subscripts via FormKC.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer(StoredSubscriptSixteen)
            .Build();
        SubmittedAnswer answer = MakeAnswer("af16");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ShortAnswerWithSuperscriptDigits_ReturnsCorrect()
    {
        // Stored answer from scraped PDF uses U+00B2 and U+2076; student types ASCII.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer(StoredSuperscriptTwoSix)
            .Build();
        SubmittedAnswer answer = MakeAnswer("226");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ShortAnswerWithMathematicalItalicLetters_ReturnsCorrect()
    {
        // Stored answer contains U+1D465 mathematical italic x; student types ASCII x.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer(StoredMathItalicX)
            .Build();
        SubmittedAnswer answer = MakeAnswer("x");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ShortAnswerWithNonBreakingSpace_ReturnsCorrect()
    {
        // Stored answer contains U+00A0 between a and b; student types regular space.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer(StoredNonBreakingSpace)
            .Build();
        SubmittedAnswer answer = MakeAnswer("a b");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ShortAnswerWithCollapsibleInternalWhitespace_ReturnsCorrect()
    {
        // Stored answer has double space; student types single space.
        // NormalizeFreeText collapses both sides to single space.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer("a  b")
            .Build();
        SubmittedAnswer answer = MakeAnswer("a b");

        GradeResult result = Grader.Grade(question, answer);

        Assert.True(result.IsCorrect);
    }

    [Fact]
    public void Grade_ShortAnswerWithWrongText_ReturnsIncorrect()
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer("hello")
            .Build();
        SubmittedAnswer answer = MakeAnswer("world");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Fact]
    public void Grade_ShortAnswerWithEmptyText_ReturnsIncorrect()
    {
        // Submitting [""] is NOT IsEmpty (Count == 1); must travel the comparison path.
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer("hello")
            .Build();
        SubmittedAnswer answer = MakeAnswer("");

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
        Assert.Equal(0.0, result.PointsAwarded);
    }

    [Theory]
    [InlineData("5", "6")]
    [InlineData("kajak", "kajaki")]
    public void Grade_ShortAnswerDoesNotFoldDistinctCharacters_ReturnsIncorrect(string submitted, string correct)
    {
        Question question = QuestionBuilder.AQuestion()
            .WithType(QuestionType.ShortAnswer)
            .WithoutOptions()
            .WithCorrectAnswer(correct)
            .Build();
        SubmittedAnswer answer = MakeAnswer(submitted);

        GradeResult result = Grader.Grade(question, answer);

        Assert.False(result.IsCorrect);
    }

    private static SubmittedAnswer MakeAnswer(params string[] values)
    {
        return new SubmittedAnswer { Values = new List<string>(values) };
    }
}
