using FluentValidation;

namespace AutoStack.Application.Features.Stacks.Commands.CreateStack;

public class CreateStackCommandValidator : AbstractValidator<CreateStackCommand>
{
    public CreateStackCommandValidator()
    {
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Stack name is required.")
            .MinimumLength(3).WithMessage("Stack name must be at least 3 characters long.")
            .MaximumLength(100).WithMessage("Stack name cannot exceed 100 characters.");

        RuleFor(c => c.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MinimumLength(10).WithMessage("Description must be at least 10 characters long.")
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(c => c.TypeResponse)
            .IsInEnum().WithMessage("Invalid stack type.");

        RuleFor(c => c.Packages)
            .NotEmpty().WithMessage("At least one package is required.")
            .Must(packages => packages != null && packages.Count > 0)
            .WithMessage("At least one package is required.");

        RuleForEach(c => c.Packages)
            .ChildRules(item =>
            {
                item.RuleFor(x => x.Name)
                    .NotEmpty().WithMessage("Package name is required.");

                item.RuleFor(x => x.Link)
                    .NotEmpty().WithMessage("Package link is required.")
                    .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
                    .WithMessage("Package link must be a valid URL.")
                    .Must(link => link.StartsWith("https://www.npmjs.com/package/", StringComparison.OrdinalIgnoreCase))
                    .WithMessage("Package link must be a valid npmjs.org package URL (e.g., https://www.npmjs.com/package/react)");
            });
    }
}
