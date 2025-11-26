using AutoStack.Application.Common.Models;
using FluentAssertions;
using Xunit;

namespace AutoStack.Application.Tests.Common.Models;

public class ResultTests
{
    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        var result = Result.Success();

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Success_ShouldHaveEmptyMessage()
    {
        var result = Result.Success();

        result.Message.Should().BeEmpty();
    }

    [Fact]
    public void Failure_WithErrorMessage_ShouldCreateFailedResult()
    {
        var errorMessage = "Something went wrong";

        var result = Result.Failure(errorMessage);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be(errorMessage);
    }

    [Fact]
    public void Failure_WithValidationErrors_ShouldStoreValidationErrors()
    {
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Name is required", "Name must be at least 3 characters" } },
            { "Email", new[] { "Email is invalid" } }
        };

        var result = Result.Failure(validationErrors);

        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
        result.ValidationErrors.Should().NotBeNull();
        result.ValidationErrors.Should().BeEquivalentTo(validationErrors);
    }

    [Fact]
    public void Failure_WithValidationErrors_ShouldHaveValidationFailedMessage()
    {
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Field", new[] { "Error" } }
        };

        var result = Result.Failure(validationErrors);

        result.Message.Should().Be("Validation failed");
    }
}
