namespace MyProject.Infrastructure.Features.Authentication;

/// <summary>
/// Token metadata class to hold parsed refresh token data
/// </summary>
internal class RefreshTokenMetadata
{
    public string? TokenValue { get; set; }

    public DateTime ExpirationTime { get; set; } = DateTime.MinValue;

    public string? UserId { get; set; }

    public string? SecurityStamp { get; set; }

    /// <summary>
    /// Returns a string representation of the token metadata.
    /// </summary>
    /// <returns>Returns a string representation of the token metadata. The : character is used as a delimiter that doesn't collide with Base64 encoding used in the token generated in <see cref="Authentication.Services.JwtTokenProvider.GenerateRefreshToken"/>.</returns>
    public override string ToString()
    {
        return $"token:{TokenValue},expires:{ExpirationTime.Ticks},userId:{UserId},stamp:{SecurityStamp}";
    }
}