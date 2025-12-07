using FluentValidation;

namespace AutoStack.Application.Features.Auth.Commands.ResetPassword;

public class ResetPasswordValidator : AbstractValidator<ResetPasswordCommand>
{
    public ResetPasswordValidator()
    {
        RuleFor(c => c.Token)
            .NotEmpty().WithMessage("Reset password token is required.");
        
        RuleFor(c => c.NewPassword)
            .NotEmpty().WithMessage("New password is required.")
            .MinimumLength(8).WithMessage("New password must be at least 8 characters long.")
            .MaximumLength(128).WithMessage("New password cannot exceed 128 characters.");

        RuleFor(c => c.ConfirmNewPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(c => c.NewPassword).WithMessage("Passwords do not match.")
            .When(c => !string.IsNullOrWhiteSpace(c.NewPassword));
    }
}