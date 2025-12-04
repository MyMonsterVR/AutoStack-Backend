using AutoStack.Application.Features.Auth.Commands.Register;
using FluentAssertions;
using Xunit;

namespace AutoStack.Application.Tests.Features.Auth.Commands.Register;

public class RegisterCommandValidatorTests
{
    private readonly RegisterCommandValidator _validator;

    public RegisterCommandValidatorTests()
    {
        _validator = new RegisterCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            "Password123",
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyEmail_ShouldFail(string email)
    {
        var command = new RegisterCommand(
            email,
            "testuser",
            "Password123",
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@example.com")]
    [InlineData("test@")]
    [InlineData("test..user@example.com")]
    [InlineData(".test@example.com")]
    public void Validate_WithInvalidEmail_ShouldFail(string email)
    {
        var command = new RegisterCommand(
            email,
            "testuser",
            "Password123",
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WithEmailTooLong_ShouldFail()
    {
        var longEmail = new string('a', 250) + "@example.com"; // 263 chars total
        var command = new RegisterCommand(
            longEmail,
            "testuser",
            "Password123",
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email" && e.ErrorMessage.Contains("254"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyUsername_ShouldFail(string username)
    {
        var command = new RegisterCommand(
            "test@example.com",
            username,
            "Password123",
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("a")]
    public void Validate_WithUsernameTooShort_ShouldFail(string username)
    {
        var command = new RegisterCommand(
            "test@example.com",
            username,
            "Password123",
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("3 characters"));
    }

    [Fact]
    public void Validate_WithUsernameTooLong_ShouldFail()
    {
        var longUsername = new string('a', 31);
        var command = new RegisterCommand(
            "test@example.com",
            longUsername,
            "Password123",
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("30"));
    }

    [Theory]
    [InlineData("test user")]
    [InlineData("user name")]
    [InlineData("user name123")]
    public void Validate_WithUsernameContainingSpaces_ShouldFail(string username)
    {
        var command = new RegisterCommand(
            "test@example.com",
            username,
            "Password123",
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("spaces"));
    }

    [Theory]
    [InlineData("-username")]
    [InlineData("_username")]
    [InlineData(".username")]
    [InlineData("@username")]
    public void Validate_WithUsernameStartingWithSpecialChar_ShouldFail(string username)
    {
        var command = new RegisterCommand(
            "test@example.com",
            username,
            "Password123",
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("start with a letter or number"));
    }

    [Theory]
    [InlineData("user@name")]
    [InlineData("user.name")]
    [InlineData("user#name")]
    [InlineData("user name")]
    [InlineData("user!name")]
    public void Validate_WithInvalidUsernameCharacters_ShouldFail(string username)
    {
        var command = new RegisterCommand(
            "test@example.com",
            username,
            "Password123",
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username");
    }

    [Theory]
    [InlineData("user123")]
    [InlineData("user_123")]
    [InlineData("user-123")]
    [InlineData("User123")]
    [InlineData("USER123")]
    [InlineData("123user")]
    [InlineData("user_name-123")]
    public void Validate_WithValidUsername_ShouldPass(string username)
    {
        var command = new RegisterCommand(
            "test@example.com",
            username,
            "Password123",
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyPassword_ShouldFail(string password)
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            password,
            "Password123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("required"));
    }

    [Theory]
    [InlineData("pass")]
    [InlineData("1234567")]
    public void Validate_WithPasswordTooShort_ShouldFail(string password)
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            password,
            password
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("8 characters"));
    }

    [Fact]
    public void Validate_WithPasswordTooLong_ShouldFail()
    {
        var longPassword = new string('a', 129);
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            longPassword,
            longPassword
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Password" && e.ErrorMessage.Contains("128"));
    }

    [Fact]
    public void Validate_WithMismatchedPasswords_ShouldFail()
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            "Password123",
            "DifferentPassword123"
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword" && e.ErrorMessage.Contains("do not match"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData(null)]
    public void Validate_WithEmptyConfirmPassword_ShouldFail(string confirmPassword)
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            "Password123",
            confirmPassword
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }
}
