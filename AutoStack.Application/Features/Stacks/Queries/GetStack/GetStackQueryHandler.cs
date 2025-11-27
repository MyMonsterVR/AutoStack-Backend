using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Queries.GetStack;

public class GetStackQueryHandler(
    IStackRepository stackRepository,
    IUserRepository userRepository
    ) : IQueryHandler<GetStackQuery, StackResponse>
{
    public async Task<Result<StackResponse>> Handle(GetStackQuery request, CancellationToken cancellationToken)
    {
        var stack = await stackRepository.GetByIdWithInfoAsync(request.Id, cancellationToken);

        if (stack == null)
        {
            return Result<StackResponse>.Failure("No stack found");
        }
        
        var user = await userRepository.GetByIdAsync(stack.UserId, cancellationToken);
        if (user == null)
        {
            return Result<StackResponse>.Failure("No user found");
        }
        
        var stackResponse = new StackResponse()
        {
            Id = stack.Id,
            Name = stack.Name,
            Description = stack.Description,
            Downloads = stack.Downloads,
            TypeResponse = Enum.Parse<StackTypeResponse>(stack.Type),
            Packages = stack.Packages.Select(si => new PackagesResponse(
                si.Package.Name,
                si.Package.Link,
                si.Package.IsVerified
            )).ToList(),
            UserId = stack.UserId,
            Username = user.Username,
            UserAvatarUrl = user.AvatarUrl
        };
        
        return Result<StackResponse>.Success(stackResponse);
    }
}