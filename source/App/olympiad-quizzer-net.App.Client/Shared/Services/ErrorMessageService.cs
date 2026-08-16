using OlympiadQuizzer.Core.Domain.Errors;

namespace OlympiadQuizzer.App.Client.Shared.Services;

public sealed class ErrorMessageService
{
    private const string _heading = "Coś poszło nie tak.";
    private const string _detail =
        "Spróbuj ponownie wykonać ostatnią akcję. " +
        "Jeżeli błąd nadal występuje, skontaktuj się proszę z twórcą aplikacji wraz z zrzutami ekranu.";

    private static readonly Dictionary<string, string> _codeMessages = new()
    {
        [ErrorCodes.Unexpected] = _heading + "\n" + _detail
    };

    public string GetMessage(string code)
    {
        if (code != null && _codeMessages.TryGetValue(code, out string mapped))
        {
            return mapped;
        }
        return _heading + "\n" + _detail;
    }
}
