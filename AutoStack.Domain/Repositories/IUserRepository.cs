using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository : IRepository<User, Guid>
{
    /// <summary>
    /// Get an account by their email
    /// </summary>
    /// <param name="email">The email address to find user on</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>User if found; else null</returns>
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get an account by their password reset token
    /// </summary>
    /// <param name="token">The password reset token to find user on</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>User if found; else null</returns>
    Task<User?> GetByPasswordResetTokenAsync(string token, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if an email address is already registered
    /// </summary>
    /// <param name="email">The email address to check</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the email exists, false otherwise</returns>
    Task<bool> EmailExists(string email, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a username is already taken
    /// </summary>
    /// <param name="username">The username to check</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the username exists, false otherwise</returns>
    Task<bool> UsernameExists(string username, CancellationToken cancellationToken = default);
}