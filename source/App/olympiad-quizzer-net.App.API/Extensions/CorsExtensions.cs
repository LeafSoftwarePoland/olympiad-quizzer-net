using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace OlympiadQuizzer.App.Api.Extensions;

internal static class CorsExtensions
{
    private const string _policyName = "frontend";

    internal static IServiceCollection AddFrontendCors(this IServiceCollection services)
    {
        services.AddCors(o => o.AddPolicy(_policyName, ConfigureCors));
        return services;
    }

    internal static WebApplication UseFrontendCors(this WebApplication app)
    {
        app.UseCors(_policyName);
        return app;
    }

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
