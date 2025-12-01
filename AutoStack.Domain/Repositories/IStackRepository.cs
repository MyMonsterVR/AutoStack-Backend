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

    /// <summary>
    /// Gets a paginated collection of stacks with filtering and sorting
    /// </summary>
    /// <param name="pageNumber">The page number</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="stackType">Optional filter by stack type</param>
    /// <param name="sortBy">Property to sort by</param>
    /// <param name="sortDescending">Whether to sort in descending order</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A tuple containing the paginated stacks and total count</returns>
    Task<(IEnumerable<Stack> Stacks, int TotalCount)> GetStacksPagedAsync(
        int pageNumber,
        int pageSize,
        string? stackType,
        string sortBy,
        bool sortDescending,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all verified packages in the system
    /// </summary>
    /// <returns>A collection of all verified packages</returns>
    Task<IEnumerable<Package>> GetVerifiedPackagesAsync(CancellationToken cancellationToken = default);
}