using FluentValidation;

namespace AutoStack.Application.Features.Stacks.Commands.RemoveVote;

/// <summary>
/// Validates the RemoveVoteCommand
/// </summary>
public class RemoveVoteCommandValidator : AbstractValidator<RemoveVoteCommand>
{
    public RemoveVoteCommandValidator()
    {
        RuleFor(x => x.StackId)
            .NotEmpty().WithMessage("Stack ID is required");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}
