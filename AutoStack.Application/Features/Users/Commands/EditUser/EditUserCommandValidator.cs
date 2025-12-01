using FluentValidation;

namespace AutoStack.Application.Features.Users.Commands.EditUser;

public class EditUserCommandValidator : AbstractValidator<EditUserCommand>
{
    public EditUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required.")
            .MinimumLength(3).WithMessage("Email has to be minimum 3 characters long.")
            .EmailAddress().WithMessage("Email is invalid.");
        
        RuleFor(c => c.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username has to be minimum 3 characters long.");

        RuleFor(c => c.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .When(c => !string.IsNullOrWhiteSpace(c.CurrentPassword));
        
        RuleFor(c => c.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .When(c => !string.IsNullOrWhiteSpace(c.NewPassword));

        RuleFor(c => c.AvatarUrl)
            .NotEmpty().WithMessage("AvatarUrl is required.");
    }
}