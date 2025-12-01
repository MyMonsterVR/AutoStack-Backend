using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Queries.GetStack;

public class GetStackQueryHandler : IQueryHandler<GetStackQuery, StackResponse>
{
    private readonly IStackRepository _stackRepository;

    public GetStackQueryHandler(IStackRepository stackRepository)
    {
        _stackRepository = stackRepository;
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

        var stackResponse = new StackResponse()
        {
            Id = stack.Id,
            Name = stack.Name,
            Description = stack.Description,
            Downloads = stack.Downloads,
            Type = Enum.Parse<StackTypeResponse>(stack.Type),
            Packages = stack.Packages.Select(si => new PackagesResponse(
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