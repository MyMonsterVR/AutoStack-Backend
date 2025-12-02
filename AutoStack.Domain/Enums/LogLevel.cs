namespace AutoStack.Domain.Enums;

/// <summary>
/// Represents the severity level of a log entry
/// </summary>
public enum LogLevel
{
    /// <summary>
    /// Informational messages for successful operations and major events
    /// </summary>
    Information = 0,

    /// <summary>
    /// Warning messages for non-critical issues and failed validations
    /// </summary>
    Warning = 1,

    /// <summary>
    /// Error messages for handled exceptions and failed operations
    /// </summary>
    Error = 2,

    /// <summary>
    /// Critical messages for unhandled exceptions and system failures
    /// </summary>
    Critical = 3
}
