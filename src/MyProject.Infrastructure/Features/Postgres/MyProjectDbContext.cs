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
            new ApplicationRole { Id = "1", Name = "User", NormalizedName = "USER" },
            new ApplicationRole { Id = "2", Name = "Admin", NormalizedName = "ADMIN" }
        );
    }
}