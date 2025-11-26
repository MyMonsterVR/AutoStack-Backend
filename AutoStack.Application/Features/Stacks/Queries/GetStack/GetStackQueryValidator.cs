using FluentValidation;

namespace AutoStack.Application.Features.Stacks.Queries.GetStack;

public class GetStackQueryValidator : AbstractValidator<GetStackQuery>
{
    public GetStackQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}