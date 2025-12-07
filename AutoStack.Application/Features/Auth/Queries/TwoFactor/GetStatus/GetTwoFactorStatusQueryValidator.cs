using FluentValidation;

namespace AutoStack.Application.Features.Auth.Queries.TwoFactor.GetStatus;

public class GetTwoFactorStatusQueryValidator : AbstractValidator<GetTwoFactorStatusQuery>
{
    public GetTwoFactorStatusQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
