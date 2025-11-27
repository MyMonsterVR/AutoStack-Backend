using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;

namespace AutoStack.Application.Features.Stacks.Queries.GetStacks;

public class GetStacksQueryHandler(
    IStackRepository stackRepository
    ) : IQueryHandler<GetStacksQuery, PagedResponse<StackResponse>>
{
    public async Task<Result<PagedResponse<StackResponse>>> Handle(GetStacksQuery request, CancellationToken cancellationToken)
    {
        var stackType = request.StackType?.ToString();
        var sortBy = request.StackSortByResponse.ToString();
        var sortDescending = request.SortingOrderResponse == SortingOrderResponse.Descending;

        // Get paginated data from repository (database-level operation)
        var (stacks, totalCount) = await stackRepository.GetStacksPagedAsync(
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
            TypeResponse = Enum.Parse<StackTypeResponse>(s.Type),
            Downloads = s.Downloads,
            CreatedAt = s.CreatedAt,
            UserId = s.UserId
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