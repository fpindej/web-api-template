using System.ComponentModel;

namespace MyProject.WebApi.Features.Authentication.Dtos.Refresh;

/// <summary>
/// Represents the response from a successful token refresh operation.
/// </summary>
public class RefreshResponse
{
    /// <summary>
    /// The new JWT access token for authenticating API requests.
    /// </summary>
    [Description("The new JWT access token for authenticating API requests")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// The new refresh token to use for future refresh operations.
    /// </summary>
    [Description("The new refresh token to use for future refresh operations")]
    public string RefreshToken { get; init; } = string.Empty;
}
