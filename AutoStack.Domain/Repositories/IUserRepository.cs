using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

/// <summary>
/// Repository interface for User entity operations
/// </summary>
public interface IUserRepository : IRepository<User, Guid>
{
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

    /// <summary>
    /// Counts the number of stacks created by a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The count of stacks created by the user</returns>
    Task<int> CountStacksByUserId(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Counts the number of templates created by a user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The count of templates created by the user</returns>
    Task<int> CountTemplatesByUserId(Guid userId, CancellationToken cancellationToken = default);
}