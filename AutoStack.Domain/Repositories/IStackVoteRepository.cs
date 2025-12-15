using AutoStack.Domain.Common;
using AutoStack.Domain.Entities;

namespace AutoStack.Domain.Repositories;

/// <summary>
/// Repository interface for StackVote entity operations
/// </summary>
public interface IStackVoteRepository : IRepository<StackVote, Guid>
{
    /// <summary>
    /// Gets a vote by user ID and stack ID
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="stackId">The ID of the stack</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>The vote if found, or null</returns>
    Task<StackVote?> GetByUserAndStackAsync(Guid userId, Guid stackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets all votes for a specific stack
    /// </summary>
    /// <param name="stackId">The ID of the stack</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A collection of votes for the stack</returns>
    Task<IEnumerable<StackVote>> GetByStackIdAsync(Guid stackId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks if a user has voted on a specific stack
    /// </summary>
    /// <param name="userId">The ID of the user</param>
    /// <param name="stackId">The ID of the stack</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>True if the user has voted, otherwise false</returns>
    Task<bool> HasUserVotedAsync(Guid userId, Guid stackId, CancellationToken cancellationToken = default);
}
