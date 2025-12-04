using AutoStack.Application.Features.Users.Commands.EditUser;
using FluentAssertions;
using Xunit;

namespace AutoStack.Application.Tests.Features.Users.Commands.EditUser;

public class EditUserCommandValidatorTests
{
    private readonly EditUserCommandValidator _validator;

    public EditUserCommandValidatorTests()
    {
        _validator = new EditUserCommandValidator();
    }

    [Fact]
    public void Validate_WithValidData_ShouldPass()
    {
        var command = new EditUserCommand(
            "newusername",
            "newemail@example.com",
            null,
            null,
            null,
            Guid.NewGuid()
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
        var command = new EditUserCommand(
            "testuser",
            email,
            null,
            null,
            null,
            Guid.NewGuid()
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
        var command = new EditUserCommand(
            "testuser",
            email,
            null,
            null,
            null,
            Guid.NewGuid()
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void Validate_WithEmailTooLong_ShouldFail()
    {
        var longEmail = new string('a', 250) + "@example.com";
        var command = new EditUserCommand(
            "testuser",
            longEmail,
            null,
            null,
            null,
            Guid.NewGuid()
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
        var command = new EditUserCommand(
            username,
            "test@example.com",
            null,
            null,
            null,
            Guid.NewGuid()
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
        var command = new EditUserCommand(
            username,
            "test@example.com",
            null,
            null,
            null,
            Guid.NewGuid()
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("3 characters"));
    }

    [Fact]
    public void Validate_WithUsernameTooLong_ShouldFail()
    {
        var longUsername = new string('a', 31);
        var command = new EditUserCommand(
            longUsername,
            "test@example.com",
            null,
            null,
            null,
            Guid.NewGuid()
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("30"));
    }

    [Theory]
    [InlineData("test user")]
    [InlineData("user name")]
    public void Validate_WithUsernameContainingSpaces_ShouldFail(string username)
    {
        var command = new EditUserCommand(
            username,
            "test@example.com",
            null,
            null,
            null,
            Guid.NewGuid()
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
        var command = new EditUserCommand(
            username,
            "test@example.com",
            null,
            null,
            null,
            Guid.NewGuid()
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Username" && e.ErrorMessage.Contains("start with a letter or number"));
    }

    [Theory]
    [InlineData("user@name")]
    [InlineData("user.name")]
    [InlineData("user#name")]
    public void Validate_WithInvalidUsernameCharacters_ShouldFail(string username)
    {
        var command = new EditUserCommand(
            username,
            "test@example.com",
            null,
            null,
            null,
            Guid.NewGuid()
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
    [InlineData("123user")]
    public void Validate_WithValidUsername_ShouldPass(string username)
    {
        var command = new EditUserCommand(
            username,
            "test@example.com",
            null,
            null,
            null,
            Guid.NewGuid()
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithCurrentPasswordButNoNewPassword_ShouldFail()
    {
        var command = new EditUserCommand(
            "testuser",
            "test@example.com",
            "CurrentPassword123",
            null,
            null,
            Guid.NewGuid()
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword");
    }

    [Fact]
    public void Validate_WithNewPasswordTooShort_ShouldFail()
    {
        var command = new EditUserCommand(
            "testuser",
            "test@example.com",
            "CurrentPassword123",
            "short",
            "short",
            Guid.NewGuid()
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("8 characters"));
    }

    [Fact]
    public void Validate_WithNewPasswordTooLong_ShouldFail()
    {
        var longPassword = new string('a', 129);
        var command = new EditUserCommand(
            "testuser",
            "test@example.com",
            "CurrentPassword123",
            longPassword,
            longPassword,
            Guid.NewGuid()
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "NewPassword" && e.ErrorMessage.Contains("128"));
    }

    [Fact]
    public void Validate_WithMismatchedNewPasswords_ShouldFail()
    {
        var command = new EditUserCommand(
            "testuser",
            "test@example.com",
            "CurrentPassword123",
            "NewPassword123",
            "DifferentPassword123",
            Guid.NewGuid()
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmNewPassword" && e.ErrorMessage.Contains("do not match"));
    }

    [Fact]
    public void Validate_WithValidPasswordChange_ShouldPass()
    {
        var command = new EditUserCommand(
            "testuser",
            "test@example.com",
            "CurrentPassword123",
            "NewPassword123",
            "NewPassword123",
            Guid.NewGuid()
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNoPasswordChange_ShouldPass()
    {
        var command = new EditUserCommand(
            "testuser",
            "test@example.com",
            null,
            null,
            null,
            Guid.NewGuid()
        );

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
