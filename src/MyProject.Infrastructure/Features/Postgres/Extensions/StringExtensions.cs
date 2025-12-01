using Microsoft.EntityFrameworkCore;

namespace MyProject.Infrastructure.Features.Postgres.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Escapes special characters in a string for safe use in SQL LIKE queries.
    /// Removes control characters and escapes SQL LIKE wildcards.
    /// </summary>
    /// <param name="input">The input string to escape</param>
    /// <returns>A sanitized and escaped string safe for SQL LIKE operations</returns>
    public static string EscapeForSqlLike(this string input)
    {
        var sanitized = new string(input
                .Where(c => !char.IsControl(c))
                .ToArray())
            .Trim();

        return sanitized
            .Replace("[", "[[]")
            .Replace("%", "[%]")
            .Replace("_", "[_]");
    }

    /// <summary>
    /// Maps to the PostgreSQL 'similarity' function which calculates text similarity between two strings.
    /// </summary>
    /// <param name="a">The first string to compare</param>
    /// <param name="b">The second string to compare</param>
    /// <returns>A value between 0 and 1, where 1 means identical strings and 0 means completely different strings</returns>
    /// <remarks>
    /// This is a placeholder method that enables Entity Framework Core to translate method calls to the PostgreSQL 'similarity' function.
    /// The actual implementation is handled by PostgreSQL.
    /// </remarks>
    [DbFunction("similarity", IsBuiltIn = true)]
    public static double Similarity(this string a, string b)
    {
        throw new NotSupportedException(
            "This is a placeholder/pointer method to call the `similarity` method in postgres.");
    }
}