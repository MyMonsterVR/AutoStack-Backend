using FluentValidation;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.BeginSetup;

public class BeginTwoFactorSetupCommandValidator : AbstractValidator<BeginTwoFactorSetupCommand>
{
    public BeginTwoFactorSetupCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");
    }
}
