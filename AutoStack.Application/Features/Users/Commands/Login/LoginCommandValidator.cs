using FluentValidation;

namespace AutoStack.Application.Features.Users.Commands.Login;

public class LoginCommandValidator : AbstractValidator<LoginCommand>
{
    public LoginCommandValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.");

        RuleFor(c => c.Password)
            .NotEmpty().WithMessage("Password is required.");
    }
}