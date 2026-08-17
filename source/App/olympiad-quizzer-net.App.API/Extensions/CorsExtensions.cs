using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace OlympiadQuizzer.App.Api.Extensions;

internal static class CorsExtensions
{
    private const string _policyName = "frontend";

    internal static IServiceCollection AddFrontendCors(this IServiceCollection services, string allowedOrigin)
    {
        services.AddCors(o => o.AddPolicy(_policyName, policy => ConfigureCors(policy, allowedOrigin)));
        return services;
    }

    internal static WebApplication UseFrontendCors(this WebApplication app)
    {
        app.UseCors(_policyName);
        return app;
    }

    private static void ConfigureCors(CorsPolicyBuilder policy, string allowedOrigin)
    {
        policy
            .SetIsOriginAllowed(origin => IsAllowedOrigin(origin, allowedOrigin))
            .AllowAnyHeader()
            .AllowAnyMethod();
    }

    internal static bool IsAllowedOrigin(string origin, string allowedOrigin)
    {
        if (string.IsNullOrWhiteSpace(origin))
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(allowedOrigin)
            && string.Equals(origin, allowedOrigin, StringComparison.OrdinalIgnoreCase))
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
