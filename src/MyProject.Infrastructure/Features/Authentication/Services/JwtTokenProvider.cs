using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using MyProject.Infrastructure.Features.Authentication.Models;
using MyProject.Infrastructure.Features.Authentication.Options;

namespace MyProject.Infrastructure.Features.Authentication.Services;

public class JwtTokenProvider(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtOptions> jwtOptions,
    TimeProvider timeProvider) : ITokenProvider
{
    private readonly JwtOptions _jwtOptions = jwtOptions.Value;

    public async Task<string> GenerateAccessToken(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expires = timeProvider.GetUtcNow().UtcDateTime.AddMinutes(_jwtOptions.ExpiresInMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            // Add the security stamp to verify token validity during refresh
            new(_jwtOptions.SecurityStampClaimType, user.SecurityStamp ?? string.Empty)
        };

        var userRoles = await userManager.GetRolesAsync(user);
        claims.AddRange(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            _jwtOptions.Issuer,
            _jwtOptions.Audience,
            claims,
            expires: expires,
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<string> GenerateRefreshToken(ApplicationUser user, CancellationToken cancellationToken = default)
    {
        var expires = timeProvider.GetUtcNow().UtcDateTime.AddDays(_jwtOptions.RefreshToken.ExpiresInDays);

        var secureToken = GenerateSecureToken(32);
        var base64Token = Convert.ToBase64String(secureToken);

        var tokenMetadata = new RefreshTokenMetadata
        {
            TokenValue = base64Token,
            ExpirationTime = expires,
            UserId = user.Id,
            SecurityStamp = user.SecurityStamp
        };

        var encryptedToken = EncryptTokenMetadata(tokenMetadata.ToString());

        await userManager.SetAuthenticationTokenAsync(
            user: user,
            loginProvider: _jwtOptions.RefreshToken.ProviderName,
            tokenName: _jwtOptions.RefreshToken.Purpose,
            tokenValue: encryptedToken);

        return encryptedToken;
    }

    public async Task<string> DecryptRefreshTokenAsync(string cipherText)
    {
        var buffer = Convert.FromBase64String(cipherText);

        using var aes = Aes.Create();
        aes.Key = GetEncryptionKey();
        aes.IV = GetEncryptionIV();

        using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream(buffer);
        await using var cryptoStream = new CryptoStream(memoryStream, decryptor, CryptoStreamMode.Read);
        using var streamReader = new StreamReader(cryptoStream);

        return await streamReader.ReadToEndAsync();
    }

    private static byte[] GenerateSecureToken(int length)
    {
        var randomBytes = new byte[length];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);

        return randomBytes;
    }

    /// <summary>
    /// Encrypts the token metadata using AES encryption.
    /// </summary>
    /// <param name="plainText">The plaintext token metadata to encrypt.</param>
    /// <returns>A Base64 encoded encrypted string.</returns>
    private string EncryptTokenMetadata(string plainText)
    {
        using var aes = Aes.Create();
        aes.Key = GetEncryptionKey();
        aes.IV = GetEncryptionIV();

        using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
        using var memoryStream = new MemoryStream();
        using var cryptoStream = new CryptoStream(memoryStream, encryptor, CryptoStreamMode.Write);
        using (var streamWriter = new StreamWriter(cryptoStream))
        {
            streamWriter.Write(plainText);
        }

        return Convert.ToBase64String(memoryStream.ToArray());
    }

    /// <summary>
    /// Derives an encryption key from the JWT signing key.
    /// </summary>
    /// <returns>A byte array representing the encryption key.</returns>
    private byte[] GetEncryptionKey()
    {
        using var deriveBytes = new Rfc2898DeriveBytes(
            _jwtOptions.Key,
            salt: "RefreshTokenSalt"u8.ToArray(),
            iterations: 10_000,
            HashAlgorithmName.SHA256);

        return deriveBytes.GetBytes(32); // 256 bits key for AES-256
    }

    /// <summary>
    /// Gets a fixed initialization vector for AES encryption.
    /// </summary>
    /// <returns>A byte array representing the initialization vector.</returns>
    private byte[] GetEncryptionIV()
    {
        using var deriveBytes = new Rfc2898DeriveBytes(
            _jwtOptions.Key,
            salt: "RefreshTokenIV"u8.ToArray(),
            iterations: 10_000,
            HashAlgorithmName.SHA256);

        return deriveBytes.GetBytes(16); // 128 bits IV for AES
    }
}
