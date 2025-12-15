using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Stacks.Commands.RemoveVote;

/// <summary>
/// Command to remove a vote from a stack
/// </summary>
/// <param name="StackId">The ID of the stack to remove vote from</param>
/// <param name="UserId">The ID of the user removing their vote</param>
public record RemoveVoteCommand(
    Guid StackId,
    Guid? UserId = null
) : ICommand<bool>;
