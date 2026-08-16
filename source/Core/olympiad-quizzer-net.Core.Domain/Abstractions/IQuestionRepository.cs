using OlympiadQuizzer.Core.Domain.Queries;
using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.Core.Domain.Abstractions;

public interface IQuestionRepository
{
    Task<IReadOnlyList<Question>> GetAsync(QuestionQuery query, CancellationToken cancellationToken);

    Task<FilterOptions> GetFilterOptionsAsync(CancellationToken cancellationToken);
}
