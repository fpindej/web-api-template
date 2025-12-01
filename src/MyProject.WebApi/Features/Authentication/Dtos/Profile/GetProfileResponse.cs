using System.ComponentModel;

namespace MyProject.WebApi.Features.Authentication.Dtos.Profile;

/// <summary>
/// DTO for user profile response.
/// </summary>
[Description("User profile response DTO")]
public class GetProfileResponse
{
    /// <summary>
    /// The user ID.
    /// </summary>
    [Description("The user ID")]
    public string Id { get; set; } = null!;
    
    /// <summary>
    /// The username.
    /// </summary>
    [Description("The username")]
    public string Username { get; set; } = null!;
    
    /// <summary>
    /// The email address.
    /// </summary>
    [Description("The email address")]
    public string Email { get; set; } = null!;
    
    /// <summary>
    /// Whether the email is confirmed.
    /// </summary>
    [Description("Whether the email is confirmed")]
    public bool EmailConfirmed { get; set; }
    
    /// <summary>
    /// The phone number.
    /// </summary>
    [Description("The phone number")]
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Whether the phone number is confirmed.
    /// </summary>
    [Description("Whether the phone number is confirmed")]
    public bool PhoneNumberConfirmed { get; set; }
    
    /// <summary>
    /// Whether two-factor authentication is enabled.
    /// </summary>
    [Description("Whether two-factor authentication is enabled")]
    public bool TwoFactorEnabled { get; set; }
    
    /// <summary>
    /// First name of the user.
    /// </summary>
    [Description("First name of the user")]
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Last name of the user.
    /// </summary>
    [Description("Last name of the user")]
    public string? LastName { get; set; }
}
