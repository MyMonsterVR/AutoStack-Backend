using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Queries.GetStack;

public class GetStackQueryHandler : IQueryHandler<GetStackQuery, StackResponse>
{
    private readonly IStackRepository _stackRepository;
    private readonly IStackVoteRepository _stackVoteRepository;
    private readonly ICurrentUserService _currentUserService;

    public GetStackQueryHandler(
        IStackRepository stackRepository,
        IStackVoteRepository stackVoteRepository,
        ICurrentUserService currentUserService)
    {
        _stackRepository = stackRepository;
        _stackVoteRepository = stackVoteRepository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<StackResponse>> Handle(GetStackQuery request, CancellationToken cancellationToken)
    {
        var stack = await _stackRepository.GetByIdWithInfoAsync(request.Id, cancellationToken);

        if (stack == null)
        {
            return Result<StackResponse>.Failure("No stack found");
        }

        if (stack.User == null)
        {
            return Result<StackResponse>.Failure("No user found");
        }

        bool? userVote = null;
        if (_currentUserService.UserId.HasValue)
        {
            var vote = await _stackVoteRepository.GetByUserAndStackAsync(
                _currentUserService.UserId.Value,
                stack.Id,
                cancellationToken);

            if (vote != null)
            {
                userVote = vote.IsUpvote;
            }
        }

        var stackResponse = new StackResponse()
        {
            Id = stack.Id,
            Name = stack.Name,
            Description = stack.Description,
            Downloads = stack.Downloads,
            UpvoteCount = stack.UpvoteCount,
            DownvoteCount = stack.DownvoteCount,
            UserVote = userVote,
            Type = Enum.Parse<StackTypeResponse>(stack.Type),
            Packages = stack.Packages.Select(si => new PackageResponse(
                si.Package.Name,
                si.Package.Link,
                si.Package.IsVerified
            )).ToList(),
            CreatedAt = stack.CreatedAt,
            UserId = stack.UserId,
            Username = stack.User.Username,
            UserAvatarUrl = stack.User.AvatarUrl
        };

        return Result<StackResponse>.Success(stackResponse);
    }
}