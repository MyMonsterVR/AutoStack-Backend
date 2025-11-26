namespace AutoStack.Infrastructure.Security.Models;

/// <summary>
/// Configuration settings for authentication cookies
/// </summary>
public class CookieSettings
{
    /// <summary>
    /// Gets or sets the name of the access token cookie
    /// </summary>
    public string AccessTokenCookieName { get; set; } = "access_token";

    /// <summary>
    /// Gets or sets the name of the refresh token cookie
    /// </summary>
    public string RefreshTokenCookieName { get; set; } = "refresh_token";

    /// <summary>
    /// Gets or sets whether cookies should be HttpOnly (not accessible via JavaScript)
    /// </summary>
    public bool UseHttpOnlyCookies { get; set; } = true;

    /// <summary>
    /// Gets or sets whether cookies should only be sent over HTTPS
    /// </summary>
    public bool UseSecureCookies { get; set; } = true;

    /// <summary>
    /// Gets or sets the SameSite attribute for cookies (Strict, Lax, or None)
    /// </summary>
    public string SameSite { get; set; } = "Strict";
}
