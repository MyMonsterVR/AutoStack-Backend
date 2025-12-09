using AutoStack.Application.Features.Users.Commands.DeleteAccount;
using FluentAssertions;
using Xunit;

namespace AutoStack.Application.Tests.Features.Users.Commands.DeleteAccount;

public class DeleteAccountValidatorTests
{
    private readonly DeleteAccountValidator _validator;

    public DeleteAccountValidatorTests()
    {
        _validator = new DeleteAccountValidator();
    }

    [Fact]
    public void Validate_WithValidUserId_ShouldPass()
    {
        var command = new DeleteAccountCommand(Guid.NewGuid());

        var result = _validator.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithNullUserId_ShouldFail()
    {
        var command = new DeleteAccountCommand(null);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId" && e.ErrorMessage == "UserId is required");
    }

    [Fact]
    public void Validate_WithEmptyGuid_ShouldFail()
    {
        var command = new DeleteAccountCommand(Guid.Empty);

        var result = _validator.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "UserId" && e.ErrorMessage == "UserId is required");
    }
}
