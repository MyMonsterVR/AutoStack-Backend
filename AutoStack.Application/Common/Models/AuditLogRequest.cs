using AutoStack.Domain.Enums;

namespace AutoStack.Application.Common.Models;

/// <summary>
/// Request model for creating an audit log entry
/// </summary>
public class AuditLogRequest
{
    public required LogLevel Level { get; init; }
    public required LogCategory Category { get; init; }
    public required string Message { get; init; }
    public Exception? Exception { get; init; }
    public long? DurationMs { get; init; }
    public int? StatusCode { get; init; }
    public string? SanitizedRequestBody { get; init; }

    /// <summary>
    /// Additional contextual data (will be serialized to JSON)
    /// </summary>
    public Dictionary<string, object>? AdditionalData { get; init; }

    /// <summary>
    /// Optional override for UserId (for background jobs or system events)
    /// </summary>
    public Guid? UserIdOverride { get; init; }

    /// <summary>
    /// Optional override for Username (for background jobs or system events)
    /// </summary>
    public string? UsernameOverride { get; init; }
}
