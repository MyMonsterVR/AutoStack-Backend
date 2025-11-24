using AutoStack.Infrastructure.Security;

namespace AutoStack.Presentation.Middleware;

public class CookieAuthenticationMiddleware
{
    private readonly RequestDelegate _next;

    public CookieAuthenticationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, ICookieManager cookieManager)
    {
        if (!context.Request.Headers.ContainsKey("Authorization"))
        {
            var accessToken = cookieManager.GetAccessTokenFromCookie(context);

            if (!string.IsNullOrEmpty(accessToken))
            {
                context.Request.Headers.Append("Authorization", $"Bearer {accessToken}");
            }
        }

        await _next(context);
    }
}

public static class CookieAuthenticationMiddlewareExtensions
{
    public static IApplicationBuilder UseCookieAuthentication(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CookieAuthenticationMiddleware>();
    }
}
