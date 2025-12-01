using FluentValidation;

namespace AutoStack.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(rc => rc.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MinimumLength(3).WithMessage("Email has to be minimum 3 characters long.")
            .EmailAddress().WithMessage("Email is invalid.");
        
        
        RuleFor(rc => rc.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username has to be minimum 3 characters long.");

        RuleFor(rc => rc.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password has to be minimum 8 characters long.");
        
        RuleFor(rc => rc.ConfirmPassword)
            .NotEmpty().WithMessage("ConfirmPassword is required.")
            .MinimumLength(8).WithMessage("ConfirmPassword has to be minimum 8 characters long.");
    }
}