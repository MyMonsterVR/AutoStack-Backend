using System.Security.Cryptography;
using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

public class RefreshToken : Entity<Guid>
{
    public string Token { get; init; } = String.Empty;
    public Guid UserId { get; init; }

    public int ExpiresAt  { get; init; }
    
    public RefreshToken()
    {}

    private RefreshToken(Guid id, string token, Guid userId, int expires) : base(id)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expires;
    }

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