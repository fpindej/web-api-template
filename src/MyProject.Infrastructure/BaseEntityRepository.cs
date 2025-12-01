using System.Linq.Expressions;
using MyProject.Application;
using MyProject.Domain;
using MyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using MyProject.Infrastructure.Features.Postgres.Extensions;

namespace MyProject.Infrastructure;

internal class BaseEntityRepository<TEntity>(DbSet<TEntity> dbSet, TimeProvider timeProvider)
    : IBaseEntityRepository<TEntity>
    where TEntity : BaseEntity
{
    protected readonly DbSet<TEntity> DbSet = dbSet;
    protected readonly TimeProvider TimeProvider = timeProvider;

    public virtual async Task<TEntity?> GetByIdAsync(
        Guid id,
        bool asTracking = false,
        CancellationToken cancellationToken = default)
    {
        var query = asTracking
            ? DbSet.AsTracking()
            : DbSet.AsNoTracking();

        return await query.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
    }

    public virtual async Task<IReadOnlyList<TEntity>> GetAllAsync(
        int pageNumber,
        int pageSize,
        bool asTracking = false,
        CancellationToken cancellationToken = default)
    {
        var query = DbSet
            .Where(e => !e.IsDeleted)
            .OrderByDescending(e => e.CreatedAt)
            .Paginate(pageNumber, pageSize);

        query = asTracking
            ? query.AsTracking()
            : query.AsNoTracking();

        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<Result<TEntity>> AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        try
        {
            await DbSet.AddAsync(entity, cancellationToken);
            return Result<TEntity>.Success(entity);
        }
        catch (Exception ex)
        {
            return Result<TEntity>.Failure($"Failed to add entity: {ex.Message}");
        }
    }

    public virtual void Update(TEntity entity)
    {
        DbSet.Update(entity);
    }

    public virtual async Task<Result<TEntity>> SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FirstOrDefaultAsync(e => e.Id == id && !e.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return Result<TEntity>.Failure($"Entity with ID {id} not found or already deleted.");
        }

        var utcNow = TimeProvider.GetUtcNow().UtcDateTime;
        entity.SoftDelete(utcNow);
        DbSet.Update(entity);
        return Result<TEntity>.Success(entity);
    }

    public virtual async Task<Result<TEntity>> RestoreAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var entity = await DbSet.FirstOrDefaultAsync(e => e.Id == id && e.IsDeleted, cancellationToken);
        if (entity is null)
        {
            return Result<TEntity>.Failure($"Entity with ID {id} not found or not deleted.");
        }

        var utcNow = TimeProvider.GetUtcNow().UtcDateTime;
        entity.Restore(utcNow);
        DbSet.Update(entity);
        return Result<TEntity>.Success(entity);
    }

    public virtual async Task<bool> ExistsAsync(
        Expression<Func<TEntity, bool>> predicate,
        CancellationToken cancellationToken = default)
    {
        return await DbSet.AnyAsync(predicate, cancellationToken);
    }
}
