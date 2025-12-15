using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Stacks.Commands.VoteStack;

/// <summary>
/// Command to vote on a stack (upvote or downvote)
/// </summary>
/// <param name="StackId">The ID of the stack to vote on</param>
/// <param name="IsUpvote">True for upvote, false for downvote</param>
/// <param name="UserId">The ID of the user voting</param>
public record VoteStackCommand(
    Guid StackId,
    bool IsUpvote,
    Guid? UserId = null
) : ICommand<bool>;
