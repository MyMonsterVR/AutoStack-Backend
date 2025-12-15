using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Queries.MyStacks;

public class MyStacksQueryHandler : IQueryHandler<MyStacksQuery, IEnumerable<StackResponse>>
{
    private readonly IStackRepository _stackRepository;
    
    public MyStacksQueryHandler(IStackRepository stackRepository)
    {
        _stackRepository = stackRepository;
    }
    
    public async Task<Result<IEnumerable<StackResponse>>> Handle(MyStacksQuery request, CancellationToken cancellationToken)
    {
        if (!request.UserId.HasValue)
        {
            return Result<IEnumerable<StackResponse>>.Failure("User ID is required");
        }
        
        var stacks = await _stackRepository.GetByUserIdAsync(request.UserId.Value, cancellationToken);
        
        var stackResponse = stacks.Select(s => new StackResponse
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            Type = Enum.Parse<StackTypeResponse>(s.Type),
            Downloads = s.Downloads,
            UpvoteCount = s.UpvoteCount,
            DownvoteCount = s.DownvoteCount,
            CreatedAt = s.CreatedAt,
            UserId = s.UserId
        });
        
        return Result<IEnumerable<StackResponse>>.Success(stackResponse);
    }
}