namespace AutoStack.Presentation.Middleware;

public class SecurityHeadersMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        // Skip security headers for OpenAPI/Scalar endpoints in development
        if (context.Request.Path.StartsWithSegments("/scalar") ||
            context.Request.Path.StartsWithSegments("/openapi"))
        {
            await next(context);
            return;
        }

        // Remove server information disclosure
        context.Response.Headers.Remove("Server");
        context.Response.Headers.Remove("X-Powered-By");

        // Prevent MIME type sniffing
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");

        // Prevent clickjacking attacks
        context.Response.Headers.Append("X-Frame-Options", "DENY");

        // Legacy XSS protection (defense in depth)
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");

        // Control referrer information
        context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");

        // Content Security Policy - Strict policy for API
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'none'; frame-ancestors 'none'");

        // Permissions Policy - Disable unnecessary browser features
        context.Response.Headers.Append("Permissions-Policy",
            "geolocation=(), microphone=(), camera=(), payment=()");

        // HSTS - Force HTTPS (only add in production or when using HTTPS)
        if (context.Request.IsHttps)
        {
            context.Response.Headers.Append("Strict-Transport-Security",
                "max-age=31536000; includeSubDomains; preload");
        }

        await next(context);
    }
}

public static class SecurityHeadersMiddlewareExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<SecurityHeadersMiddleware>();
    }
}
