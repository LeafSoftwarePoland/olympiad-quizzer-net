using OlympiadQuizzer.Domain.Queries;
using OlympiadQuizzer.Domain.Questions;

namespace OlympiadQuizzer.Domain.Abstractions;

public interface IQuestionRepository
{
    Task<IReadOnlyList<Question>> GetAsync(QuestionQuery query, CancellationToken cancellationToken);

    Task<FilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken);
}
