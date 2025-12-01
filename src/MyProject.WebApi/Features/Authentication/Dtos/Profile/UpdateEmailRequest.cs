using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyProject.WebApi.Features.Authentication.Dtos.Profile;

/// <summary>
/// DTO for updating email request.
/// </summary>
[Description("Update email request DTO")]
public class UpdateEmailRequest
{
    /// <summary>
    /// The new email address.
    /// </summary>
    [Description("The new email address")]
    [Required]
    [EmailAddress]
    public string NewEmail { get; set; } = null!;
    
    /// <summary>
    /// The user's current password for verification.
    /// </summary>
    [Description("The user's current password for verification")]
    [DataType(DataType.Password)]
    [Required]
    public string CurrentPassword { get; set; } = null!;
}
