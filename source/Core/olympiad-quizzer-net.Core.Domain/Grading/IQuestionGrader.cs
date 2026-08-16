using OlympiadQuizzer.Core.Domain.Questions;

namespace OlympiadQuizzer.Core.Domain.Grading;

public interface IQuestionGrader
{
    QuestionType QuestionType { get; }

    GradeResult Grade(Question question, SubmittedAnswer answer);
}
