using AutoStack.Application.Common.Interfaces;

namespace AutoStack.Infrastructure.Services;

/// <summary>
/// Service for storing and retrieving current HTTP request context using AsyncLocal storage
/// This allows MediatR handlers deep in the application layer to access HTTP context
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private static readonly AsyncLocal<UserContext?> _context = new();

    public Guid? UserId => _context.Value?.UserId;
    public string? Username => _context.Value?.Username;
    public string? IpAddress => _context.Value?.IpAddress;
    public string? UserAgent => _context.Value?.UserAgent;
    public string? HttpMethod => _context.Value?.HttpMethod;
    public string? Endpoint => _context.Value?.Endpoint;

    public void SetContext(
        Guid? userId,
        string? username,
        string? ipAddress,
        string? userAgent,
        string? httpMethod,
        string? endpoint)
    {
        _context.Value = new UserContext
        {
            UserId = userId,
            Username = username,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            HttpMethod = httpMethod,
            Endpoint = endpoint
        };
    }

    /// <summary>
    /// Internal class to hold user context information
    /// </summary>
    private class UserContext
    {
        public Guid? UserId { get; init; }
        public string? Username { get; init; }
        public string? IpAddress { get; init; }
        public string? UserAgent { get; init; }
        public string? HttpMethod { get; init; }
        public string? Endpoint { get; init; }
    }
}
