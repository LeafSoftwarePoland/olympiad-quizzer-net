namespace OlympiadQuizzer.Client.Shared.Services;

public sealed class ToastMessage
{
    public int Id { get; }
    public string Message { get; }
    public ToastKind Kind { get; }

    public ToastMessage(int id, string message, ToastKind kind)
    {
        Id = id;
        Message = message;
        Kind = kind;
    }
}
