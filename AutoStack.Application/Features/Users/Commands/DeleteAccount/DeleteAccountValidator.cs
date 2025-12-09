using FluentValidation;

namespace AutoStack.Application.Features.Users.Commands.DeleteAccount;

public class DeleteAccountValidator : AbstractValidator<DeleteAccountCommand>
{
    public DeleteAccountValidator()
    {
        RuleFor(da => da.UserId)
            .NotEmpty().WithMessage("UserId is required");
    }
}