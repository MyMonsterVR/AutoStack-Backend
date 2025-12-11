using AutoStack.Application.Features.Auth.Commands.VerifyEmail;
using FluentAssertions;
using Xunit;

namespace AutoStack.Application.Tests.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandValidatorTests
{
    private readonly VerifyEmailCommandValidator _validator;

    public VerifyEmailCommandValidatorTests()
    {
        _validator = new VerifyEmailCommandValidator();
    }

    [Fact]
    public void Validate_WithValidCommand_ShouldNotHaveErrors()
    {
        var command = new VerifyEmailCommand(Guid.NewGuid(), "123456");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        var command = new VerifyEmailCommand(Guid.Empty, "123456");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId");
    }

    [Fact]
    public void Validate_WithEmptyCode_ShouldHaveError()
    {
        var command = new VerifyEmailCommand(Guid.NewGuid(), string.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage == "Verification code is required");
    }

    [Fact]
    public void Validate_WithCodeShorterThan6Digits_ShouldHaveError()
    {
        var command = new VerifyEmailCommand(Guid.NewGuid(), "12345");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage == "Verification code must be 6 digits");
    }

    [Fact]
    public void Validate_WithCodeLongerThan6Digits_ShouldHaveError()
    {
        var command = new VerifyEmailCommand(Guid.NewGuid(), "1234567");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage == "Verification code must be 6 digits");
    }

    [Fact]
    public void Validate_WithCodeContainingLetters_ShouldHaveError()
    {
        var command = new VerifyEmailCommand(Guid.NewGuid(), "12A456");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code" && e.ErrorMessage == "Verification code must contain only digits");
    }

    [Fact]
    public void Validate_WithCodeContainingSpecialCharacters_ShouldHaveError()
    {
        var command = new VerifyEmailCommand(Guid.NewGuid(), "123-56");

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Code");
    }
}
