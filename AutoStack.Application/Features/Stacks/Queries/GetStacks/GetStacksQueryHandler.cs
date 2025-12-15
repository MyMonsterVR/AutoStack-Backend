using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Queries.GetStacks;

public class GetStacksQueryHandler : IQueryHandler<GetStacksQuery, PagedResponse<StackResponse>>
{
    private readonly IStackRepository _stackRepository;

    public GetStacksQueryHandler(IStackRepository stackRepository)
    {
        _stackRepository = stackRepository;
    }

    public async Task<Result<PagedResponse<StackResponse>>> Handle(GetStacksQuery request, CancellationToken cancellationToken)
    {
        var stackType = request.StackType?.ToString();
        var sortBy = request.StackSortBy.ToString();
        var sortDescending = request.SortingOrder == SortingOrderResponse.Descending;

        // Get paginated data from repository (database-level operation)
        var (stacks, totalCount) = await _stackRepository.GetStacksPagedAsync(
            request.PageNumber,
            request.PageSize,
            stackType,
            sortBy,
            sortDescending,
            cancellationToken);

        var stackResponses = stacks.Select(s => new StackResponse
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            Type = Enum.Parse<StackTypeResponse>(s.Type),
            Downloads = s.Downloads,
            UpvoteCount = s.UpvoteCount,
            DownvoteCount = s.DownvoteCount,
            CreatedAt = s.CreatedAt,
            UserId = s.User.Id,
            Username = s.User.Username,
            UserAvatarUrl = s.User.AvatarUrl,
        }).ToList();

        var result = new PagedResponse<StackResponse>
        {
            Items = stackResponses,
            TotalCount = totalCount,
            PageNumber = request.PageNumber,
            PageSize = request.PageSize
        };

        return Result<PagedResponse<StackResponse>>.Success(result);
    }
}