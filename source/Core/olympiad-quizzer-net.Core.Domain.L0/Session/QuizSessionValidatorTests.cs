using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Session;
using OlympiadQuizzer.Core.Tests.Common.Builders;

namespace OlympiadQuizzer.Core.Domain.L0.Session;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class QuizSessionValidatorTests
{
    private static readonly DateTimeOffset _now = DateTimeOffset.UtcNow;

    [Fact]
    public void IsValid_WithNullState_ReturnsFalse()
    {
        bool result = QuizSessionValidator.IsValid(null, _now);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithUnknownSchemaVersion_ReturnsFalse()
    {
        QuizSessionState state = ValidState(1);
        state.SchemaVersion = 99;

        bool result = QuizSessionValidator.IsValid(state, _now);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithZeroQuestions_ReturnsFalse()
    {
        QuizSessionState state = ValidState(1);
        state.Questions = [];
        state.Answers   = [];

        bool result = QuizSessionValidator.IsValid(state, _now);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithMoreQuestionsThanMaxLimit_ReturnsFalse()
    {
        int count = QuestionQuery.MaxLimit + 1;
        List<Question> questions = [.. Enumerable.Range(1, count).Select(i => QuestionBuilder.AQuestion().WithId(i).Build())];
        List<SubmittedAnswer> answers = [.. Enumerable.Range(0, count).Select(_ => SubmittedAnswer.Empty)];

        QuizSessionState state = ValidState(1);
        state.Questions = questions;
        state.Answers   = answers;

        bool result = QuizSessionValidator.IsValid(state, _now);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithAnswerCountNotMatchingQuestionCount_ReturnsFalse()
    {
        QuizSessionState state = ValidState(2);
        state.Answers.Add(SubmittedAnswer.Empty);

        bool result = QuizSessionValidator.IsValid(state, _now);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithNegativeCurrentIndex_ReturnsFalse()
    {
        QuizSessionState state = ValidState(2);
        state.CurrentIndex = -1;

        bool result = QuizSessionValidator.IsValid(state, _now);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithCurrentIndexBeyondLastQuestion_ReturnsFalse()
    {
        QuizSessionState state = ValidState(2);
        state.CurrentIndex = 2;

        bool result = QuizSessionValidator.IsValid(state, _now);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithStartTimestampInTheFuture_ReturnsFalse()
    {
        QuizSessionState state = ValidState(1);
        state.StartedAtUtc = _now.AddMinutes(10);

        bool result = QuizSessionValidator.IsValid(state, _now);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithTamperedStartTimestampMovedBackwards_ReturnsTrueAndTimerReadsExpired()
    {
        // Start recorded as 2 hours ago. Timed session with 60-minute limit.
        // IsValid must accept this: "too far in past" is not a validity condition.
        // ExamTimer.Remaining must return zero, not negative.
        QuizSessionState state = ValidState(1);
        state.Timed            = true;
        state.TimeLimitMinutes = 60;
        state.StartedAtUtc     = _now.AddMinutes(-120);

        bool isValid    = QuizSessionValidator.IsValid(state, _now);
        TimeSpan timer  = ExamTimer.Remaining(state, _now);

        Assert.True(isValid);
        Assert.Equal(TimeSpan.Zero, timer);
    }

    [Fact]
    public void IsValid_WithQuestionOfUnknownType_ReturnsFalse()
    {
        QuizSessionState state = ValidState(2);
        state.Questions[0] = QuestionBuilder.AQuestion().WithType(QuestionType.Unknown).Build();

        bool result = QuizSessionValidator.IsValid(state, _now);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithNullElementInQuestionsList_ReturnsFalse()
    {
        QuizSessionState state = ValidState(1);
        state.Questions[0] = null;

        bool result = QuizSessionValidator.IsValid(state, _now);

        Assert.False(result);
    }

    [Fact]
    public void IsValid_WithAbsurdTimeLimit_ReturnsFalse()
    {
        QuizSessionState state = ValidState(1);
        state.Timed            = true;
        state.TimeLimitMinutes = 601;

        bool result = QuizSessionValidator.IsValid(state, _now);

        Assert.False(result);
    }

    private static QuizSessionState ValidState(int questionCount)
    {
        List<Question> questions = [.. Enumerable.Range(1, questionCount).Select(i => QuestionBuilder.AQuestion().WithId(i).Build())];
        List<SubmittedAnswer> answers = [.. Enumerable.Range(0, questionCount).Select(_ => SubmittedAnswer.Empty)];

        return new QuizSessionState
        {
            SchemaVersion    = 1,
            ModeId           = "OIJ-E1",
            Timed            = false,
            TimeLimitMinutes = 60,
            StartedAtUtc     = _now.AddMinutes(-5),
            CurrentIndex     = 0,
            Questions        = questions,
            Answers          = answers
        };
    }
}
