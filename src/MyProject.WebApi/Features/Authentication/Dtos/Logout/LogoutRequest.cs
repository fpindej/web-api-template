using System.ComponentModel;
using JetBrains.Annotations;

namespace MyProject.WebApi.Features.Authentication.Dtos.Logout;

/// <summary>
/// Represents a request to logout a user and invalidate their refresh token.
/// </summary>
public class LogoutRequest
{
    /// <summary>
    /// The refresh token to be invalidated during logout.
    /// </summary>
    [Description("The refresh token to be invalidated during logout")]
    public string RefreshToken { get; [UsedImplicitly] init; } = string.Empty;
}
