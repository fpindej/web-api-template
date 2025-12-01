using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyProject.WebApi.Features.Authentication.Dtos.Profile;

/// <summary>
/// DTO for updating user profile.
/// </summary>
[Description("Update profile request DTO")]
public class UpdateProfileRequest
{
    /// <summary>
    /// First name of the user.
    /// </summary>
    [Description("First name of the user")]
    [MaxLength(255)]
    public string? FirstName { get; set; }
    
    /// <summary>
    /// Last name of the user.
    /// </summary>
    [Description("Last name of the user")]
    [MaxLength(255)]
    public string? LastName { get; set; }
    
    /// <summary>
    /// The phone number (optional).
    /// </summary>
    [Description("The phone number (optional)")]
    [RegularExpression(@"^(\+\d{1,3})? ?\d{6,14}$", ErrorMessage = "Phone number must be a valid format (e.g. +420123456789)")]
    public string? PhoneNumber { get; set; }
}