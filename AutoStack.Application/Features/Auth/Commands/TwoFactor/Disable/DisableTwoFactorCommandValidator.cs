using FluentValidation;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.Disable;

public class DisableTwoFactorCommandValidator : AbstractValidator<DisableTwoFactorCommand>
{
    public DisableTwoFactorCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("User ID is required");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required");

        RuleFor(x => x.TotpCode)
            .NotEmpty()
            .WithMessage("Verification code is required")
            .Length(6)
            .WithMessage("Verification code must be 6 digits")
            .Matches(@"^\d{6}$")
            .WithMessage("Verification code must be numeric");
    }
}
