using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using AutoStack.Infrastructure.Security.Models;

namespace AutoStack.Infrastructure.Security;

public interface ICookieManager
{
    void SetAccessTokenCookie(HttpContext httpContext, string accessToken, int expirationMinutes);
    void SetRefreshTokenCookie(HttpContext httpContext, string refreshToken, int expirationDays);
    string? GetAccessTokenFromCookie(HttpContext httpContext);
    string? GetRefreshTokenFromCookie(HttpContext httpContext);
    void ClearAuthCookies(HttpContext httpContext);
}

public class CookieManager(IOptions<CookieSettings> cookieSettings, IOptions<JwtSettings> jwtSettings)
    : ICookieManager
{
    private readonly CookieSettings _cookieSettings = cookieSettings.Value;
    private readonly JwtSettings _jwtSettings = jwtSettings.Value;

    /// <summary>
    /// Force sets the access token to the cookie of a response
    /// </summary>
    /// <param name="httpContext">The users http context</param>
    /// <param name="accessToken">The users access token</param>
    /// <param name="expirationMinutes">Time before access token expires</param>
    public void SetAccessTokenCookie(HttpContext httpContext, string accessToken, int expirationMinutes)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = _cookieSettings.UseHttpOnlyCookies,
            Secure = _cookieSettings.UseSecureCookies,
            SameSite = ParseSameSite(_cookieSettings.SameSite),
            Expires = DateTimeOffset.UtcNow.AddMinutes(expirationMinutes),
            Path = "/"
        };

        httpContext.Response.Cookies.Append(
            _cookieSettings.AccessTokenCookieName,
            accessToken,
            cookieOptions
        );
    }

    /// <summary>
    /// Force sets the refresh token to the cookie of a response
    /// </summary>
    /// <param name="httpContext">The users http context</param>
    /// <param name="refreshToken">The users refresh token</param>
    /// <param name="expirationDays">Days before refresh token expires</param>
    public void SetRefreshTokenCookie(HttpContext httpContext, string refreshToken, int expirationDays)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = _cookieSettings.UseHttpOnlyCookies,
            Secure = _cookieSettings.UseSecureCookies,
            SameSite = ParseSameSite(_cookieSettings.SameSite),
            Expires = DateTimeOffset.UtcNow.AddDays(expirationDays),
            Path = "/"
        };

        httpContext.Response.Cookies.Append(
            _cookieSettings.RefreshTokenCookieName,
            refreshToken,
            cookieOptions
        );
    }

    /// <summary>
    /// Gets the access token from the users cookies
    /// </summary>
    /// <param name="httpContext">The users http context</param>
    /// <returns>The access token</returns>
    public string? GetAccessTokenFromCookie(HttpContext httpContext)
    {
        return httpContext.Request.Cookies[_cookieSettings.AccessTokenCookieName];
    }

    /// <summary>
    /// Gets the refresh token from the users cookies
    /// </summary>
    /// <param name="httpContext">The users http context</param>
    /// <returns>The refresh token</returns>
    public string? GetRefreshTokenFromCookie(HttpContext httpContext)
    {
        return httpContext.Request.Cookies[_cookieSettings.RefreshTokenCookieName];
    }

    /// <summary>
    /// Clears the authentication cookies from the user
    /// </summary>
    /// <param name="httpContext">The users http context</param>
    public void ClearAuthCookies(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(_cookieSettings.AccessTokenCookieName);
        httpContext.Response.Cookies.Delete(_cookieSettings.RefreshTokenCookieName);
    }

    /// <summary>
    /// Parses a string value to a SameSiteMode enum
    /// </summary>
    /// <param name="sameSite">The SameSite string value (strict, lax, or none)</param>
    /// <returns>The corresponding SameSiteMode enum value</returns>
    private static SameSiteMode ParseSameSite(string sameSite)
    {
        return sameSite.ToLower() switch
        {
            "strict" => SameSiteMode.Strict,
            "lax" => SameSiteMode.Lax,
            "none" => SameSiteMode.None,
            _ => SameSiteMode.Strict
        };
    }
}
