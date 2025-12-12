using System.Security.Claims;
using AutoStack.Domain.Entities;

namespace AutoStack.Application.Common.Interfaces.Auth;

/// <summary>
/// Service for generating and verifying JWT tokens for authentication
/// </summary>
public interface IToken
{
    /// <summary>
    /// Generates an access token
    /// </summary>
    /// <param name="userId">The users id</param>
    /// <param name="username">The users username</param>
    /// <param name="email">The users email</param>
    /// <returns>A JWT access token string</returns>
    string GenerateAccessToken(Guid userId, string username, string email);

    /// <summary>
    /// Generates a refresh token
    /// </summary>
    /// <param name="userId">The users id</param>
    /// <returns>Refresh token data</returns>
    RefreshToken GenerateRefreshToken(Guid userId);

    /// <summary>
    /// Verifies a token is valid
    /// </summary>
    /// <param name="token">The users auth token</param>
    /// <returns>The Claims principal and null if invalid</returns>
    ClaimsPrincipal? VerifyToken(string token);

    /// <summary>
    /// Generates a short-lived token for two-factor authentication verification
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <returns>A JWT token for 2FA verification</returns>
    string GenerateTwoFactorToken(Guid userId);

    /// <summary>
    /// Verifies a two-factor authentication token and extracts the user ID
    /// </summary>
    /// <param name="token">The 2FA token to verify</param>
    /// <returns>The user ID if valid, null otherwise</returns>
    Guid? VerifyTwoFactorToken(string token);

    /// <summary>
    /// Generates a token for two-factor authentication setup containing the secret key
    /// </summary>
    /// <param name="userId">The user's ID</param>
    /// <param name="secretKey">The TOTP secret key</param>
    /// <returns>A JWT token containing the setup information</returns>
    string GenerateSetupToken(Guid userId, string secretKey);

    /// <summary>
    /// Verifies a setup token and extracts the user ID and secret key
    /// </summary>
    /// <param name="token">The setup token to verify</param>
    /// <returns>A tuple of (userId, secretKey) if valid, null otherwise</returns>
    (Guid, string)? VerifySetupToken(string token);
}