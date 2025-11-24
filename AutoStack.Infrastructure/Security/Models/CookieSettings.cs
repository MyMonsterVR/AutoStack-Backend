namespace AutoStack.Infrastructure.Security.Models;

public class CookieSettings
{
    public string AccessTokenCookieName { get; set; } = "access_token";
    public string RefreshTokenCookieName { get; set; } = "refresh_token";
    public bool UseHttpOnlyCookies { get; set; } = true;
    public bool UseSecureCookies { get; set; } = true;
    public string SameSite { get; set; } = "Strict";
}
