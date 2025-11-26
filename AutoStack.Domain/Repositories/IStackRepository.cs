using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

/// <summary>
/// Repository interface for Stack entity operations
/// </summary>
public interface IStackRepository : IRepository<Stack, Guid>
{
    /// <summary>
    /// Gets a stack by ID with its related StackInfo and Package data loaded
    /// </summary>
    /// <param name="id">The ID of the stack</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The stack with related data, or null if not found</returns>
    Task<Stack?> GetByIdWithInfoAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all stacks created by a specific user
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A collection of stacks created by the user</returns>
    Task<IEnumerable<Stack>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all stacks in the system
    /// </summary>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A collection of all stacks</returns>
    Task<IEnumerable<Stack>> GetAllAsync(CancellationToken cancellationToken = default);
}