using OlympiadQuizzer.Core.Domain.Abstractions;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Domain.Session;

namespace OlympiadQuizzer.App.Client.Features.Quiz;

public sealed class QuizSession
{
    private readonly IQuestionRepository _repository;
    private readonly QuizSessionStore _store;

    public QuizSession(IQuestionRepository repository, QuizSessionStore store)
    {
        _repository = repository;
        _store = store;
        Status = QuizSessionStatus.None;
    }

    public QuizSessionStatus Status { get; private set; }
    public QuizSessionState State { get; private set; }

    public bool HasSession => State != null && State.Questions.Count > 0;

    public Question CurrentQuestion
    {
        get
        {
            if (State == null || State.CurrentIndex >= State.Questions.Count)
            {
                return null;
            }

            return State.Questions[State.CurrentIndex];
        }
    }

    public SubmittedAnswer CurrentAnswer
    {
        get
        {
            if (State == null || State.CurrentIndex >= State.Answers.Count)
            {
                return SubmittedAnswer.Empty;
            }

            return State.Answers[State.CurrentIndex];
        }
    }

    public bool IsLastQuestion => State != null && State.CurrentIndex == State.Questions.Count - 1;

    public async Task<bool> TryResumeAsync()
    {
        QuizSessionState loaded = await _store.TryLoadAsync();
        if (loaded == null)
        {
            return false;
        }

        State = loaded;
        Status = QuizSessionStatus.Running;
        return true;
    }

    public async Task StartAsync(QuestionQuery query, string modeId, bool timed, int timeLimitMinutes, CancellationToken cancellationToken)
    {
        Status = QuizSessionStatus.Loading;

        IReadOnlyList<Question> questions = await _repository.GetAsync(query, cancellationToken);

        if (questions.Count == 0)
        {
            State = null;
            Status = QuizSessionStatus.None;
            return;
        }

        QuizSessionState state = new()
		{
            SchemaVersion = 1,
            ModeId = modeId,
            Timed = timed,
            TimeLimitMinutes = timeLimitMinutes,
            StartedAtUtc = DateTimeOffset.UtcNow,
            CurrentIndex = 0,
            Questions = new List<Question>(questions),
            Answers = []
        };

        for (int i = 0; i < questions.Count; i++)
        {
            state.Answers.Add(SubmittedAnswer.Empty);
        }

        State = state;
        Status = QuizSessionStatus.Running;
        await _store.SaveAsync(State);
    }

    public GradeResult SubmitAnswer(SubmittedAnswer answer)
    {
        if (State == null)
        {
            return new GradeResult(false, 0, 0);
        }

        State.Answers[State.CurrentIndex] = answer;

        GradeResult result = CurrentQuestion.Type switch
        {
            QuestionType.Single      => GraderSingle.Grade(CurrentQuestion, answer),
            QuestionType.Multi       => GraderMulti.Grade(CurrentQuestion, answer),
            QuestionType.ShortAnswer => GraderShortAnswer.Grade(CurrentQuestion, answer),
            QuestionType.TrueFalse or QuestionType.Ordering or QuestionType.Matching
                                     => GraderPositional.Grade(CurrentQuestion, answer),
            _                        => new GradeResult(false, 0, CurrentQuestion.Points)
        };

        return result;
    }

    public async Task AdvanceAsync()
    {
        if (State == null)
        {
            return;
        }

        if (State.CurrentIndex < State.Questions.Count - 1)
        {
            State.CurrentIndex++;
            await _store.SaveAsync(State);
        }
        else
        {
            Status = QuizSessionStatus.Complete;
            await _store.ClearAsync();
        }
    }

    public void Reset()
    {
        State = null;
        Status = QuizSessionStatus.None;
    }
}
