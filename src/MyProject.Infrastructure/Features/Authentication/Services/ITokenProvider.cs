using MyProject.Infrastructure.Features.Authentication.Models;

namespace MyProject.Infrastructure.Features.Authentication.Services;

/// <summary>
/// Provider interface for generating authentication tokens.
/// </summary>
public interface ITokenProvider
{
    /// <summary>
    /// Generates an access token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom to generate the access token.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string representing the generated access token.</returns>
    Task<string> GenerateAccessToken(ApplicationUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Generates a refresh token.
    /// </summary>
    /// <param name="user">The user for whom to generate the refresh token.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A string representing the generated refresh token.</returns>
    Task<string> GenerateRefreshToken(ApplicationUser user, CancellationToken cancellationToken = default);

    /// <summary>
    /// Decrypts the token metadata.
    /// </summary>
    /// <param name="cipherText">The Base64 encoded encrypted string to decrypt.</param>
    /// <returns>The plaintext token metadata.</returns>
    Task<string> DecryptRefreshTokenAsync(string cipherText);
}
