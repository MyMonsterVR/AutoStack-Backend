using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

public class RefreshToken : Entity<Guid>
{
    public string? Token { get; init; }
    public Guid UserId { get; set; }
    public DateTime ExpiresAt { get; set; }
    
    public RefreshToken()
    {}

    private RefreshToken(string token, Guid userId, DateTime expires)
    {
        Token = token;
        UserId = userId;
        ExpiresAt = expires;
    }
}