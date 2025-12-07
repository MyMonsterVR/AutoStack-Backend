namespace AutoStack.Application.DTOs.Login;

/// <summary>
/// Response DTO containing authentication tokens for a successful login
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// Indicates if 2FA verification is required to complete login
    /// </summary>
    public bool RequiresTwoFactor { get; set; }

    /// <summary>
    /// Temporary token for 2FA verification (only present if RequiresTwoFactor is true)
    /// </summary>
    public string? TwoFactorToken { get; set; }

    /// <summary>
    /// Gets or sets the JWT access token for API authentication (null if RequiresTwoFactor is true)
    /// </summary>
    public string? AccessToken { get; set; }

    /// <summary>
    /// Gets or sets the refresh token for obtaining new access tokens (null if RequiresTwoFactor is true)
    /// </summary>
    public string? RefreshToken { get; set; }
}