using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace MyProject.Infrastructure.Features.Postgres.Extensions;

public static class ApplicationBuilderExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder appBuilder)
    {
        using var scope = appBuilder.ApplicationServices.CreateScope();

        var dbContext = scope.ServiceProvider.GetRequiredService<MyProjectDbContext>();

        dbContext.Database.Migrate();
    }
}
