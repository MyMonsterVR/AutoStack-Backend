using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Commands.RemoveVote;

/// <summary>
/// Handles removing a vote from a stack
/// </summary>
public class RemoveVoteCommandHandler : ICommandHandler<RemoveVoteCommand, bool>
{
    private readonly IStackRepository _stackRepository;
    private readonly IStackVoteRepository _stackVoteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveVoteCommandHandler(
        IStackRepository stackRepository,
        IStackVoteRepository stackVoteRepository,
        IUnitOfWork unitOfWork)
    {
        _stackRepository = stackRepository;
        _stackVoteRepository = stackVoteRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Processes the remove vote command
    /// </summary>
    /// <param name="request">The remove vote command containing stack ID</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result indicating success or failure</returns>
    public async Task<Result<bool>> Handle(RemoveVoteCommand request, CancellationToken cancellationToken)
    {
        if (!request.UserId.HasValue)
        {
            return Result<bool>.Failure("User ID is required");
        }

        var stack = await _stackRepository.GetByIdAsync(request.StackId, cancellationToken);
        if (stack == null)
        {
            return Result<bool>.Failure("Stack not found");
        }

        var existingVote = await _stackVoteRepository.GetByUserAndStackAsync(
            request.UserId.Value,
            request.StackId,
            cancellationToken);

        if (existingVote == null)
        {
            return Result<bool>.Failure("You have not voted on this stack");
        }

        if (existingVote.IsUpvote)
        {
            stack.DecrementUpvotes();
        }
        else
        {
            stack.DecrementDownvotes();
        }

        await _stackVoteRepository.DeleteAsync(existingVote, cancellationToken);
        await _stackRepository.UpdateAsync(stack, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
