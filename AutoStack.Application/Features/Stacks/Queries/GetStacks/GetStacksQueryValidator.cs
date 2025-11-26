using FluentValidation;

namespace AutoStack.Application.Features.Stacks.Queries.GetStacks;

public class GetStacksQueryValidator : AbstractValidator<GetStacksQuery>
{
    public GetStacksQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageNumber must be at least 1");

        RuleFor(x => x.PageSize)
            .GreaterThanOrEqualTo(1)
            .WithMessage("PageSize must be at least 1");

        RuleFor(x => x.StackSortByResponse)
            .IsInEnum()
            .WithMessage("StackSortBy must be one of: Popularity, Rating, PostedDate");

        RuleFor(x => x.SortingOrderResponse)
            .IsInEnum()
            .WithMessage("SortingOrder must be either Ascending or Descending");

        RuleFor(x => x.StackType)
            .IsInEnum()
            .When(x => x.StackType.HasValue)
            .WithMessage("StackType must be one of: FRONTEND, BACKEND, FULLSTACK");
    }
}
