using AutoStack.Application.Common.Models;
using AutoStack.Domain.Enums;

namespace AutoStack.Application.Common.Interfaces;

/// <summary>
/// Service for logging audit entries to the database
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Logs an audit entry with the specified details
    /// </summary>
    /// <param name="request">The audit log request</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task LogAsync(AuditLogRequest request, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an informational message
    /// </summary>
    /// <param name="category">The log category</param>
    /// <param name="message">The log message</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task LogInformationAsync(
        LogCategory category,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a warning message
    /// </summary>
    /// <param name="category">The log category</param>
    /// <param name="message">The log message</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task LogWarningAsync(
        LogCategory category,
        string message,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs an error message with optional exception
    /// </summary>
    /// <param name="category">The log category</param>
    /// <param name="message">The log message</param>
    /// <param name="exception">Optional exception details</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task LogErrorAsync(
        LogCategory category,
        string message,
        Exception? exception = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Logs a critical error message with exception
    /// </summary>
    /// <param name="category">The log category</param>
    /// <param name="message">The log message</param>
    /// <param name="exception">The exception details</param>
    /// <param name="cancellationToken">The cancellation token</param>
    Task LogCriticalAsync(
        LogCategory category,
        string message,
        Exception exception,
        CancellationToken cancellationToken = default);
}
