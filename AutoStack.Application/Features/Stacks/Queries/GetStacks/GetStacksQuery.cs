using AutoStack.Application.Common.Interfaces.Queries;
using AutoStack.Application.Common.Models;
using AutoStack.Application.DTOs.Stacks;

namespace AutoStack.Application.Features.Stacks.Queries.GetStacks;

/// <summary>
/// Gets multiple stacks
/// </summary>
/// <param name="StackSortBy">Property to sort by</param>
/// <param name="SortingOrder">The order for sorting</param>
/// <param name="StackType">Stack type; null if all stacks should be shown</param>
/// <param name="PageNumber">The page we are on</param>
/// <param name="PageSize">How many elements to fetch on one page</param>
public record GetStacksQuery(
    StackSortByResponse StackSortBy = StackSortByResponse.Popularity,
    SortingOrderResponse SortingOrder = SortingOrderResponse.Ascending,
    StackTypeResponse? StackType = null,
    int PageNumber = 1,
    int PageSize = 20) : IQuery<PagedResponse<StackResponse>>;