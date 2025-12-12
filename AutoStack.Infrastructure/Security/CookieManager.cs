using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using AutoStack.Infrastructure.Security.Models;

namespace AutoStack.Infrastructure.Security;

/// <summary>
/// Service for managing authentication cookies (access and refresh tokens)
/// </summary>
public interface ICookieManager
{
    /// <summary>
    /// Sets the access token cookie in the HTTP response
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    /// <param name="accessToken">The access token value</param>
    /// <param name="expirationMinutes">Time before access token expires</param>
    void SetAccessTokenCookie(HttpContext httpContext, string accessToken, int expirationMinutes);

    /// <summary>
    /// Sets the refresh token cookie in the HTTP response
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    /// <param name="refreshToken">The refresh token value</param>
    /// <param name="expirationDays">Days before refresh token expires</param>
    void SetRefreshTokenCookie(HttpContext httpContext, string refreshToken, int expirationDays);

    /// <summary>
    /// Gets the access token from the request cookies
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    /// <returns>The access token if present, null otherwise</returns>
    string? GetAccessTokenFromCookie(HttpContext httpContext);

    /// <summary>
    /// Gets the refresh token from the request cookies
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    /// <returns>The refresh token if present, null otherwise</returns>
    string? GetRefreshTokenFromCookie(HttpContext httpContext);

    /// <summary>
    /// Clears the authentication cookies from the response
    /// </summary>
    /// <param name="httpContext">The HTTP context</param>
    void ClearAuthCookies(HttpContext httpContext);
}

/// <summary>
/// Implementation of cookie manager for authentication cookies
/// </summary>
public class CookieManager : ICookieManager
{
    private readonly CookieSettings _cookieSettings;

    public CookieManager(IOptions<CookieSettings> cookieSettings, IOptions<JwtSettings> jwtSettings)
    {
        _cookieSettings = cookieSettings.Value;
    }

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
