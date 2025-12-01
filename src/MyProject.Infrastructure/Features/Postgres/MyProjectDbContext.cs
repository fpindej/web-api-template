using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyProject.Infrastructure.Features.Authentication.Models;
using MyProject.Infrastructure.Features.Postgres.Extensions;

namespace MyProject.Infrastructure.Features.Postgres;

public class MyProjectDbContext(DbContextOptions options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, string>(options)
{
    // ToDo: Add DbSet<TEntity> properties for your entities

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(MyProjectDbContext).Assembly);
        modelBuilder.ApplyAuthSchema();
        modelBuilder.ApplyFuzzySearch();

        // Seed default roles
        modelBuilder.Entity<ApplicationRole>().HasData(
            new ApplicationRole
            {
                Id = "1",
                Name = "User",
                NormalizedName = "USER",
                ConcurrencyStamp = "76b99507-9cf8-4708-9fe8-4dc4e9ea09ae"
            },
            new ApplicationRole
            {
                Id = "2",
                Name = "Admin",
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "971e674f-c4fb-4bb2-9170-3ad7a753182c"
            }
        );
    }
}
