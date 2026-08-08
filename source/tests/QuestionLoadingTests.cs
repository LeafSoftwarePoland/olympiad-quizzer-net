using System.Text.Json;
using OlympiadQuizzer.Shared;

namespace OlympiadQuizzer.Tests;

public class QuestionLoadingTests
{
    static List<Question> Load()
    {
        var path = Path.Combine(AppContext.BaseDirectory, "Data", "questions.json");
        return JsonSerializer.Deserialize<List<Question>>(File.ReadAllText(path), JsonOptions.Default)!;
    }

    [Fact]
    public void QuestionsJson_Deserializes()
    {
        var questions = Load();
        Assert.Equal(6, questions.Count);
    }

    [Fact]
    public void QuestionsJson_AllTypesPresent()
    {
        var questions = Load();
        var types = questions.Select(q => q.Type).ToHashSet();
        Assert.Contains(QuestionType.MultiSelect, types);
        Assert.Contains(QuestionType.SingleAbcd,  types);
        Assert.Contains(QuestionType.ShortAnswer,  types);
        Assert.Contains(QuestionType.TrueFalse,    types);
        Assert.Contains(QuestionType.Ordering,     types);
        Assert.Contains(QuestionType.Matching,     types);
        Assert.DoesNotContain(QuestionType.Unknown, types);
    }

    [Fact]
    public void QuestionsJson_IdsUnique()
    {
        var questions = Load();
        Assert.Equal(6, questions.Select(q => q.Id).Distinct().Count());
    }

    [Fact]
    public void QuestionsJson_MatchesAdr022Bindings()
    {
        var questions = Load();
        foreach (var q in questions)
        {
            if (q.Type == QuestionType.ShortAnswer)
                Assert.Null(q.Options);
            else
                Assert.NotNull(q.Options);

            if (q.Type == QuestionType.Matching)
                Assert.NotNull(q.MatchOptions);
            else
                Assert.Null(q.MatchOptions);
        }
    }

    [Fact]
    public void QuestionsJson_CorrectAnswerShapesValid()
    {
        var questions = Load();
        foreach (var q in questions)
        {
            switch (q.Type)
            {
                case QuestionType.TrueFalse:
                    var bools = q.CorrectBooleans();
                    Assert.Equal(q.Options!.Count, bools.Length);
                    break;
                case QuestionType.Ordering:
                    var ordering = q.CorrectIndices();
                    Assert.Equal(q.Options!.Count, ordering.Length);
                    Assert.Equal(ordering.Length, ordering.Distinct().Count());
                    Assert.True(ordering.All(i => i >= 0 && i < ordering.Length));
                    break;
                case QuestionType.Matching:
                    var matching = q.CorrectIndices();
                    Assert.Equal(q.Options!.Count, matching.Length);
                    Assert.True(matching.All(i => i >= 0 && i < q.MatchOptions!.Count));
                    break;
                case QuestionType.MultiSelect:
                case QuestionType.SingleAbcd:
                    var indices = q.CorrectIndices();
                    Assert.True(indices.Length > 0);
                    break;
                case QuestionType.ShortAnswer:
                    var strings = q.CorrectStrings();
                    Assert.True(strings.Length > 0);
                    break;
            }
        }
    }

    [Fact]
    public void QuestionsJson_FixturesMatchAdr022WorkedExamples()
    {
        var questions = Load();
        var poc5 = questions.Single(q => q.Id == "poc-5");
        var poc6 = questions.Single(q => q.Id == "poc-6");

        Assert.Equal(new[] { 1, 3, 0, 2 }, poc5.CorrectIndices());
        Assert.Equal(new[] { 2, 1, 0 },    poc6.CorrectIndices());
    }

    [Fact]
    public void Serialization_UsesCamelCase()
    {
        var q = new Question
        {
            Id = "x", Source = "other", Competition = "POC", Year = "2026",
            Type = QuestionType.MultiSelect,
            CorrectAnswer = JsonSerializer.SerializeToElement(new[] { 0 }, JsonOptions.Default),
            MatchOptions = new List<string> { "x" }
        };
        var json = JsonSerializer.Serialize(q, JsonOptions.Default);
        Assert.Contains("\"correctAnswer\"", json);
        Assert.Contains("\"matchOptions\"", json);
        Assert.DoesNotContain("\"CorrectAnswer\"", json);
        Assert.DoesNotContain("\"correct_answer\"", json);
    }
}
