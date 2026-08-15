using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.Core.Domain.Grading;

public sealed class GraderDispatcher
{
    private readonly Dictionary<QuestionType, IQuestionGrader> _graders;

    public GraderDispatcher(IEnumerable<IQuestionGrader> graders)
    {
        _graders = graders.ToDictionary(g => g.QuestionType);
    }

    public GradeResult Grade(Question question, SubmittedAnswer answer)
    {
        if (!_graders.TryGetValue(question.Type, out IQuestionGrader grader))
        {
            throw new InvalidOperationException(
                $"Unregistered grader for question type: {question.Type}.");
        }

        return grader.Grade(question, answer);
    }
}
