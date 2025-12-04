using FluentValidation;

namespace AutoStack.Application.Features.Users.Commands.EditUser;

public class EditUserCommandValidator : AbstractValidator<EditUserCommand>
{
    public EditUserCommandValidator()
    {
        RuleFor(c => c.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is invalid.")
            .MaximumLength(254).WithMessage("Email cannot exceed 254 characters.")
            .Must(email => email == null || !email.Contains("..")).WithMessage("Email cannot contain consecutive dots.")
            .Must(email => email == null || (!email.StartsWith('.') && !email.EndsWith('.'))).WithMessage("Email cannot start or end with a dot.");

        RuleFor(c => c.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(30).WithMessage("Username cannot exceed 30 characters.")
            .Must(username => username == null || !username.Contains(' ')).WithMessage("Username cannot contain spaces.")
            .Must(username => username != null && username.Length > 0 && char.IsLetterOrDigit(username[0])).WithMessage("Username must start with a letter or number.")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, hyphens, and underscores.");

        RuleFor(c => c.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("New password cannot exceed 128 characters.")
            .When(c => !string.IsNullOrWhiteSpace(c.CurrentPassword));

        RuleFor(c => c.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(c => c.NewPassword).WithMessage("Passwords do not match.")
            .When(c => !string.IsNullOrWhiteSpace(c.NewPassword));
    }
}