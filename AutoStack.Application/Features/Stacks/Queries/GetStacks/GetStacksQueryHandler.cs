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
        var query = await stackRepository.GetAllAsync(cancellationToken);

        if (request.StackType.HasValue)
        {
            query = query.Where(st => st.Type == request.StackType.Value.ToString());
        }

        query = ApplySorting(query, request.StackSortByResponse, request.SortingOrderResponse);

        var stacks = query.ToList();
        var totalCount = stacks.Count();

        var stackPaginated = stacks
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToList();

        var stackResponses = stackPaginated.Select(s => new StackResponse
        {
            Id = s.Id,
            Name = s.Name,
            Description = s.Description,
            TypeResponse = Enum.Parse<StackTypeResponse>(s.Type),
            Downloads = s.Downloads,
            CreatedAt =  s.CreatedAt,
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

    private static IEnumerable<Stack> ApplySorting(IEnumerable<Stack> query, StackSortByResponse sortByResponse,
        SortingOrderResponse sortingOrderResponse)
    {
        return sortByResponse switch
        {
            StackSortByResponse.Popularity => sortingOrderResponse == SortingOrderResponse.Ascending
                ? query.OrderBy(s => s.Downloads)
                : query.OrderByDescending(s => s.Downloads),
            StackSortByResponse.CreatedDate => sortingOrderResponse == SortingOrderResponse.Ascending
                ? query.OrderBy(s => s.CreatedAt)
                : query.OrderByDescending(s => s.CreatedAt),
            _ => query.OrderByDescending(s => s.Downloads)
        };
    }
}