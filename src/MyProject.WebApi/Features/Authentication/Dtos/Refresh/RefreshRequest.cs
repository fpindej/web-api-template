using System.ComponentModel;
using JetBrains.Annotations;

namespace MyProject.WebApi.Features.Authentication.Dtos.Refresh;

/// <summary>
/// Represents a request to refresh an expired access token.
/// </summary>
public class RefreshRequest
{
    /// <summary>
    /// The expired access token that needs to be refreshed.
    /// </summary>
    [Description("The expired access token that needs to be refreshed")]
    public string AccessToken { get; [UsedImplicitly] init; } = string.Empty;
    
    /// <summary>
    /// The refresh token received during login or previous refresh operation.
    /// </summary>
    [Description("The refresh token received during login or previous refresh operation")]
    public string RefreshToken { get; [UsedImplicitly] init; } = string.Empty;
}
