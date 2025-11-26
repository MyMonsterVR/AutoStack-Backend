using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Queries.GetStack;

public class GetStackQueryHandler(
    IStackRepository stackRepository
    ) : IQueryHandler<GetStackQuery, StackResponse>
{
    public async Task<Result<StackResponse>> Handle(GetStackQuery request, CancellationToken cancellationToken)
    {
        var stack = await stackRepository.GetByIdWithInfoAsync(request.Id, cancellationToken);

        if (stack == null)
        {
            return Result<StackResponse>.Failure("No stack found");
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
        };
        
        return Result<StackResponse>.Success(stackResponse);
    }
}