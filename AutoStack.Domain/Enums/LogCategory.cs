namespace AutoStack.Domain.Enums;

/// <summary>
/// Represents the category/type of a log entry for classification purposes
/// </summary>
public enum LogCategory
{
    /// <summary>
    /// Authentication-related events (login, logout, failed login attempts)
    /// </summary>
    Authentication = 0,

    /// <summary>
    /// Authorization-related events (permission denied, forbidden access)
    /// </summary>
    Authorization = 1,

    /// <summary>
    /// Stack-related operations (create, update, delete, download)
    /// </summary>
    Stack = 2,

    /// <summary>
    /// User-related operations (registration, update, delete)
    /// </summary>
    User = 3,

    /// <summary>
    /// Validation failures from FluentValidation
    /// </summary>
    Validation = 4,

    /// <summary>
    /// System-level events (unhandled exceptions, system errors)
    /// </summary>
    System = 5,

    /// <summary>
    /// Rate limiting events (rate limit exceeded, potential DoS)
    /// </summary>
    RateLimiting = 6,

    /// <summary>
    /// Security-related events (suspicious activity, security threats)
    /// </summary>
    Security = 7
}
