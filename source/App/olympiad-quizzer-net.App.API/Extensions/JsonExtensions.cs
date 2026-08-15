using Microsoft.Extensions.DependencyInjection;
using OlympiadQuizzer.Core.Domain.Serialization;
using System.Text.Json.Serialization;

namespace OlympiadQuizzer.App.Api.Extensions;

internal static class JsonExtensions
{
    internal static IServiceCollection AddApiJsonOptions(this IServiceCollection services)
    {
        services.ConfigureHttpJsonOptions(o =>
        {
            o.SerializerOptions.PropertyNamingPolicy = JsonOptions.Default.PropertyNamingPolicy;
            o.SerializerOptions.PropertyNameCaseInsensitive = JsonOptions.Default.PropertyNameCaseInsensitive;
            o.SerializerOptions.DefaultIgnoreCondition = JsonOptions.Default.DefaultIgnoreCondition;
            o.SerializerOptions.Encoder = JsonOptions.Default.Encoder;

            foreach (JsonConverter converter in JsonOptions.Default.Converters)
            {
                o.SerializerOptions.Converters.Add(converter);
            }
        });

        services.Configure<Microsoft.AspNetCore.Mvc.JsonOptions>(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = JsonOptions.Default.PropertyNamingPolicy;
            o.JsonSerializerOptions.PropertyNameCaseInsensitive = JsonOptions.Default.PropertyNameCaseInsensitive;
            o.JsonSerializerOptions.DefaultIgnoreCondition = JsonOptions.Default.DefaultIgnoreCondition;
            o.JsonSerializerOptions.Encoder = JsonOptions.Default.Encoder;

            foreach (JsonConverter converter in JsonOptions.Default.Converters)
            {
                o.JsonSerializerOptions.Converters.Add(converter);
            }
        });

        return services;
    }
}
