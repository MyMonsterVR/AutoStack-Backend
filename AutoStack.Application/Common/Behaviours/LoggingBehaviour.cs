using System.Diagnostics;
using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Enums;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoStack.Application.Common.Behaviours;

/// <summary>
/// Pipeline behavior that logs request and response information for all requests.
/// Logs the request name, timestamp, duration, and success/failure status.
/// Enhanced with database logging for major events and errors.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;
    private readonly IAuditLogService _auditLogService;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingBehaviour{TRequest,TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    /// <param name="auditLogService">The audit log service for database logging.</param>
    public LoggingBehaviour(
        ILogger<LoggingBehaviour<TRequest, TResponse>> logger,
        IAuditLogService auditLogService)
    {
        _logger = logger;
        _auditLogService = auditLogService;
    }

    /// <summary>
    /// Logs request and response information. 
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next behavior or handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var timestamp = DateTime.UtcNow;

        _logger.LogInformation(
            "Handling {RequestName} at {Timestamp}",
            requestName,
            timestamp);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Completed {RequestName} in {Duration}ms with success",
                requestName,
                stopwatch.ElapsedMilliseconds);

            // Database logging - only for major events (not all requests)
            if (IsMajorEvent(requestName))
            {
                try
                {
                    await _auditLogService.LogAsync(new AuditLogRequest
                    {
                        Level = Domain.Enums.LogLevel.Information,
                        Category = GetCategory(requestName),
                        Message = $"Successfully completed {requestName}",
                        DurationMs = stopwatch.ElapsedMilliseconds
                    }, cancellationToken);
                }
                catch (Exception loggingEx)
                {
                    _logger.LogError(loggingEx, "Failed to log success to database");
                }
            }

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Completed {RequestName} in {Duration}ms with failure: {ErrorMessage}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            // Database logging - always log errors
            try
            {
                await _auditLogService.LogAsync(new AuditLogRequest
                {
                    Level = Domain.Enums.LogLevel.Error,
                    Category = GetCategory(requestName),
                    Message = $"Failed to complete {requestName}: {ex.Message}",
                    Exception = ex,
                    DurationMs = stopwatch.ElapsedMilliseconds
                }, cancellationToken);
            }
            catch (Exception loggingEx)
            {
                _logger.LogError(loggingEx, "Failed to log error to database");
            }

            throw;
        }
    }

    /// <summary>
    /// Determines if the request is a major event that should be logged to the database
    /// </summary>
    private static bool IsMajorEvent(string requestName)
    {
        return requestName switch
        {
            "LoginCommand" => true,
            "RegisterCommand" => true,
            "RefreshTokenCommand" => true,
            "CreateStackCommand" => true,
            "DeleteStackCommand" => true,
            "EditUserCommand" => true,
            _ => false
        };
    }

    /// <summary>
    /// Gets the log category for a given request name
    /// </summary>
    private static LogCategory GetCategory(string requestName)
    {
        return requestName switch
        {
            "LoginCommand" or "RegisterCommand" or "RefreshTokenCommand"
                => LogCategory.Authentication,
            "CreateStackCommand" or "DeleteStackCommand" or "EditStackCommand"
                => LogCategory.Stack,
            "EditUserCommand" or "DeleteUserCommand"
                => LogCategory.User,
            _ => LogCategory.System
        };
    }
}