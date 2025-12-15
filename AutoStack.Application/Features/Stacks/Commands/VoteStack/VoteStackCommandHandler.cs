using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Commands;
using AutoStack.Application.Common.Models;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Commands.VoteStack;

/// <summary>
/// Handles voting on stacks (upvote or downvote)
/// </summary>
public class VoteStackCommandHandler : ICommandHandler<VoteStackCommand, bool>
{
    private readonly IStackRepository _stackRepository;
    private readonly IStackVoteRepository _stackVoteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public VoteStackCommandHandler(
        IStackRepository stackRepository,
        IStackVoteRepository stackVoteRepository,
        IUnitOfWork unitOfWork)
    {
        _stackRepository = stackRepository;
        _stackVoteRepository = stackVoteRepository;
        _unitOfWork = unitOfWork;
    }

    /// <summary>
    /// Processes the vote command by creating a new vote or updating an existing one
    /// </summary>
    /// <param name="request">The vote command containing stack ID and vote type</param>
    /// <param name="cancellationToken">The cancellation token</param>
    /// <returns>A result indicating success or failure</returns>
    public async Task<Result<bool>> Handle(VoteStackCommand request, CancellationToken cancellationToken)
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

        if (existingVote != null)
        {
            if (existingVote.IsUpvote == request.IsUpvote)
            {
                return Result<bool>.Failure("You have already voted this way");
            }

            if (existingVote.IsUpvote)
            {
                stack.DecrementUpvotes();
                stack.IncrementDownvotes();
            }
            else
            {
                stack.DecrementDownvotes();
                stack.IncrementUpvotes();
            }

            existingVote.UpdateVote(request.IsUpvote);
            await _stackVoteRepository.UpdateAsync(existingVote, cancellationToken);
        }
        else
        {
            if (request.IsUpvote)
            {
                stack.IncrementUpvotes();
            }
            else
            {
                stack.IncrementDownvotes();
            }

            var vote = StackVote.Create(request.StackId, request.UserId.Value, request.IsUpvote);
            await _stackVoteRepository.AddAsync(vote, cancellationToken);
        }

        await _stackRepository.UpdateAsync(stack, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
