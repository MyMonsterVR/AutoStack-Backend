using FluentValidation;

namespace AutoStack.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(rc => rc.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Email is invalid.")
            .MaximumLength(254).WithMessage("Email cannot exceed 254 characters.")
            .Must(email => email == null || !email.Contains("..")).WithMessage("Email cannot contain consecutive dots.")
            .Must(email => email == null || (!email.StartsWith('.') && !email.EndsWith('.'))).WithMessage("Email cannot start or end with a dot.");

        RuleFor(rc => rc.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(3).WithMessage("Username must be at least 3 characters long.")
            .MaximumLength(30).WithMessage("Username cannot exceed 30 characters.")
            .Must(username => username == null || !username.Contains(' ')).WithMessage("Username cannot contain spaces.")
            .Must(username => username != null && username.Length > 0 && char.IsLetterOrDigit(username[0])).WithMessage("Username must start with a letter or number.")
            .Matches("^[a-zA-Z0-9_-]+$").WithMessage("Username can only contain letters, numbers, hyphens, and underscores.");

        RuleFor(rc => rc.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("Password cannot exceed 128 characters.");

        RuleFor(rc => rc.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(rc => rc.Password).WithMessage("Passwords do not match.");
    }
}