using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyProject.Infrastructure.Features.Authentication.Models;

namespace MyProject.Infrastructure.Features.Postgres.Extensions;

internal static class ModelBuilderExtensions
{
    public static void ApplyAuthSchema(this ModelBuilder builder)
    {
        const string schema = "auth";

        _ = builder.Entity<ApplicationUser>().ToTable(name: "Users", schema);
        _ = builder.Entity<ApplicationRole>().ToTable(name: "Roles", schema);
        _ = builder.Entity<IdentityUserRole<string>>().ToTable(name: "UserRoles", schema);
        _ = builder.Entity<IdentityUserClaim<string>>().ToTable(name: "UserClaims", schema);
        _ = builder.Entity<IdentityUserLogin<string>>().ToTable(name: "UserLogins", schema);
        _ = builder.Entity<IdentityRoleClaim<string>>().ToTable(name: "RoleClaims", schema);
        _ = builder.Entity<IdentityUserToken<string>>().ToTable(name: "UserTokens", schema);
    }

    public static void ApplyFuzzySearch(this ModelBuilder builder) =>
        builder
            .HasDbFunction(
                typeof(StringExtensions)
                    .GetMethod(nameof(StringExtensions.Similarity))!)
            .HasName("similarity")
            .IsBuiltIn();
}
