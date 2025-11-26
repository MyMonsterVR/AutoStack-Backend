using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;

namespace AutoStack.Application.Features.Stacks.Queries.GetStacks;

/// <summary>
/// Gets multiple stacks
/// </summary>
/// <param name="StackSortByResponse">Property to sort by</param>
/// <param name="StackType">Stack type; null if all stacks should be shown</param>
public record GetStacksQuery(
    StackSortByResponse StackSortByResponse = StackSortByResponse.Popularity,
    SortingOrderResponse SortingOrderResponse = SortingOrderResponse.Descending,
    StackTypeResponse? StackType = null,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagedResponse<StackResponse>>;