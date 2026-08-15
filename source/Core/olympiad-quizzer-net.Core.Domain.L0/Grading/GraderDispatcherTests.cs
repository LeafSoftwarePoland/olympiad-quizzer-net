using Moq;
using OlympiadQuizzer.Core.Tests.Common;
using OlympiadQuizzer.Core.Domain.Grading;
using OlympiadQuizzer.Core.Domain.Questions;
using OlympiadQuizzer.Core.Tests.Common.Builders;

namespace OlympiadQuizzer.Core.Domain.L0.Grading;

[Trait(TestTiers.Tier, TestTiers.L0)]
public sealed class GraderDispatcherTests
{
    private static IQuestionGrader StubGrader(QuestionType handledType, GradeResult returnValue)
    {
        var mock = new Mock<IQuestionGrader>();
        mock.Setup(g => g.QuestionType).Returns(handledType);
        mock.Setup(g => g.Grade(It.IsAny<Question>(), It.IsAny<SubmittedAnswer>())).Returns(returnValue);
        return mock.Object;
    }

    private static IQuestionGrader ThrowingGrader(QuestionType handledType, Exception exception)
    {
        var mock = new Mock<IQuestionGrader>();
        mock.Setup(g => g.QuestionType).Returns(handledType);
        mock.Setup(g => g.Grade(It.IsAny<Question>(), It.IsAny<SubmittedAnswer>())).Throws(exception);
        return mock.Object;
    }

    [Fact]
    public void Grade_ReturnsGraderResult_WhenGraderIsRegisteredForQuestionType()
    {
        // Arrange
        GradeResult expectedResult = new(true, 1.0, 1.0);
        IQuestionGrader grader = StubGrader(QuestionType.Single, expectedResult);
        GraderDispatcher dispatcher = new([grader]);
        Question question = QuestionBuilder.AQuestion().WithType(QuestionType.Single).Build();
        SubmittedAnswer answer = new();

        // Act
        GradeResult result = dispatcher.Grade(question, answer);

        // Assert
        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void Grade_DispatchesToCorrectGrader_WhenMultipleGradersAreRegistered()
    {
        // Arrange
        GradeResult singleResult = new(true, 1.0, 1.0);
        GradeResult multiResult  = new(false, 0.0, 2.0);
        IQuestionGrader singleGrader = StubGrader(QuestionType.Single, singleResult);
        IQuestionGrader multiGrader  = StubGrader(QuestionType.Multi, multiResult);
        GraderDispatcher dispatcher = new([singleGrader, multiGrader]);
        Question question = QuestionBuilder.AQuestion().WithType(QuestionType.Multi).Build();
        SubmittedAnswer answer = new();

        // Act
        GradeResult result = dispatcher.Grade(question, answer);

        // Assert
        Assert.Equal(multiResult, result);
    }

    [Fact]
    public void Grade_ThrowsInvalidOperationException_WhenNoGraderIsRegisteredForType()
    {
        // Arrange
        IQuestionGrader grader = StubGrader(QuestionType.Single, new GradeResult(true, 1.0, 1.0));
        GraderDispatcher dispatcher = new([grader]);
        Question question = QuestionBuilder.AQuestion().WithType(QuestionType.Multi).Build();
        SubmittedAnswer answer = new();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => dispatcher.Grade(question, answer));
    }

    [Fact]
    public void Grade_ThrowsInvalidOperationException_WhenRegisteredCollectionIsEmpty()
    {
        // Arrange
        GraderDispatcher dispatcher = new([]);
        Question question = QuestionBuilder.AQuestion().WithType(QuestionType.Single).Build();
        SubmittedAnswer answer = new();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => dispatcher.Grade(question, answer));
    }

    [Fact]
    public void Grade_BubblesAnticipatedInvalidOperationException_WhenGraderThrows()
    {
        // Arrange
        InvalidOperationException graderException = new("grader failed");
        IQuestionGrader grader = ThrowingGrader(QuestionType.Single, graderException);
        GraderDispatcher dispatcher = new([grader]);
        Question question = QuestionBuilder.AQuestion().WithType(QuestionType.Single).Build();
        SubmittedAnswer answer = new();

        // Act & Assert
        InvalidOperationException thrown = Assert.Throws<InvalidOperationException>(
            () => dispatcher.Grade(question, answer));
        Assert.Same(graderException, thrown);
    }

    [Fact]
    public void Grade_BubblesUnanticipatedExceptions_WhenGraderThrowsUnexpectedType()
    {
        // Arrange
        Exception unexpectedException = new("unanticipated failure");
        IQuestionGrader grader = ThrowingGrader(QuestionType.Single, unexpectedException);
        GraderDispatcher dispatcher = new([grader]);
        Question question = QuestionBuilder.AQuestion().WithType(QuestionType.Single).Build();
        SubmittedAnswer answer = new();

        // Act & Assert
        Exception thrown = Assert.Throws<Exception>(
            () => dispatcher.Grade(question, answer));
        Assert.Same(unexpectedException, thrown);
    }
}
