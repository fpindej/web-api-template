using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyProject.WebApi.Features.Authentication.Dtos.Password;

/// <summary>
/// DTO for forgot password request.
/// </summary>
[Description("Forgot password request DTO")]
public class ForgotPasswordRequest
{
    /// <summary>
    /// The email address of the account.
    /// </summary>
    [Description("The email address of the account")]
    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;
}
