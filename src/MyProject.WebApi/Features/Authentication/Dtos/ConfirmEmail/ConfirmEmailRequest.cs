using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyProject.WebApi.Features.Authentication.Dtos.ConfirmEmail;

/// <summary>
/// DTO for email verification request.
/// </summary>
[Description("Email verification request DTO")]
public class ConfirmEmailRequest
{
    /// <summary>
    /// The user ID.
    /// </summary>
    [Description("The user ID")]
    [Required]
    public string UserId { get; set; } = null!;
    
    /// <summary>
    /// The verification token.
    /// </summary>
    [Description("The verification token")]
    [Required]
    public string Token { get; set; } = null!;
}