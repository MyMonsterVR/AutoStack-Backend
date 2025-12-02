namespace AutoStack.Presentation.Middleware;

/// <summary>
/// Middleware to mark requests as CLI requests for rate limiting purposes
/// This helps us apply different rate limits to CLI vs web requests
/// </summary>
public class CliIdentificationMiddleware
{
    private readonly RequestDelegate _next;

    public CliIdentificationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Mark CLI track requests for rate limiting
        if (context.Request.Path.StartsWithSegments("/api/cli/track"))
        {
            // Check if user is authenticated
            var isAuthenticated = context.User?.Identity?.IsAuthenticated ?? false;
            context.Items["IsCliTrackRequest"] = true;
            context.Items["IsAuthenticatedCliRequest"] = isAuthenticated;
        }

        await _next(context);
    }
}

public static class CliIdentificationMiddlewareExtensions
{
    public static IApplicationBuilder UseCliIdentification(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<CliIdentificationMiddleware>();
    }
}
