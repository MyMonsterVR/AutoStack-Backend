namespace AutoStack.Application.Common.Interfaces;

/// <summary>
/// Service for accessing current HTTP request context (user, IP, etc.)
/// Uses AsyncLocal storage to make context available in MediatR handlers
/// </summary>
public interface ICurrentUserService
{
    /// <summary>
    /// Gets the current user's ID (from JWT claims)
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the current user's username (from JWT claims)
    /// </summary>
    string? Username { get; }

    /// <summary>
    /// Gets the IP address of the current HTTP request
    /// </summary>
    string? IpAddress { get; }

    /// <summary>
    /// Gets the User-Agent header of the current HTTP request
    /// </summary>
    string? UserAgent { get; }

    /// <summary>
    /// Gets the HTTP method of the current request (GET, POST, etc.)
    /// </summary>
    string? HttpMethod { get; }

    /// <summary>
    /// Gets the endpoint/path of the current HTTP request
    /// </summary>
    string? Endpoint { get; }

    /// <summary>
    /// Sets the context for the current request (called by middleware)
    /// </summary>
    /// <param name="userId">The user ID from JWT claims</param>
    /// <param name="username">The username from JWT claims</param>
    /// <param name="ipAddress">The IP address of the request</param>
    /// <param name="userAgent">The User-Agent header</param>
    /// <param name="httpMethod">The HTTP method</param>
    /// <param name="endpoint">The endpoint/path</param>
    void SetContext(
        Guid? userId,
        string? username,
        string? ipAddress,
        string? userAgent,
        string? httpMethod,
        string? endpoint);
}
