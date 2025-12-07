using System.Threading.RateLimiting;
using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Enums;
using Microsoft.AspNetCore.RateLimiting;

namespace AutoStack.Presentation.Extensions;

public static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            // Global rate limiter for all endpoints
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    factory: partition => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Strict rate limiter for authentication endpoint
            options.AddFixedWindowLimiter("login", options =>
            {
                options.PermitLimit = 5;
                options.Window = TimeSpan.FromMinutes(1);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 0;
            });

            // Rate limiter for registration
            options.AddFixedWindowLimiter("register", options =>
            {
                options.PermitLimit = 3;
                options.Window = TimeSpan.FromHours(1);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 0;
            });

            // Rate limiter for token refresh
            options.AddFixedWindowLimiter("refresh", options =>
            {
                options.PermitLimit = 10;
                options.Window = TimeSpan.FromMinutes(1);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 0;
            });

            // Rate limiter for 2FA verification
            options.AddFixedWindowLimiter("2fa-verify", options =>
            {
                options.PermitLimit = 5;
                options.Window = TimeSpan.FromMinutes(5);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 0;
            });

            // Rate limiter for recovery code usage
            options.AddFixedWindowLimiter("2fa-recovery", options =>
            {
                options.PermitLimit = 3;
                options.Window = TimeSpan.FromMinutes(15);
                options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                options.QueueLimit = 0;
            });

            // Rate limiter for sensitive 2FA operations (disable, regenerate codes)
            options.AddPolicy("2fa-sensitive", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(
                    partitionKey: httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown",
                    factory: _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 3,
                        Window = TimeSpan.FromHours(1),
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                        QueueLimit = 0
                    }));

            // Rate limiter for CLI download tracking - dynamic based on authentication
            options.AddPolicy("cli-track", httpContext =>
            {
                var isAuthenticated = httpContext.User?.Identity?.IsAuthenticated ?? false;

                if (isAuthenticated)
                {
                    // Authenticated users get relaxed limits (50 per 5 minutes)
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 50,
                            Window = TimeSpan.FromMinutes(5),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                }
                else
                {
                    // Unauthenticated users get strict IP-based limits (5 per 5 minutes)
                    return RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = 5,
                            Window = TimeSpan.FromMinutes(5),
                            QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                            QueueLimit = 0
                        });
                }
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                // Log rate limit exceeded
                try
                {
                    var auditLogService = context.HttpContext.RequestServices
                        .GetService<IAuditLogService>();

                    if (auditLogService != null)
                    {
                        await auditLogService.LogAsync(new AuditLogRequest
                        {
                            Level = Domain.Enums.LogLevel.Warning,
                            Category = LogCategory.RateLimiting,
                            Message = "Rate limit exceeded"
                        }, cancellationToken);
                    }
                }
                catch
                {
                    // Ignore logging failures - don't let them block rate limiting response
                }

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    success = false,
                    message = "Too many requests. Please try again later."
                }, cancellationToken);
            };
        });

        return services;
    }
}
