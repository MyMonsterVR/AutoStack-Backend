using System.Security.Claims;
using AutoStack.Application.Common.Interfaces;

namespace AutoStack.Presentation.Middleware;

/// <summary>
/// Middleware that captures HTTP context (user, IP, endpoint) and stores it in ICurrentUserService
/// This allows MediatR handlers deep in the application layer to access HTTP context for logging
/// </summary>
public class HttpContextLoggingMiddleware
{
    private readonly RequestDelegate _next;

    public HttpContextLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICurrentUserService currentUserService)
    {
        // Extract user from JWT claims (if authenticated)
        var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var username = context.User.FindFirst(ClaimTypes.Name)?.Value;

        Guid? userId = null;
        if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out var parsedUserId))
        {
            userId = parsedUserId;
        }

        // Capture HTTP context
        var ipAddress = context.Connection.RemoteIpAddress?.ToString();
        var userAgent = context.Request.Headers.UserAgent.ToString();
        var httpMethod = context.Request.Method;
        var endpoint = context.Request.Path.Value;

        // Store in scoped service for access by handlers
        currentUserService.SetContext(
            userId,
            username,
            ipAddress,
            userAgent,
            httpMethod,
            endpoint
        );

        await _next(context);
    }
}

/// <summary>
/// Extension method to register HttpContextLoggingMiddleware in the pipeline
/// </summary>
public static class HttpContextLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseHttpContextLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<HttpContextLoggingMiddleware>();
    }
}
