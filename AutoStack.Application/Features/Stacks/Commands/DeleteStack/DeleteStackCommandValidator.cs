using FluentValidation;

namespace AutoStack.Application.Features.Stacks.Commands.DeleteStack;

public class DeleteStackCommandValidator : AbstractValidator<DeleteStackCommand>
{
    public DeleteStackCommandValidator()
    {
        RuleFor(command => command.StackId)
            .NotEmpty().WithMessage("The StackId is required.");
        
        RuleFor(command => command.UserId)
            .NotEmpty().WithMessage("The UserId is required.");
    }
}