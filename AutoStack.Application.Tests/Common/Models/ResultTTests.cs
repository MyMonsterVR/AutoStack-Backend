using AutoStack.Application.Common.Models;
using FluentAssertions;
using Xunit;

namespace AutoStack.Application.Tests.Common.Models;

public class ResultTTests
{
    [Fact]
    public void Success_WithValue_ShouldCreateSuccessfulResult()
    {
        // Arrange
        var value = "test value";

        // Act
        var result = Result<string>.Success(value);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Success_WithComplexValue_ShouldReturnValue()
    {
        // Arrange
        var value = new TestObject { Id = 1, Name = "Test" };

        // Act
        var result = Result<TestObject>.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
        result.Value.Id.Should().Be(1);
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public void Failure_WithErrorMessage_ShouldCreateFailedResult()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = Result<string>.Failure(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be(errorMessage);
    }

    [Fact]
    public void Failure_ValueAccess_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var result = Result<string>.Failure("Error occurred");

        // Act
        Action act = () => { var _ = result.Value; };

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Cannot access the value of an error.");
    }

    [Fact]
    public void Failure_WithValidationErrors_ShouldStoreErrors()
    {
        // Arrange
        var validationErrors = new Dictionary<string, string[]>
        {
            { "Property1", new[] { "Error1", "Error2" } },
            { "Property2", new[] { "Error3" } }
        };

        // Act
        var result = Result<string>.Failure(validationErrors);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Validation failed");
        result.ValidationErrors.Should().BeEquivalentTo(validationErrors);
    }

    [Fact]
    public void Success_ShouldHaveEmptyMessage()
    {
        // Arrange
        var value = 42;

        // Act
        var result = Result<int>.Success(value);

        // Assert
        result.Message.Should().BeEmpty();
    }

    private class TestObject
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
