using System.Text.Json;
using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Enums;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace AutoStack.Presentation.Middleware;

public class GlobalExceptionHandlerMiddleware(
    ILogger<GlobalExceptionHandlerMiddleware> logger,
    IHostEnvironment environment)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An unhandled exception occurred: {Message}", exception.Message);

        // Database logging
        try
        {
            var auditLogService = httpContext.RequestServices.GetService<IAuditLogService>();

            if (auditLogService != null)
            {
                await auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = Domain.Enums.LogLevel.Critical,
                    Category = LogCategory.System,
                    Message = $"Unhandled exception: {exception.Message}",
                    Exception = exception,
                    StatusCode = GetStatusCode(exception)
                }, cancellationToken);
            }
        }
        catch (Exception loggingEx)
        {
            // Never let logging failures crash the app
            logger.LogError(loggingEx, "Failed to log exception to database");
        }

        var (statusCode, message) = exception switch
        {
            BadHttpRequestException => (StatusCodes.Status400BadRequest, "Invalid request format. Please check your JSON data."),
            JsonException => (StatusCodes.Status400BadRequest, "Invalid JSON format in request body."),
            DbUpdateException dbEx when dbEx.InnerException?.Message.Contains("duplicate key") == true
                => (StatusCodes.Status400BadRequest, "Unable to process your request. Please verify your data."),
            DbUpdateException => (StatusCodes.Status400BadRequest, "Unable to save changes. Please check your data."),
            _ => (StatusCodes.Status500InternalServerError, "An internal server error occurred.")
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        // Only include detailed error in development
        var response = new
        {
            success = false,
            message = message,
            error = environment.IsDevelopment() ? exception.Message : null as string,
            stackTrace = environment.IsDevelopment() ? exception.StackTrace : null as string
        };

        await httpContext.Response.WriteAsJsonAsync(response, cancellationToken);

        return true; // Exception handled
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            BadHttpRequestException => StatusCodes.Status400BadRequest,
            JsonException => StatusCodes.Status400BadRequest,
            DbUpdateException => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }
}

public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IServiceCollection AddGlobalExceptionHandler(this IServiceCollection services)
    {
        services.AddExceptionHandler<GlobalExceptionHandlerMiddleware>();
        services.AddProblemDetails();
        return services;
    }
}
