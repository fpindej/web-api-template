using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MyProject.Infrastructure.Features.Authentication.Extensions;

namespace MyProject.Infrastructure.Features.Postgres.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.ConfigureDbContext(configuration);

        return services;
    }

    public static IServiceCollection AddIdentityServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentity<MyProjectDbContext>(configuration);

        return services;
    }

    private static IServiceCollection ConfigureDbContext(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<MyProjectDbContext>((sp, opt) =>
        {
            var connectionString = configuration.GetConnectionString("Database");
            opt.UseNpgsql(connectionString);
            var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<MyProjectDbContext>();
            logger.LogInformation("Opening database connection: {connectionString}", connectionString);
        });
        return services;
    }
}
