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
    public void IsValid_ReturnsFalse_WhenStateIsNull()
    {
        // Act
        bool result = QuizSessionValidator.IsValid(null, _now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenSchemaVersionIsUnrecognised()
    {
        // Arrange
        const int unrecognisedSchemaVersion = 99;
        QuizSessionState state = ValidState(1);
        state.SchemaVersion = unrecognisedSchemaVersion;

        // Act
        bool result = QuizSessionValidator.IsValid(state, _now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenQuestionListIsEmpty()
    {
        // Arrange
        QuizSessionState state = ValidState(1);
        state.Questions = [];
        state.Answers   = [];

        // Act
        bool result = QuizSessionValidator.IsValid(state, _now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenQuestionCountExceedsMaxLimit()
    {
        // Arrange
        int count = QuestionQuery.MaxLimit + 1;
        List<Question> questions = [.. Enumerable.Range(1, count).Select(i => QuestionBuilder.AQuestion().WithId(i).Build())];
        List<SubmittedAnswer> answers = [.. Enumerable.Range(0, count).Select(_ => SubmittedAnswer.Empty)];

        QuizSessionState state = ValidState(1);
        state.Questions = questions;
        state.Answers   = answers;

        // Act
        bool result = QuizSessionValidator.IsValid(state, _now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenAnswerCountDoesNotMatchQuestionCount()
    {
        // Arrange
        QuizSessionState state = ValidState(2);
        state.Answers.Add(SubmittedAnswer.Empty);

        // Act
        bool result = QuizSessionValidator.IsValid(state, _now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenCurrentIndexIsNegative()
    {
        // Arrange
        QuizSessionState state = ValidState(2);
        state.CurrentIndex = -1;

        // Act
        bool result = QuizSessionValidator.IsValid(state, _now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenCurrentIndexExceedsLastQuestion()
    {
        // Arrange
        int questionCount = 2;
        QuizSessionState state = ValidState(questionCount);
        state.CurrentIndex = questionCount;

        // Act
        bool result = QuizSessionValidator.IsValid(state, _now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenStartTimestampIsInTheFuture()
    {
        // Arrange
        QuizSessionState state = ValidState(1);
        state.StartedAtUtc = _now.AddMinutes(10);

        // Act
        bool result = QuizSessionValidator.IsValid(state, _now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsTrueAndTimerReadsExpired_WhenStartTimestampIsMovedFarBackwards()
    {
        // Arrange
        // Start recorded as 2 hours ago. Timed session with 60-minute limit.
        // IsValid must accept this: "too far in past" is not a validity condition.
        // ExamTimer.Remaining must return zero, not negative.
        const int timeLimitMinutes = 60;
        const int minutesPastStart = 120;
        QuizSessionState state = ValidState(1);
        state.Timed            = true;
        state.TimeLimitMinutes = timeLimitMinutes;
        state.StartedAtUtc     = _now.AddMinutes(-minutesPastStart);

        // Act
        bool isValid   = QuizSessionValidator.IsValid(state, _now);
        TimeSpan timer = ExamTimer.Remaining(state, _now);

        // Assert
        Assert.True(isValid);
        Assert.Equal(TimeSpan.Zero, timer);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenQuestionInListIsNull()
    {
        // Arrange
        QuizSessionState state = ValidState(1);
        state.Questions[0] = null;

        // Act
        bool result = QuizSessionValidator.IsValid(state, _now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenTimeLimitIsAboveMaximum()
    {
        // Arrange
        const int excessiveTimeLimitMinutes = 601;
        QuizSessionState state = ValidState(1);
        state.Timed            = true;
        state.TimeLimitMinutes = excessiveTimeLimitMinutes;

        // Act
        bool result = QuizSessionValidator.IsValid(state, _now);

        // Assert
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
