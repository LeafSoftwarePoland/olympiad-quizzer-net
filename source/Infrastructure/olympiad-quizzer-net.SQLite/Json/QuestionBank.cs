using OlympiadQuizzer.Domain.Questions;

namespace OlympiadQuizzer.Infrastructure.SQLite.Json;

public sealed class QuestionBank
{
    private readonly IReadOnlyList<Question> _questions;

    public QuestionBank(List<Question> questions)
    {
        // AsReadOnly, not the list itself: this collection is the process-wide singleton, and an
        // in-place shuffle of it would make every response depend on request history.
        _questions = questions.AsReadOnly();
    }

    public IReadOnlyList<Question> Questions => _questions;
}
