using System.Security.Cryptography;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using MyProject.Infrastructure.Features.Authentication.Models;
using MyProject.Infrastructure.Features.Authentication.Options;

namespace MyProject.Infrastructure.Features.Authentication.Services;

public class CustomRefreshTokenProvider(
    IDataProtectionProvider dataProtectionProvider,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider)
    : IUserTwoFactorTokenProvider<ApplicationUser>
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public Task<bool> CanGenerateTwoFactorTokenAsync(UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        return Task.FromResult(!string.IsNullOrEmpty(user.Id));
    }

    public Task<string> GenerateAsync(string purpose, UserManager<ApplicationUser> manager, ApplicationUser user)
    {
        // Create a unique token with random data, the user ID, and timestamp
        var tokenBytes = new byte[64];
        RandomNumberGenerator.Fill(tokenBytes);

        var timestamp = timeProvider.GetUtcNow().UtcDateTime.AddDays(_jwtOptions.RefreshToken.ExpiresInDays).Ticks;
        var timestampBytes = BitConverter.GetBytes(timestamp);

        // Combine all data
        var combinedBytes = new byte[tokenBytes.Length + timestampBytes.Length];
        Buffer.BlockCopy(tokenBytes, 0, combinedBytes, 0, tokenBytes.Length);
        Buffer.BlockCopy(timestampBytes, 0, combinedBytes, tokenBytes.Length, timestampBytes.Length);

        // Encrypt the data - use the purpose parameter instead of hardcoded value
        var protector = dataProtectionProvider.CreateProtector(purpose);
        var protectedData = protector.Protect(combinedBytes);

        // Return as base64 string
        return Task.FromResult(Convert.ToBase64String(protectedData));
    }

    public Task<bool> ValidateAsync(string purpose, string token, UserManager<ApplicationUser> manager,
        ApplicationUser user)
    {
        try
        {
            // Decrypt the token - use the purpose parameter instead of hardcoded value
            var protector = dataProtectionProvider.CreateProtector(purpose);
            var unprotectedData = protector.Unprotect(Convert.FromBase64String(token));

            // Extract timestamp
            var timestampBytes = new byte[8];
            Buffer.BlockCopy(unprotectedData, unprotectedData.Length - 8, timestampBytes, 0, 8);
            var expiryTicks = BitConverter.ToInt64(timestampBytes, 0);
            var expiryTime = new DateTime(expiryTicks, DateTimeKind.Utc);

            // Check if token is expired
            return Task.FromResult(timeProvider.GetUtcNow().UtcDateTime < expiryTime);
        }
        catch
        {
            // If any exception occurs during validation, consider the token invalid
            return Task.FromResult(false);
        }
    }
}
