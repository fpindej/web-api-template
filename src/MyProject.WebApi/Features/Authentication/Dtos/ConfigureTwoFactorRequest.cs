using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MyProject.WebApi.Features.Authentication.Dtos;

/// <summary>
/// DTO for configuring two-factor authentication.
/// </summary>
[Description("Configure two-factor authentication request DTO")]
public class ConfigureTwoFactorRequest
{
    /// <summary>
    /// Whether to enable or disable two-factor authentication.
    /// </summary>
    [Description("Whether to enable or disable two-factor authentication")]
    [Required]
    public bool Enable { get; set; }
}
