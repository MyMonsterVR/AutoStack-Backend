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

    public string? GetAccessTokenFromCookie(HttpContext httpContext)
    {
        return httpContext.Request.Cookies[_cookieSettings.AccessTokenCookieName];
    }

    public string? GetRefreshTokenFromCookie(HttpContext httpContext)
    {
        return httpContext.Request.Cookies[_cookieSettings.RefreshTokenCookieName];
    }

    public void ClearAuthCookies(HttpContext httpContext)
    {
        httpContext.Response.Cookies.Delete(_cookieSettings.AccessTokenCookieName);
        httpContext.Response.Cookies.Delete(_cookieSettings.RefreshTokenCookieName);
    }

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
