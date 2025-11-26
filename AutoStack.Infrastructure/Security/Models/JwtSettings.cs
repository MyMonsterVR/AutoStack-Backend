namespace AutoStack.Infrastructure.Security;

/// <summary>
/// Configuration settings for JWT token generation and validation
/// </summary>
public class JwtSettings
{
    /// <summary>
    /// Gets or sets the secret key used for signing JWT tokens
    /// </summary>
    public required string SecretKey { get; set; }

    /// <summary>
    /// Gets or sets the issuer claim for JWT tokens
    /// </summary>
    public required string Issuer { get; set; }

    /// <summary>
    /// Gets or sets the audience claim for JWT tokens
    /// </summary>
    public required string Audience { get; set; }

    /// <summary>
    /// Gets or sets the expiration time for access tokens in minutes
    /// </summary>
    public int ExpirationMinutes { get; set; }

    /// <summary>
    /// Gets or sets the expiration time for refresh tokens in days
    /// </summary>
    public int RefreshTokenExpirationDays { get; set; }
}