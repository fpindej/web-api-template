namespace MyProject.Domain.Entities;

public abstract class BaseEntity
{
    public Guid Id { get; protected init; }

    public DateTime CreatedAt { get; private init; }

    public DateTime? UpdatedAt { get; private set; }

    public bool IsDeleted { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    // Protected constructor for EF Core
    protected BaseEntity()
    {
    }

    protected BaseEntity(DateTime createdAt)
    {
        CreatedAt = createdAt;
    }

    protected void SetUpdatedAt(DateTime updatedAt)
    {
        UpdatedAt = updatedAt;
    }

    public void SoftDelete(DateTime deletedAt)
    {
        if (IsDeleted)
        {
            return;
        }

        IsDeleted = true;
        DeletedAt = deletedAt;
        SetUpdatedAt(deletedAt);
    }

    public void Restore(DateTime restoredAt)
    {
        if (!IsDeleted)
        {
            return;
        }

        IsDeleted = false;
        DeletedAt = null;
        SetUpdatedAt(restoredAt);
    }
}
