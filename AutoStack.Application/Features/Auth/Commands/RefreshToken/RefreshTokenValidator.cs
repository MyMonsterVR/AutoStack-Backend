using FluentValidation;

namespace AutoStack.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenValidator : AbstractValidator<RefreshTokenCommand>
{
    public RefreshTokenValidator()
    {
        RuleFor(r => r.RefreshToken)
            .NotNull().WithMessage("RefreshToken cannot be null")
            .NotEmpty().WithMessage("RefreshToken cannot be empty");
    }
}