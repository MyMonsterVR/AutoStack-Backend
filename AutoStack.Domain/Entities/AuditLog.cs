using AutoStack.Domain.Common;
using AutoStack.Domain.Enums;

namespace AutoStack.Domain.Entities;

/// <summary>
/// Represents an audit log entry for errors and major application events
/// GDPR: Logs are automatically deleted after 30 days.
/// User logs are anonymized when user account is deleted.
/// </summary>
public class AuditLog : Entity<Guid>
{
    public LogLevel Level { get; private set; }
    public LogCategory Category { get; private set; }
    public string Message { get; private set; } = string.Empty;
    public string? Exception { get; private set; }

    /// <summary>
    /// User ID who triggered this log (nullable for anonymous/system events)
    /// </summary>
    public Guid? UserId { get; private set; }

    public string? Username { get; private set; }
    public string? IpAddress { get; private set; }
    public string? UserAgent { get; private set; }
    public string? HttpMethod { get; private set; }
    public string? Endpoint { get; private set; }

    /// <summary>
    /// Gets the HTTP status code returned
    /// </summary>
    public int? StatusCode { get; private set; }

    /// <summary>
    /// Sanitized request body with sensitive fields redacted for GDPR compliance
    /// </summary>
    public string? SanitizedRequestBody { get; private set; }

    public long? DurationMs { get; private set; }
    public string? AdditionalData { get; private set; }

    private AuditLog()
    {
    }

    private AuditLog(
        Guid id,
        LogLevel level,
        LogCategory category,
        string message,
        string? exception,
        Guid? userId,
        string? username,
        string? ipAddress,
        string? userAgent,
        string? httpMethod,
        string? endpoint,
        int? statusCode,
        string? sanitizedRequestBody,
        long? durationMs,
        string? additionalData) : base(id)
    {
        Level = level;
        Category = category;
        Message = message;
        Exception = exception;
        UserId = userId;
        Username = username;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        HttpMethod = httpMethod;
        Endpoint = endpoint;
        StatusCode = statusCode;
        SanitizedRequestBody = sanitizedRequestBody;
        DurationMs = durationMs;
        AdditionalData = additionalData;
    }

    /// <summary>
    /// Creates a new audit log entry
    /// </summary>
    /// <param name="level">The severity level of the log</param>
    /// <param name="category">The category/type of the log</param>
    /// <param name="message">The log message</param>
    /// <param name="exception">Optional exception details</param>
    /// <param name="userId">Optional user ID who triggered this log</param>
    /// <param name="username">Optional username who triggered this log</param>
    /// <param name="ipAddress">Optional IP address of the request</param>
    /// <param name="userAgent">Optional User-Agent header</param>
    /// <param name="httpMethod">Optional HTTP method</param>
    /// <param name="endpoint">Optional endpoint/path</param>
    /// <param name="statusCode">Optional HTTP status code</param>
    /// <param name="sanitizedRequestBody">Optional sanitized request body</param>
    /// <param name="durationMs">Optional duration in milliseconds</param>
    /// <param name="additionalData">Optional additional data as JSON</param>
    /// <returns>A new AuditLog instance</returns>
    /// <exception cref="ArgumentException">Thrown when message is null or empty</exception>
    public static AuditLog CreateLog(
        LogLevel level,
        LogCategory category,
        string message,
        string? exception = null,
        Guid? userId = null,
        string? username = null,
        string? ipAddress = null,
        string? userAgent = null,
        string? httpMethod = null,
        string? endpoint = null,
        int? statusCode = null,
        string? sanitizedRequestBody = null,
        long? durationMs = null,
        string? additionalData = null)
    {
        if (string.IsNullOrWhiteSpace(message))
            throw new ArgumentException("Log message cannot be empty", nameof(message));

        return new AuditLog(
            Guid.NewGuid(),
            level,
            category,
            message,
            exception,
            userId,
            username,
            ipAddress,
            userAgent,
            httpMethod,
            endpoint,
            statusCode,
            sanitizedRequestBody,
            durationMs,
            additionalData);
    }

    /// <summary>
    /// Anonymizes this log entry for GDPR compliance when user is deleted
    /// </summary>
    public void Anonymize()
    {
        UserId = null;
        Username = "DELETED_USER";
        IpAddress = null;
        UserAgent = null;
        UpdateTimestamp();
    }
}
