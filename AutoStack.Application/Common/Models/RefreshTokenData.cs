namespace AutoStack.Application.Common.Models;

public class RefreshTokenData
{
    public string Token { get; set; } = string.Empty;
    public Guid UserId { get; set; } = Guid.Empty;
    public DateTime? ExpiresAt { get; set; }
    public DateTime? CreatedAt { get; set; }
}