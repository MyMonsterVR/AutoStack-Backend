using FluentValidation;

namespace AutoStack.Application.Features.Stacks.Commands.VoteStack;

/// <summary>
/// Validates the VoteStackCommand
/// </summary>
public class VoteStackCommandValidator : AbstractValidator<VoteStackCommand>
{
    public VoteStackCommandValidator()
    {
        RuleFor(x => x.StackId)
            .NotEmpty().WithMessage("Stack ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
