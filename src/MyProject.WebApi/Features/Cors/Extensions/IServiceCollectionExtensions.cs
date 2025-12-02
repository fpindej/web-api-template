using Microsoft.AspNetCore.Cors.Infrastructure;
using CorsOptions = MyProject.WebApi.Features.Cors.Options.CorsOptions;

namespace MyProject.WebApi.Features.Cors.Extensions;

internal static class IServiceCollectionExtensions
{
    public static IServiceCollection AddCors(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<CorsOptions>()
            .BindConfiguration(CorsOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var corsSettings = configuration.GetSection(CorsOptions.SectionName).Get<CorsOptions>()
                           ?? throw new InvalidOperationException("CORS options are not configured properly.");

        services.AddCors(options =>
        {
            options.AddPolicy(corsSettings.PolicyName, policy =>
            {
                {
                    policy.ConfigureCorsPolicy(corsSettings);
                }
            });
        });

        return services;
    }

    private static CorsPolicyBuilder ConfigureCorsPolicy(this CorsPolicyBuilder policy, CorsOptions corsOptions)
    {
        policy.AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();

        return corsOptions.AllowAllOrigins switch
        {
            true => policy.SetIsOriginAllowed(_ => true),
            false => policy.WithOrigins(corsOptions.AllowedOrigins)
        };
    }
}
