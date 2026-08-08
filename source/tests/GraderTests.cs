using System.Text;
using System.Text.Json;
using OlympiadQuizzer.Shared;

namespace OlympiadQuizzer.Tests;

public class GraderTests
{
    static Question Q(QuestionType type, object correct, string[]? options = null,
                      string[]? matchOptions = null, bool partial = false, int points = 1) => new()
    {
        Id = "t", Source = "other", Competition = "POC", Year = "2026", Type = type,
        Options = options?.ToList(), MatchOptions = matchOptions?.ToList(),
        CorrectAnswer = JsonSerializer.SerializeToElement(correct, JsonOptions.Default),
        Points = points, PartialCredit = partial
    };

    static AnswerSubmission MultiSub(params int[] idx) =>
        new() { SelectedIndices = idx.ToList() };

    static AnswerSubmission ShortSub(string? text) =>
        new() { Text = text };

    static AnswerSubmission TrueFalseSub(params bool?[] bools) =>
        new() { Booleans = bools.ToList() };

    static AnswerSubmission OrderSub(params int[] order) =>
        new() { Order = order.ToList() };

    static AnswerSubmission MatchSub(params int[] matches) =>
        new() { Matches = matches.ToList() };

    [Fact]
    public void MultiSelect_ExactSet_IsCorrect()
    {
        var q = Q(QuestionType.MultiSelect, new[] { 0, 1 }, options: ["A", "B", "C", "D"]);
        var r = Grader.Grade(q, MultiSub(0, 1));
        Assert.True(r.IsCorrect);
        Assert.Equal(1.0, r.PointsAwarded);
    }

    [Fact]
    public void MultiSelect_OrderIrrelevant()
    {
        var q = Q(QuestionType.MultiSelect, new[] { 0, 1 }, options: ["A", "B", "C", "D"]);
        var r = Grader.Grade(q, MultiSub(1, 0));
        Assert.True(r.IsCorrect);
    }

    [Fact]
    public void MultiSelect_Subset_IsIncorrect()
    {
        var q = Q(QuestionType.MultiSelect, new[] { 0, 1 }, options: ["A", "B", "C", "D"]);
        var r = Grader.Grade(q, MultiSub(0));
        Assert.False(r.IsCorrect);
        Assert.Equal(0.0, r.PointsAwarded);
    }

    [Fact]
    public void MultiSelect_Superset_IsIncorrect()
    {
        var q = Q(QuestionType.MultiSelect, new[] { 0, 1 }, options: ["A", "B", "C", "D"]);
        var r = Grader.Grade(q, MultiSub(0, 1, 2));
        Assert.False(r.IsCorrect);
    }

    [Fact]
    public void SingleAbcd_Correct()
    {
        var q = Q(QuestionType.SingleAbcd, new[] { 2 }, options: ["A", "B", "C", "D"]);
        var r = Grader.Grade(q, MultiSub(2));
        Assert.True(r.IsCorrect);
    }

    [Fact]
    public void SingleAbcd_Wrong()
    {
        var q = Q(QuestionType.SingleAbcd, new[] { 2 }, options: ["A", "B", "C", "D"]);
        var r = Grader.Grade(q, MultiSub(0));
        Assert.False(r.IsCorrect);
    }

    [Fact]
    public void SingleAbcd_Empty_IsIncorrect()
    {
        var q = Q(QuestionType.SingleAbcd, new[] { 2 }, options: ["A", "B", "C", "D"]);
        var r = Grader.Grade(q, MultiSub());
        Assert.False(r.IsCorrect);
    }

    [Fact]
    public void ShortAnswer_ExactMatch()
    {
        var q = Q(QuestionType.ShortAnswer, new[] { "kajak" });
        var r = Grader.Grade(q, ShortSub("kajak"));
        Assert.True(r.IsCorrect);
    }

    [Fact]
    public void ShortAnswer_CaseInsensitive()
    {
        var q = Q(QuestionType.ShortAnswer, new[] { "kajak" });
        Assert.True(Grader.Grade(q, ShortSub("KAJAK")).IsCorrect);
        Assert.True(Grader.Grade(q, ShortSub("Kajak")).IsCorrect);
    }

    [Fact]
    public void ShortAnswer_TrimsWhitespace()
    {
        var q = Q(QuestionType.ShortAnswer, new[] { "kajak" });
        var r = Grader.Grade(q, ShortSub("  kajak  "));
        Assert.True(r.IsCorrect);
    }

    [Fact]
    public void ShortAnswer_NfcNormalization()
    {
        // Derive decomposed at runtime so the file encoding cannot silently precompose both.
        var precomposed = "kód";         // k + precomposed o-acute (U+00F3) + d
        var decomposed  = precomposed.Normalize(NormalizationForm.FormD);  // k + o + U+0301 + d
        Assert.NotEqual(precomposed, decomposed);        // guard: they must differ
        Assert.Equal(Grader.Normalize(precomposed), Grader.Normalize(decomposed));

        var q = Q(QuestionType.ShortAnswer, new[] { precomposed });
        Assert.True(Grader.Grade(q, ShortSub(decomposed)).IsCorrect);
    }

    [Fact]
    public void ShortAnswer_AcceptsAnyListedForm()
    {
        var q = Q(QuestionType.ShortAnswer, new[] { "AF₁₆", "AF16" });
        var r = Grader.Grade(q, ShortSub("af16"));
        Assert.True(r.IsCorrect);
    }

    [Fact]
    public void ShortAnswer_Wrong()
    {
        var q = Q(QuestionType.ShortAnswer, new[] { "kajak" });
        var r = Grader.Grade(q, ShortSub("rower"));
        Assert.False(r.IsCorrect);
    }

    [Fact]
    public void ShortAnswer_NullOrEmpty_IsIncorrect()
    {
        var q = Q(QuestionType.ShortAnswer, new[] { "kajak" });
        Assert.False(Grader.Grade(q, ShortSub(null)).IsCorrect);
        Assert.False(Grader.Grade(q, ShortSub("")).IsCorrect);
    }

    [Fact]
    public void TrueFalse_AllCorrect()
    {
        var q = Q(QuestionType.TrueFalse, new[] { true, false, true },
            options: ["S1", "S2", "S3"]);
        var r = Grader.Grade(q, TrueFalseSub(true, false, true));
        Assert.True(r.IsCorrect);
        Assert.Equal(1.0, r.PointsAwarded);
    }

    [Fact]
    public void TrueFalse_OneWrong()
    {
        var q = Q(QuestionType.TrueFalse, new[] { true, false, true },
            options: ["S1", "S2", "S3"], partial: false);
        var r = Grader.Grade(q, TrueFalseSub(true, true, true));
        Assert.False(r.IsCorrect);
        Assert.Equal(0.0, r.PointsAwarded);
    }

    [Fact]
    public void TrueFalse_OneWrong_WithPartialCredit()
    {
        var q = Q(QuestionType.TrueFalse, new[] { true, false, true },
            options: ["S1", "S2", "S3"], partial: true);
        var r = Grader.Grade(q, TrueFalseSub(true, true, true));
        Assert.False(r.IsCorrect);
        Assert.Equal(2.0 / 3.0, r.PointsAwarded, 9);
    }

    [Fact]
    public void TrueFalse_Unanswered_IsIncorrect()
    {
        var q = Q(QuestionType.TrueFalse, new[] { true, false, true },
            options: ["S1", "S2", "S3"]);
        var r = Grader.Grade(q, TrueFalseSub(true, null, true));
        Assert.False(r.IsCorrect);
    }

    [Fact]
    public void Ordering_Correct()
    {
        var q = Q(QuestionType.Ordering, new[] { 1, 3, 0, 2 },
            options: ["C", "A", "D", "B"]);
        var r = Grader.Grade(q, OrderSub(1, 3, 0, 2));
        Assert.True(r.IsCorrect);
    }

    [Fact]
    public void Ordering_TwoSwapped()
    {
        var q = Q(QuestionType.Ordering, new[] { 1, 3, 0, 2 },
            options: ["C", "A", "D", "B"], partial: false);
        var r = Grader.Grade(q, OrderSub(3, 1, 0, 2));
        Assert.False(r.IsCorrect);
        Assert.Equal(0.0, r.PointsAwarded);
    }

    [Fact]
    public void Ordering_TwoSwapped_WithPartialCredit()
    {
        var q = Q(QuestionType.Ordering, new[] { 1, 3, 0, 2 },
            options: ["C", "A", "D", "B"], partial: true);
        var r = Grader.Grade(q, OrderSub(3, 1, 0, 2));
        Assert.Equal(2.0 / 4.0, r.PointsAwarded, 9);
    }

    [Fact]
    public void Ordering_WrongLength_IsIncorrect()
    {
        var q = Q(QuestionType.Ordering, new[] { 1, 3, 0, 2 },
            options: ["C", "A", "D", "B"]);
        var r = Grader.Grade(q, OrderSub(1, 3, 0));
        Assert.False(r.IsCorrect);
    }

    [Fact]
    public void Matching_Correct()
    {
        var q = Q(QuestionType.Matching, new[] { 2, 1, 0 },
            options: ["Kot", "Pies", "Ryba"],
            matchOptions: ["Woda", "Trawa", "Mleko"]);
        var r = Grader.Grade(q, MatchSub(2, 1, 0));
        Assert.True(r.IsCorrect);
    }

    [Fact]
    public void Matching_Unanswered_IsIncorrect()
    {
        var q = Q(QuestionType.Matching, new[] { 2, 1, 0 },
            options: ["Kot", "Pies", "Ryba"],
            matchOptions: ["Woda", "Trawa", "Mleko"]);
        var r = Grader.Grade(q, MatchSub(-1, 1, 0));
        Assert.False(r.IsCorrect);
    }

    [Fact]
    public void Matching_Partial_WithPartialCredit()
    {
        var q = Q(QuestionType.Matching, new[] { 2, 1, 0 },
            options: ["Kot", "Pies", "Ryba"],
            matchOptions: ["Woda", "Trawa", "Mleko"], partial: true);
        var r = Grader.Grade(q, MatchSub(2, 0, 2));
        Assert.Equal(1.0 / 3.0, r.PointsAwarded, 9);
    }

    [Fact]
    public void UnknownType_ReturnsIncorrect_DoesNotThrow()
    {
        var q = Q(QuestionType.Unknown, 0);
        var r = Grader.Grade(q, new AnswerSubmission());
        Assert.False(r.IsCorrect);
        Assert.Equal(0.0, r.PointsAwarded);
        Assert.Equal(1.0, r.MaxPoints);
    }

    [Fact]
    public void Grade_NullSubmission_IsIncorrect()
    {
        var q = Q(QuestionType.MultiSelect, new[] { 0 });
        var r = Grader.Grade(q, null);
        Assert.False(r.IsCorrect);
        Assert.Equal(0.0, r.PointsAwarded);
    }

    [Fact]
    public void MalformedQuestion_EmptyCorrectAnswer_IsIncorrect()
    {
        var q = Q(QuestionType.Ordering, Array.Empty<int>(), options: ["A", "B"]);
        var r = Grader.Grade(q, OrderSub());
        Assert.False(r.IsCorrect);
        Assert.Equal(0.0, r.PointsAwarded);
    }
}
