using System.Linq.Expressions;

namespace MyProject.Infrastructure.Features.Postgres.Extensions;

/// <summary>
/// Extension methods for IQueryable to support conditional filtering
/// </summary>
public static class QueryableExtensions
{
    /// <summary>
    /// Conditionally applies a Where clause if the condition is not null
    /// </summary>
    /// <typeparam name="T">The type of the entity</typeparam>
    /// <typeparam name="TValue">The type of the condition value</typeparam>
    /// <param name="query">The query to extend</param>
    /// <param name="condition">The condition value (if null, no filter is applied)</param>
    /// <param name="predicate">The predicate to apply when condition is not null</param>
    /// <returns>The filtered query if condition is not null, otherwise the original query</returns>
    public static IQueryable<T> ConditionalWhere<T, TValue>(
        this IQueryable<T> query,
        TValue? condition,
        Expression<Func<T, bool>> predicate)
        where TValue : struct
    {
        return condition.HasValue ? query.Where(predicate) : query;
    }

    /// <summary>
    /// Conditionally applies a Where clause if the condition is not null or empty
    /// </summary>
    /// <typeparam name="T">The type of the entity</typeparam>
    /// <param name="query">The query to extend</param>
    /// <param name="condition">The condition value (if null or empty, no filter is applied)</param>
    /// <param name="predicate">The predicate to apply when condition is not null or empty</param>
    /// <returns>The filtered query if condition is not null or empty, otherwise the original query</returns>
    public static IQueryable<T> ConditionalWhere<T>(
        this IQueryable<T> query,
        string? condition,
        Expression<Func<T, bool>> predicate)
    {
        return !string.IsNullOrEmpty(condition) ? query.Where(predicate) : query;
    }

    /// <summary>
    /// Conditionally applies a Where clause if the collection has at least one value
    /// </summary>
    /// <typeparam name="T">The type of the entity</typeparam>
    /// <typeparam name="TValue">The element type of the collection</typeparam>
    /// <param name="query">The query to extend</param>
    /// <param name="values">The values collection (if null or empty, no filter is applied)</param>
    /// <param name="predicate">The predicate to apply when values has at least one element</param>
    /// <returns>The filtered query if values has elements, otherwise the original query</returns>
    public static IQueryable<T> ConditionalWhere<T, TValue>(
        this IQueryable<T> query,
        IEnumerable<TValue>? values,
        Expression<Func<T, bool>> predicate)
        where TValue : struct
    {
        return values != null && values.Any() ? query.Where(predicate) : query;
    }
}