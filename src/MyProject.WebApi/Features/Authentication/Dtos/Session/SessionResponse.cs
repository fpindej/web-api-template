using System.ComponentModel;

namespace MyProject.WebApi.Features.Authentication.Dtos.Session;

/// <summary>
/// Represents the current user session information returned by the session endpoint.
/// </summary>
public class SessionResponse
{
    /// <summary>
    /// The unique identifier of the user.
    /// </summary>
    [Description("The unique identifier of the user")]
    public string UserId { get; init; } = string.Empty;

    /// <summary>
    /// The username of the authenticated user.
    /// </summary>
    [Description("The username of the authenticated user")]
    public string Username { get; init; } = string.Empty;

    /// <summary>
    /// The email address of the authenticated user.
    /// </summary>
    [Description("The email address of the authenticated user")]
    public string Email { get; init; } = string.Empty;

    /// <summary>
    /// The list of roles assigned to the authenticated user.
    /// </summary>
    [Description("The list of roles assigned to the authenticated user")]
    public IEnumerable<string> Roles { get; init; } = [];
}
