using System.Reflection;

namespace OlympiadQuizzer.App.Client.Shared.Services;

public sealed class AppVersionService
{
    private readonly string _version;

    public AppVersionService()
    {
        // The version is compiled in from Directory.Build.props, so it is present in every build
        // including a local one. It used to be fetched from a generated version.json, which the
        // deploy wrote and local development did not have — so the footer was blank until deploy.
        Assembly assembly = typeof(AppVersionService).Assembly;

        string informational = assembly
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
            ?.InformationalVersion;

        _version = string.IsNullOrWhiteSpace(informational)
            ? assembly.GetName().Version?.ToString(3) ?? string.Empty
            : StripBuildMetadata(informational);
    }

    public string GetVersion()
    {
        return _version;
    }

    // The SDK appends "+<commit sha>" to the informational version. A student reading a footer
    // has no use for it.
    private static string StripBuildMetadata(string informationalVersion)
    {
        int plusIndex = informationalVersion.IndexOf('+');
        return plusIndex < 0 ? informationalVersion : informationalVersion[..plusIndex];
    }
}
