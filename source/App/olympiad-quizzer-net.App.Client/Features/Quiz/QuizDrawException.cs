namespace OlympiadQuizzer.App.Client.Features.Quiz;

public sealed class QuizDrawException : Exception
{
    public QuizDrawException(string polishMessage) : base(polishMessage) { }
}
