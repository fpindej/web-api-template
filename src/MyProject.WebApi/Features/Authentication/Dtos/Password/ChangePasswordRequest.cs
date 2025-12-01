using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyProject.WebApi.Features.Authentication.Dtos.Password;

/// <summary>
/// DTO for change password request.
/// </summary>
[Description("Change password request DTO")]
public class ChangePasswordRequest
{
    /// <summary>
    /// The current password.
    /// </summary>
    [Description("The current password")]
    [DataType(DataType.Password)]
    [Required]
    public string CurrentPassword { get; set; } = null!;
    
    /// <summary>
    /// The new password.
    /// </summary>
    [Description("The new password")]
    [Required]
    [DataType(DataType.Password)]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}
