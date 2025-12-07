using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Security.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AutoStack.Infrastructure.Security;

public class Token : IToken
{
    private readonly JwtSettings _jwtSettings;

    private const string TokenTypeClaimName = "token_type";
    private const string SecretKeyClaimName = "secret_key";
    private const string TwoFactorTokenType = "two-factor";
    private const string SetupTokenType = "setup";

    // Token expiration constants (in minutes)
    private const int TwoFactorTokenExpirationMinutes = 5;
    private const int SetupTokenExpirationMinutes = 10;

    public Token(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }
    
    /// <summary>
    /// Generates a JWT access token for the user with their claims
    /// </summary>
    /// <param name="userId">The id of the user</param>
    /// <param name="username">The username for the user</param>
    /// <param name="email">The email of the user</param>
    /// <returns>The JWT access token as a string</returns>
    public string GenerateAccessToken(Guid userId, string username, string email)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.NameId, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(), 
                ClaimValueTypes.Integer64)
        };
        
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
    
    /// <summary>
    /// Generates a cryptographically secure refresh token for the user
    /// </summary>
    /// <param name="userId">The id of the user</param>
    /// <returns>A RefreshToken object containing the token string, user id, and expiration</returns>
    public RefreshToken GenerateRefreshToken(Guid userId)
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        var token = Convert.ToBase64String(randomNumber);
        
        var unixTimestamp = (int)DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays).Subtract(DateTime.UnixEpoch).TotalSeconds;
        return RefreshToken.Create(token, userId, unixTimestamp);
    }
    
    /// <summary>
    /// Verifies a JWT token
    /// </summary>
    /// <param name="token">JWT token</param>
    /// <returns>Principals from token; null if token is invalid</returns>
    public ClaimsPrincipal? VerifyToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = _jwtSettings.Audience,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);

            if (validatedToken is JwtSecurityToken jwtToken &&
                !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
            {
                return null;
            }

            return principal;
        }
        catch (Exception)
        {
            // Return null for any token validation failure (expired, invalid signature, malformed, etc.)
            return null;
        }
    }

    /// <summary>
    /// Generates a short-lived token for 2FA verification (used between login steps)
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>JWT token valid for 5 minutes</returns>
    public string GenerateTwoFactorToken(Guid userId)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.NameId, userId.ToString()),
            new Claim(TokenTypeClaimName, TwoFactorTokenType),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: TwoFactorTokenType,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(TwoFactorTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a two-factor token and extracts the user ID
    /// </summary>
    /// <param name="token">The 2FA token</param>
    /// <returns>User ID if valid, null otherwise</returns>
    public Guid? VerifyTwoFactorToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = TwoFactorTokenType,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            var tokenType = principal.FindFirst(TokenTypeClaimName)?.Value;
            if (tokenType != TwoFactorTokenType)
                return null;

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.NameId)?.Value
                ?? principal.FindFirst("nameid")?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId))
                return userId;

            return null;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Generates a setup token containing the TOTP secret for the setup process
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="secretKey">The TOTP secret key (plain text)</param>
    /// <returns>JWT token valid for 10 minutes</returns>
    public string GenerateSetupToken(Guid userId, string secretKey)
    {
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(JwtRegisteredClaimNames.NameId, userId.ToString()),
            new Claim(TokenTypeClaimName, SetupTokenType),
            new Claim(SecretKeyClaimName, secretKey),
            new Claim(JwtRegisteredClaimNames.Iat,
                new DateTimeOffset(DateTime.UtcNow).ToUnixTimeSeconds().ToString(),
                ClaimValueTypes.Integer64)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: SetupTokenType,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: DateTime.UtcNow.AddMinutes(SetupTokenExpirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    /// <summary>
    /// Validates a setup token and extracts user ID and secret key
    /// </summary>
    /// <param name="token">The setup token</param>
    /// <returns>Tuple of (userId, secretKey) if valid, null otherwise</returns>
    public (Guid, string)? VerifySetupToken(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);

        try
        {
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateIssuer = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidateAudience = true,
                ValidAudience = SetupTokenType,
                ValidateLifetime = true,
                ClockSkew = TimeSpan.Zero,
                RequireExpirationTime = true,
                RequireSignedTokens = true,
            };

            var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

            var tokenType = principal.FindFirst(TokenTypeClaimName)?.Value;
            if (tokenType != SetupTokenType)
                return null;

            var userIdClaim = principal.FindFirst(JwtRegisteredClaimNames.NameId)?.Value
                ?? principal.FindFirst("nameid")?.Value
                ?? principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var secretKey = principal.FindFirst(SecretKeyClaimName)?.Value;

            if (Guid.TryParse(userIdClaim, out var userId) && !string.IsNullOrEmpty(secretKey))
                return (userId, secretKey);

            return null;
        }
        catch
        {
            return null;
        }
    }
}