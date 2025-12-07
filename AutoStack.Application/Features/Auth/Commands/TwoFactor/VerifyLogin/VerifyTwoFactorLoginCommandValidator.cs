using FluentValidation;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.VerifyLogin;

public class VerifyTwoFactorLoginCommandValidator : AbstractValidator<VerifyTwoFactorLoginCommand>
{
    public VerifyTwoFactorLoginCommandValidator()
    {
        RuleFor(x => x.TwoFactorToken)
            .NotEmpty()
            .WithMessage("Two-factor token is required");

        RuleFor(x => x.TotpCode)
            .NotEmpty()
            .WithMessage("Verification code is required")
            .Length(6)
            .WithMessage("Verification code must be 6 digits")
            .Matches(@"^\d{6}$")
            .WithMessage("Verification code must be numeric");
    }
}
