using AutoStack.Domain.Common;

namespace AutoStack.Domain.Entities;

/// <summary>
/// Represents a vote (upvote or downvote) on a stack by a user
/// </summary>
public class StackVote : Entity<Guid>
{
    /// <summary>
    /// Gets the ID of the stack being voted on
    /// </summary>
    public Guid StackId { get; private set; }

    /// <summary>
    /// Gets the ID of the user who voted
    /// </summary>
    public Guid UserId { get; private set; }

    /// <summary>
    /// Gets whether this is an upvote (true) or downvote (false)
    /// </summary>
    public bool IsUpvote { get; private set; }

    // Navigation properties
    public Stack Stack { get; private set; } = null!;
    public User User { get; private set; } = null!;

    private StackVote()
    {
    }

    private StackVote(Guid id, Guid stackId, Guid userId, bool isUpvote) : base(id)
    {
        StackId = stackId;
        UserId = userId;
        IsUpvote = isUpvote;
    }

    /// <summary>
    /// Creates a new vote on a stack
    /// </summary>
    /// <param name="stackId">The ID of the stack being voted on</param>
    /// <param name="userId">The ID of the user voting</param>
    /// <param name="isUpvote">True for upvote, false for downvote</param>
    /// <returns>A new StackVote instance</returns>
    public static StackVote Create(Guid stackId, Guid userId, bool isUpvote)
    {
        if (stackId == Guid.Empty)
            throw new ArgumentException("Stack ID cannot be empty", nameof(stackId));

        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty", nameof(userId));

        return new StackVote(Guid.NewGuid(), stackId, userId, isUpvote);
    }

    /// <summary>
    /// Updates the vote type (changes between upvote and downvote)
    /// </summary>
    /// <param name="isUpvote">True for upvote, false for downvote</param>
    public void UpdateVote(bool isUpvote)
    {
        IsUpvote = isUpvote;
        UpdateTimestamp();
    }
}
