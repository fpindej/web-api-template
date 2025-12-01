using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyProject.Domain;
using MyProject.Infrastructure.Features.Authentication.Constants;
using MyProject.Infrastructure.Features.Authentication.Models;
using MyProject.Infrastructure.Features.Authentication.Options;

namespace MyProject.Infrastructure.Features.Authentication.Services;

public class AuthenticationService(
    UserManager<ApplicationUser> _userManager,
    SignInManager<ApplicationUser> _signInManager,
    ITokenProvider _tokenProvider,
    TimeProvider _timeProvider,
    IHttpContextAccessor _httpContextAccessor,
    IOptions<JwtOptions> authenticationOptions)
{
    private readonly JwtOptions _jwtOptions = authenticationOptions.Value;

    public async Task<Result> Login(string username, string password, CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByNameAsync(username);

        if (user is null)
        {
            return Result.Failure("Invalid username or password.");
        }

        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);
        if (!signInResult.Succeeded)
        {
            return Result.Failure("Invalid username or password.");
        }

        var accessToken = await _tokenProvider.GenerateAccessToken(user, cancellationToken);
        var refreshToken = await _tokenProvider.GenerateRefreshToken(user, cancellationToken);
        var utcNow = _timeProvider.GetUtcNow();

        SetCookie(
            cookieName: CookieNames.AccessToken,
            content: accessToken,
            options: CreateCookieOptions(expiresAt: utcNow.AddMinutes(_jwtOptions.ExpiresInMinutes)));

        SetCookie(
            cookieName: CookieNames.RefreshToken,
            content: refreshToken,
            options: CreateCookieOptions(expiresAt: utcNow.AddDays(_jwtOptions.RefreshToken.ExpiresInDays)));

        return Result.Success();
    }

    public async Task Logout()
    {
        // Get user ID before clearing cookies
        var userId = TryGetUserIdFromAccessToken();

        DeleteCookie(CookieNames.AccessToken);
        DeleteCookie(CookieNames.RefreshToken);

        if (!string.IsNullOrEmpty(userId))
        {
            await RevokeUserTokens(userId);
        }
    }

    /// <summary>
    /// Refreshes the authentication tokens using only the refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A Result indicating success or failure</returns>
    public async Task<Result> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(refreshToken))
        {
            return Result.Failure("Refresh token is missing.");
        }

        var validationResult = await ValidateRefreshToken(refreshToken);

        if (!validationResult.IsSuccess)
        {
            return Result.Failure(validationResult.Error!);
        }

        var user = validationResult.Value!;

        // Remove the used refresh token immediately to prevent reuse
        await _userManager.RemoveAuthenticationTokenAsync(
            user: user,
            loginProvider: _jwtOptions.RefreshToken.ProviderName,
            tokenName: _jwtOptions.RefreshToken.Purpose);

        var utcNow = _timeProvider.GetUtcNow();
        var newAccessToken = await _tokenProvider.GenerateAccessToken(
            user: user,
            cancellationToken: cancellationToken);

        var newRefreshToken = await _tokenProvider.GenerateRefreshToken(
            user: user,
            cancellationToken: cancellationToken);

        SetCookie(
            cookieName: CookieNames.AccessToken,
            content: newAccessToken,
            options: CreateCookieOptions(expiresAt: utcNow.AddMinutes(_jwtOptions.ExpiresInMinutes)));

        SetCookie(
            cookieName: CookieNames.RefreshToken,
            content: newRefreshToken,
            options: CreateCookieOptions(expiresAt: utcNow.AddDays(_jwtOptions.RefreshToken.ExpiresInDays)));

        return Result.Success();
    }

    /// <summary>
    /// Validates a refresh token and returns the associated user
    /// </summary>
    /// <param name="refreshToken">The refresh token to validate</param>
    /// <returns>A Result containing the user if successful, or a failure message if not</returns>
    private async Task<Result<ApplicationUser>> ValidateRefreshToken(string refreshToken)
    {
        var decryptedToken = await _tokenProvider.DecryptRefreshTokenAsync(refreshToken);

        if (!TryExtractRefreshTokenMetadata(decryptedToken, out var tokenMetadata))
        {
            return Result<ApplicationUser>.Failure("Invalid refresh token format.");
        }

        // Check if the token has expired
        if (tokenMetadata.ExpirationTime < _timeProvider.GetUtcNow().UtcDateTime)
        {
            return Result<ApplicationUser>.Failure("Refresh token has expired.");
        }

        // Direct user lookup using the embedded userId rather than iterating through all users
        if (string.IsNullOrEmpty(tokenMetadata.UserId))
        {
            return Result<ApplicationUser>.Failure("Invalid refresh token: missing user identifier.");
        }

        // Find the user directly using the embedded ID
        var user = await _userManager.FindByIdAsync(tokenMetadata.UserId);
        if (user is null)
        {
            return Result<ApplicationUser>.Failure("User not found.");
        }

        // Verify the security stamp to detect user security changes since token issuance
        if (!string.IsNullOrEmpty(tokenMetadata.SecurityStamp) &&
            tokenMetadata.SecurityStamp != user.SecurityStamp)
        {
            return Result<ApplicationUser>.Failure("Refresh token has been invalidated. Please login again.");
        }

        // Verify the token against what's stored for the user
        var storedToken = await _userManager.GetAuthenticationTokenAsync(
            user: user,
            loginProvider: _jwtOptions.RefreshToken.ProviderName,
            tokenName: _jwtOptions.RefreshToken.Purpose);

        if (storedToken != refreshToken)
        {
            return Result<ApplicationUser>.Failure("Invalid refresh token.");
        }

        // Final validation of user account status
        if (!await _userManager.IsEmailConfirmedAsync(user))
        {
            return Result<ApplicationUser>.Failure("User email is not confirmed.");
        }

        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result<ApplicationUser>.Failure("User is locked out.");
        }

        return Result<ApplicationUser>.Success(user);
    }

    /// <summary>
    /// Extracts all metadata from a refresh token
    /// </summary>
    /// <param name="refreshToken">The refresh token with metadata</param>
    /// <param name="metadata">The extracted token metadata</param>
    /// <returns>True if extraction was successful, false otherwise</returns>
    private static bool TryExtractRefreshTokenMetadata(string refreshToken, out RefreshTokenMetadata metadata)
    {
        metadata = new RefreshTokenMetadata();

        try
        {
            var pairs = refreshToken.Split(',');
            var tokenParts = new Dictionary<string, string?>();

            foreach (var pair in pairs)
            {
                var keyValue = pair.Split(':');
                if (keyValue.Length != 2)
                {
                    return false;
                }

                tokenParts[keyValue[0]] = keyValue[1];
            }

            if (!tokenParts.TryGetValue("token", out var tokenValue))
            {
                return false;
            }
            metadata.TokenValue = tokenValue;

            if (!tokenParts.TryGetValue("expires", out var expiresValue) ||
                !long.TryParse(expiresValue, out var ticks) ||
                ticks < 0)
            {
                return false;
            }

            try
            {
                metadata.ExpirationTime = new DateTime(ticks, DateTimeKind.Utc);
            }
            catch (ArgumentOutOfRangeException)
            {
                return false;
            }

            if (tokenParts.TryGetValue("userId", out var userId))
            {
                metadata.UserId = userId;
            }

            if (tokenParts.TryGetValue("stamp", out var securityStamp))
            {
                metadata.SecurityStamp = securityStamp;
            }

            return true;
        }
        catch
        {
            // Any parsing error means the token is invalid
            return false;
        }
    }

    /// <summary>
    /// Attempts to extract a user ID from the access token cookie
    /// </summary>
    /// <returns>User ID if found, null otherwise</returns>
    private string? TryGetUserIdFromAccessToken()
    {
        if (_httpContextAccessor.HttpContext?.Request.Cookies.TryGetValue(
                key: CookieNames.AccessToken,
                value: out var accessToken) is not true)
        {
            return null;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(accessToken))
            {
                return null;
            }

            var jwtToken = handler.ReadJwtToken(accessToken);

            return jwtToken.Claims.FirstOrDefault(c => c.Type is JwtRegisteredClaimNames.Sub)?.Value;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Revokes all tokens for a specified user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    private async Task RevokeUserTokens(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return;
        }

        await _userManager.RemoveAuthenticationTokenAsync(
            user: user,
            loginProvider: _jwtOptions.RefreshToken.ProviderName,
            tokenName: _jwtOptions.RefreshToken.Purpose);

        // Update security stamp to invalidate all existing tokens issued before logout
        await _userManager.UpdateSecurityStampAsync(user);
    }

    /// <summary>
    /// Gets the current authenticated user information
    /// </summary>
    /// <returns>A Result containing the user if authenticated, or failure if not</returns>
    public async Task<Result<ApplicationUser>> GetCurrentUserAsync()
    {
        var userId = TryGetUserIdFromAccessToken();

        if (string.IsNullOrEmpty(userId))
        {
            return Result<ApplicationUser>.Failure("User is not authenticated.");
        }

        var user = await _userManager.FindByIdAsync(userId);

        if (user is null)
        {
            return Result<ApplicationUser>.Failure("User not found.");
        }

        return Result<ApplicationUser>.Success(user);
    }

    /// <summary>
    /// Gets the roles for a specific user
    /// </summary>
    /// <param name="user">The user to get roles for</param>
    /// <returns>A list of role names</returns>
    public async Task<IList<string>> GetUserRolesAsync(ApplicationUser user)
    {
        return await _userManager.GetRolesAsync(user);
    }

    /// <summary>
    /// Sets the authentication cookie in the HTTP response
    /// </summary>
    /// <param name="cookieName">Name of the cookie</param>
    /// <param name="content">Content to be stored in the cookie</param>
    /// <param name="options">Cookie options</param>
    private void SetCookie(string cookieName, string content, CookieOptions options)
        => _httpContextAccessor.HttpContext?.Response.Cookies.Append(key: cookieName, value: content, options: options);

    /// <summary>
    /// Deletes a specific cookie from the HTTP response
    /// </summary>
    /// <param name="cookieName">Name of the cookie to delete</param>
    private void DeleteCookie(string cookieName)
        => _httpContextAccessor.HttpContext?.Response.Cookies.Delete(
            cookieName,
            new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.None
            });

    private static CookieOptions CreateCookieOptions(DateTimeOffset expiresAt)
        => new()
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = expiresAt
        };
}
