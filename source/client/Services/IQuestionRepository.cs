using OlympiadQuizzer.Shared;

namespace OlympiadQuizzer.Client.Services;

public interface IQuestionRepository
{
    Task<List<Question>> GetAsync(QuizFilter filter);
}
