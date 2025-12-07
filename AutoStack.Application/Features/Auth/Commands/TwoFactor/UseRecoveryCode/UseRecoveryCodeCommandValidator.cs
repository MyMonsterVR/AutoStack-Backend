using FluentValidation;

namespace AutoStack.Application.Features.Auth.Commands.TwoFactor.UseRecoveryCode;

public class UseRecoveryCodeCommandValidator : AbstractValidator<UseRecoveryCodeCommand>
{
    public UseRecoveryCodeCommandValidator()
    {
        RuleFor(x => x.TwoFactorToken)
            .NotEmpty()
            .WithMessage("Two-factor token is required");

        RuleFor(x => x.RecoveryCode)
            .NotEmpty()
            .WithMessage("Recovery code is required")
            .Must(code => code.Replace("-", "").Length == 10)
            .WithMessage("Recovery code must be 10 characters");
    }
}
