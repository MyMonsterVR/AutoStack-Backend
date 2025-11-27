using System.Text.Json;
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
