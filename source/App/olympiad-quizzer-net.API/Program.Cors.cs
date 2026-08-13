using System;
using Microsoft.AspNetCore.Cors.Infrastructure;

namespace OlympiadQuizzer.Api;

public partial class Program
{
    private static void ConfigureCors(CorsPolicyBuilder policy)
    {
        policy
            .SetIsOriginAllowed(IsAllowedOrigin)
            .AllowAnyHeader()
            .AllowAnyMethod();
    }

    private static bool IsAllowedOrigin(string origin)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            return false;
        }

        if (string.Equals(origin, "https://leafsoftwarepoland.github.io", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (!Uri.TryCreate(origin, UriKind.Absolute, out Uri uri))
        {
            return false;
        }

        return string.Equals(uri.Host, "localhost", StringComparison.OrdinalIgnoreCase)
            || uri.Host == "127.0.0.1"
            || uri.Host == "[::1]";
    }
}
