using FluentValidation;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.ConfirmSetup;

public class ConfirmTwoFactorSetupCommandValidator : AbstractValidator<ConfirmTwoFactorSetupCommand>
{
    public ConfirmTwoFactorSetupCommandValidator()
    {
        RuleFor(x => x.SetupToken)
            .NotEmpty()
            .WithMessage("Setup token is required");

        RuleFor(x => x.TotpCode)
            .NotEmpty()
            .WithMessage("Verification code is required")
            .Length(6)
            .WithMessage("Verification code must be 6 digits")
            .Matches(@"^\d{6}$")
            .WithMessage("Verification code must be numeric");
    }
}
