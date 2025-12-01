namespace MyProject.WebApi.Shared;

/// <summary>
/// Base class for response DTOs containing system tracking fields.
/// </summary>
public abstract class BaseResponse
{
    /// <summary>
    /// The date and time when the entity was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// The date and time when the entity was last updated.
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}
