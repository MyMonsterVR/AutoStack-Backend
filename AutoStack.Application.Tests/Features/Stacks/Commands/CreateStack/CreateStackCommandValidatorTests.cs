using AutoStack.Application.DTOs.Stacks;
using AutoStack.Application.Features.Stacks.Commands.CreateStack;
using FluentValidation.TestHelper;
using Xunit;

namespace AutoStack.Application.Tests.Features.Stacks.Commands.CreateStack;

public class CreateStackCommandValidatorTests
{
    private readonly CreateStackCommandValidator _validator;

    public CreateStackCommandValidatorTests()
    {
        _validator = new CreateStackCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new CreateStackCommand(
            Name: "React Stack",
            Description: "A modern React stack for building UIs",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithEmptyName_ShouldHaveError()
    {
        var command = new CreateStackCommand(
            Name: "",
            Description: "Valid description here",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Stack name is required.");
    }

    [Fact]
    public void Validate_WithNameTooShort_ShouldHaveError()
    {
        var command = new CreateStackCommand(
            Name: "AB",
            Description: "Valid description here",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Stack name must be at least 3 characters long.");
    }

    [Fact]
    public void Validate_WithNameTooLong_ShouldHaveError()
    {
        var command = new CreateStackCommand(
            Name: new string('A', 101),
            Description: "Valid description here",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Name)
            .WithErrorMessage("Stack name cannot exceed 100 characters.");
    }

    [Fact]
    public void Validate_WithEmptyDescription_ShouldHaveError()
    {
        var command = new CreateStackCommand(
            Name: "Valid Stack",
            Description: "",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Description)
            .WithErrorMessage("Description is required.");
    }

    [Fact]
    public void Validate_WithDescriptionTooShort_ShouldHaveError()
    {
        var command = new CreateStackCommand(
            Name: "Valid Stack",
            Description: "Short",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Description)
            .WithErrorMessage("Description must be at least 10 characters long.");
    }

    [Fact]
    public void Validate_WithDescriptionTooLong_ShouldHaveError()
    {
        var command = new CreateStackCommand(
            Name: "Valid Stack",
            Description: new string('A', 501),
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Description)
            .WithErrorMessage("Description cannot exceed 500 characters.");
    }

    [Fact]
    public void Validate_WithEmptyPackages_ShouldHaveError()
    {
        var command = new CreateStackCommand(
            Name: "Valid Stack",
            Description: "Valid description here",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>()
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(c => c.Packages)
            .WithErrorMessage("At least one package is required.");
    }

    [Fact]
    public void Validate_WithEmptyPackageName_ShouldHaveError()
    {
        var command = new CreateStackCommand(
            Name: "Valid Stack",
            Description: "Valid description here",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("", "https://www.npmjs.com/package/react")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Packages[0].Name")
            .WithErrorMessage("Package name is required.");
    }

    [Fact]
    public void Validate_WithEmptyPackageLink_ShouldHaveError()
    {
        var command = new CreateStackCommand(
            Name: "Valid Stack",
            Description: "Valid description here",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Packages[0].Link")
            .WithErrorMessage("Package link is required.");
    }

    [Fact]
    public void Validate_WithInvalidPackageUrl_ShouldHaveError()
    {
        var command = new CreateStackCommand(
            Name: "Valid Stack",
            Description: "Valid description here",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "not-a-valid-url")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Packages[0].Link")
            .WithErrorMessage("Package link must be a valid URL.");
    }

    [Fact]
    public void Validate_WithNonNpmjsUrl_ShouldHaveError()
    {
        var command = new CreateStackCommand(
            Name: "Valid Stack",
            Description: "Valid description here",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://github.com/facebook/react")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Packages[0].Link")
            .WithErrorMessage("Package link must be a valid npmjs.org package URL (e.g., https://www.npmjs.com/package/react)");
    }

    [Fact]
    public void Validate_WithMultiplePackages_ShouldNotHaveErrors()
    {
        var command = new CreateStackCommand(
            Name: "React Stack",
            Description: "A modern React stack for building UIs",
            Type: StackTypeResponse.FRONTEND,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react"),
                new("react-dom", "https://www.npmjs.com/package/react-dom"),
                new("typescript", "https://www.npmjs.com/package/typescript")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(StackTypeResponse.FRONTEND)]
    [InlineData(StackTypeResponse.BACKEND)]
    [InlineData(StackTypeResponse.FULLSTACK)]
    public void Validate_WithValidStackType_ShouldNotHaveErrors(StackTypeResponse stackType)
    {
        var command = new CreateStackCommand(
            Name: "Valid Stack",
            Description: "Valid description here",
            Type: stackType,
            Packages: new List<PackageInput>
            {
                new("react", "https://www.npmjs.com/package/react")
            }
        );

        var result = _validator.TestValidate(command);

        result.ShouldNotHaveValidationErrorFor(c => c.Type);
    }
}
