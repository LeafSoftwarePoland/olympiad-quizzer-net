namespace OlympiadQuizzer.App.Client.Shared.Services;

public sealed class ToastService
{
    private readonly List<ToastMessage> _messages = [];
    private int _nextId;

    public IReadOnlyList<ToastMessage> Messages => _messages;

    public event Action OnChange;

    public void ShowSuccess(string message)
    {
        AddToast(message, ToastKind.Success);
    }

    public void ShowError(string message)
    {
        AddToast(message, ToastKind.Error);
    }

    public void ShowInfo(string message)
    {
        AddToast(message, ToastKind.Info);
    }

    public void Dismiss(int id)
    {
        _messages.RemoveAll(m => m.Id == id);
        OnChange?.Invoke();
    }

    private void AddToast(string message, ToastKind kind)
    {
        int id = Interlocked.Increment(ref _nextId);
        _messages.Add(new ToastMessage(id, message, kind));
        OnChange?.Invoke();

        _ = DismissAfterDelayAsync(id);
    }

    private async Task DismissAfterDelayAsync(int id)
    {
        await Task.Delay(TimeSpan.FromSeconds(4));
        Dismiss(id);
    }
}
