using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using MyProject.Infrastructure.Features.Authentication.Constants;
using MyProject.Infrastructure.Features.Authentication.Models;
using MyProject.Infrastructure.Features.Authentication.Options;
using MyProject.Infrastructure.Features.Authentication.Services;

namespace MyProject.Infrastructure.Features.Authentication.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIdentity<TContext>(this IServiceCollection services,
        IConfiguration configuration) where TContext : DbContext
    {
        services.ConfigureIdentity<TContext>(configuration);
        services.ConfigureJwtAuthentication(configuration);

        services.AddScoped<ITokenProvider, JwtTokenProvider>();
        services.AddScoped<AuthenticationService>();

        return services;
    }

    private static IServiceCollection ConfigureIdentity<TContext>(this IServiceCollection services,
        IConfiguration configuration) where TContext : DbContext
    {
        // Register custom refresh token provider
        services.AddTransient<CustomRefreshTokenProvider>();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;

        services.AddIdentity<ApplicationUser, ApplicationRole>(opt =>
            {
                opt.Password.RequireDigit = true;
                opt.Password.RequireLowercase = true;
                opt.Password.RequireUppercase = true;
                opt.Password.RequireNonAlphanumeric = false;
                opt.Password.RequiredLength = 6;

                opt.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
                opt.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;

                opt.Tokens.AuthenticatorTokenProvider = TokenOptions.DefaultAuthenticatorProvider;
            })
            .AddEntityFrameworkStores<TContext>()
            .AddDefaultTokenProviders();

        // Configure the provider with identity options
        services.Configure<IdentityOptions>(options =>
        {
            options.Tokens.ProviderMap.Add(jwtOptions.RefreshToken.ProviderName,
                new TokenProviderDescriptor(typeof(CustomRefreshTokenProvider)));
        });

        return services;
    }

    private static IServiceCollection ConfigureJwtAuthentication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddOptions<JwtOptions>()
            .BindConfiguration(JwtOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()!;
        var key = Encoding.UTF8.GetBytes(jwtOptions.Key);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(key)
                };

                // Configure cookie-based token handling
                opt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        if (context.Request.Cookies.TryGetValue(CookieNames.AccessToken, out var accessToken))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    },
                };

                opt.SaveToken = true;
            });

        return services;
    }
}
