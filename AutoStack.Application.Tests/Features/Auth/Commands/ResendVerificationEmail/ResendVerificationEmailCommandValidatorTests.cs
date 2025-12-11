using AutoStack.Application.Features.Auth.Commands.ResendVerificationEmail;
using FluentAssertions;
using Xunit;

namespace AutoStack.Application.Tests.Features.Auth.Commands.ResendVerificationEmail;

public class ResendVerificationEmailCommandValidatorTests
{
    private readonly ResendVerificationEmailCommandValidator _validator;

    public ResendVerificationEmailCommandValidatorTests()
    {
        _validator = new ResendVerificationEmailCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new ResendVerificationEmailCommand(Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        var command = new ResendVerificationEmailCommand(Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId" && e.ErrorMessage == "User ID is required");
    }
}
