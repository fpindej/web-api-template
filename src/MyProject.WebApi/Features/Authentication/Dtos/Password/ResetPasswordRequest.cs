using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyProject.WebApi.Features.Authentication.Dtos.Password;

/// <summary>
/// DTO for reset password request.
/// </summary>
[Description("Reset password request DTO")]
public class ResetPasswordRequest
{
    /// <summary>
    /// The user ID.
    /// </summary>
    [Description("The user ID")]
    [Required]
    public string UserId { get; set; } = null!;
    
    /// <summary>
    /// The reset token.
    /// </summary>
    [Description("The reset token")]
    [Required]
    public string Token { get; set; } = null!;
    
    /// <summary>
    /// The new password.
    /// </summary>
    [Description("The new password")]
    [DataType(DataType.Password)]
    [Required]
    [MinLength(6)]
    public string NewPassword { get; set; } = null!;
}