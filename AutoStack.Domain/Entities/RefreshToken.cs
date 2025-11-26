using System.Security.Cryptography;
using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

/// <summary>
/// Represents a refresh token used for authentication token renewal
/// </summary>
public class RefreshToken : Entity<Guid>
{
    /// <summary>
    /// Gets the token string value
    /// </summary>
    public string Token { get; init; } = String.Empty;

    /// <summary>
    /// Gets the ID of the user this token belongs to
    /// </summary>
    public Guid UserId { get; init; }

    /// <summary>
    /// Gets the Unix timestamp when this token expires
    /// </summary>
    public int ExpiresAt  { get; init; }
    
    public RefreshToken()
    {}

    private RefreshToken(Guid id, string token, Guid userId, int expires) : base(id)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expires;
    }

    /// <summary>
    /// Creates a new refresh token with validation
    /// </summary>
    /// <param name="token">The token string value</param>
    /// <param name="userId">The ID of the user this token belongs to</param>
    /// <param name="expiresAt">The Unix timestamp when this token expires</param>
    /// <returns>A new RefreshToken instance</returns>
    /// <exception cref="ArgumentException">Thrown when token is null/empty, userId is empty, or expiresAt is not in the future</exception>
    public static RefreshToken Create(string token, Guid userId, int expiresAt)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new ArgumentException("Token cannot be null or empty", nameof(token));
        }

        if (userId == Guid.Empty)
        {
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        }

        var currentUnixTime = (int)DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
        if (expiresAt <= currentUnixTime)
        {
            throw new ArgumentException("Expiration time must be in the future", nameof(expiresAt));
        }

        return new RefreshToken(Guid.NewGuid(), token, userId, expiresAt);
    }
}