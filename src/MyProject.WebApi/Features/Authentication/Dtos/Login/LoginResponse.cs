using System.ComponentModel;

namespace MyProject.WebApi.Features.Authentication.Dtos.Login;

/// <summary>
/// Represents the response from a successful login operation.
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// The JWT access token for authenticating API requests.
    /// </summary>
    [Description("The JWT access token for authenticating API requests")]
    public string AccessToken { get; init; } = string.Empty;

    /// <summary>
    /// The refresh token used to obtain a new access token when it expires.
    /// </summary>
    [Description("The refresh token used to obtain a new access token when it expires")]
    public string RefreshToken { get; init; } = string.Empty;
}
