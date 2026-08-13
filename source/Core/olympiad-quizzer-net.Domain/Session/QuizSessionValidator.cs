using OlympiadQuizzer.Domain.Queries;
using OlympiadQuizzer.Domain.Questions;

namespace OlympiadQuizzer.Domain.Session;

public static class QuizSessionValidator
{
    public static bool IsValid(QuizSessionState state, DateTimeOffset now)
    {
        if (state == null)
        {
            return false;
        }

        if (state.SchemaVersion != 1)
        {
            return false;
        }

        if (state.Questions == null || state.Questions.Count == 0)
        {
            return false;
        }

        if (state.Questions.Count > QuestionQuery.MaxLimit)
        {
            return false;
        }

        if (state.Answers == null || state.Answers.Count != state.Questions.Count)
        {
            return false;
        }

        if (state.CurrentIndex < 0 || state.CurrentIndex >= state.Questions.Count)
        {
            return false;
        }

        if (state.StartedAtUtc == default)
        {
            return false;
        }

        if (state.StartedAtUtc > now.AddMinutes(5))
        {
            return false;
        }

        if (state.Timed && (state.TimeLimitMinutes <= 0 || state.TimeLimitMinutes > 600))
        {
            return false;
        }

        foreach (Question question in state.Questions)
        {
            if (question == null)
            {
                return false;
            }

            if (question.Type == QuestionType.Unknown)
            {
                return false;
            }

            if (question.Content == null || question.Content.Count == 0)
            {
                return false;
            }
        }

        return true;
    }
}
