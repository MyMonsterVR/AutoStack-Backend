using AutoStack.Application.Common.Behaviours;
using AutoStack.Application.Common.Models;
using FluentValidation;
using FluentValidation.Results;
using FluentAssertions;
using MediatR;
using Moq;
using System.Runtime.CompilerServices;
using Xunit;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2, PublicKey=0024000004800000940000000602000000240000525341310004000001000100c547cac37abd99c8db225ef2f6c8a3602f3b3606cc9891605d02baa56104f4cfc0734aa39b93bf7852f7d9266654753cc297e7d2edfe0bac1cdcf9f717241550e0a7b191195b7667bb4f64bcb8e2121380fd1d9d46ad2d92d2d15605093924cceaf74c4861eff62abf69b9291ed0a340e113be11e6a7d3113e92484cf7045cc7")]

namespace AutoStack.Application.Tests.Common.Behaviours;

public class ValidationBehaviourTests
{
    [Fact]
    public async Task Handle_WithNoValidators_ShouldCallNext()
    {
        var validators = Enumerable.Empty<IValidator<TestRequest>>();
        var behaviour = new ValidationBehaviour<TestRequest, Result<bool>>(validators);
        var request = new TestRequest { Value = "test" };
        var nextCalled = false;

        Task<Result<bool>> Next(CancellationToken ct)
        {
            nextCalled = true;
            return Task.FromResult(Result<bool>.Success(true));
        }

        var result = await behaviour.Handle(request, Next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithValidRequest_ShouldCallNext()
    {
        var mockValidator = new Mock<IValidator<TestRequest>>();
        mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var validators = new[] { mockValidator.Object };
        var behaviour = new ValidationBehaviour<TestRequest, Result<bool>>(validators);
        var request = new TestRequest { Value = "test" };
        var nextCalled = false;

        Task<Result<bool>> Next(CancellationToken ct)
        {
            nextCalled = true;
            return Task.FromResult(Result<bool>.Success(true));
        }

        var result = await behaviour.Handle(request, Next, CancellationToken.None);

        nextCalled.Should().BeTrue();
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithInvalidRequest_ShouldReturnFailureAndNotCallNext()
    {
        var validationFailures = new List<ValidationFailure>
        {
            new("Value", "Value is required")
        };

        var mockValidator = new Mock<IValidator<TestRequest>>();
        mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var validators = new[] { mockValidator.Object };
        var behaviour = new ValidationBehaviour<TestRequest, Result<bool>>(validators);
        var request = new TestRequest { Value = null };
        var nextCalled = false;

        Task<Result<bool>> Next(CancellationToken ct)
        {
            nextCalled = true;
            return Task.FromResult(Result<bool>.Success(true));
        }

        var result = await behaviour.Handle(request, Next, CancellationToken.None);

        nextCalled.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().ContainKey("Value");
        result.ValidationErrors!["Value"].Should().Contain("Value is required");
    }

    [Fact]
    public async Task Handle_WithMultipleValidationErrors_ShouldReturnAllErrors()
    {
        var validationFailures = new List<ValidationFailure>
        {
            new("Value", "Value is required"),
            new("Value", "Value must be at least 3 characters"),
            new("Name", "Name is required")
        };

        var mockValidator = new Mock<IValidator<TestRequest>>();
        mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var validators = new[] { mockValidator.Object };
        var behaviour = new ValidationBehaviour<TestRequest, Result<bool>>(validators);
        var request = new TestRequest { Value = "" };

        Task<Result<bool>> Next(CancellationToken ct)
        {
            return Task.FromResult(Result<bool>.Success(true));
        }

        var result = await behaviour.Handle(request, Next, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().HaveCount(2);
        result.ValidationErrors.Should().ContainKey("Value");
        result.ValidationErrors.Should().ContainKey("Name");
        result.ValidationErrors!["Value"].Should().HaveCount(2);
        result.ValidationErrors["Name"].Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithMultipleValidators_ShouldRunAllValidators()
    {
        var validator1Failures = new List<ValidationFailure>
        {
            new("Value", "Error from validator 1")
        };

        var validator2Failures = new List<ValidationFailure>
        {
            new("Name", "Error from validator 2")
        };

        var mockValidator1 = new Mock<IValidator<TestRequest>>();
        mockValidator1.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validator1Failures));

        var mockValidator2 = new Mock<IValidator<TestRequest>>();
        mockValidator2.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validator2Failures));

        var validators = new[] { mockValidator1.Object, mockValidator2.Object };
        var behaviour = new ValidationBehaviour<TestRequest, Result<bool>>(validators);
        var request = new TestRequest { Value = "" };

        Task<Result<bool>> Next(CancellationToken ct)
        {
            return Task.FromResult(Result<bool>.Success(true));
        }

        var result = await behaviour.Handle(request, Next, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().ContainKey("Value");
        result.ValidationErrors.Should().ContainKey("Name");
        result.ValidationErrors!["Value"].Should().Contain("Error from validator 1");
        result.ValidationErrors["Name"].Should().Contain("Error from validator 2");

        mockValidator1.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        mockValidator2.Verify(v => v.ValidateAsync(
            It.IsAny<ValidationContext<TestRequest>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithOneValidatorPassingAndOneFailing_ShouldReturnFailure()
    {
        var failingValidatorFailures = new List<ValidationFailure>
        {
            new("Value", "Validation failed")
        };

        var mockPassingValidator = new Mock<IValidator<TestRequest>>();
        mockPassingValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        var mockFailingValidator = new Mock<IValidator<TestRequest>>();
        mockFailingValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(failingValidatorFailures));

        var validators = new[] { mockPassingValidator.Object, mockFailingValidator.Object };
        var behaviour = new ValidationBehaviour<TestRequest, Result<bool>>(validators);
        var request = new TestRequest { Value = "" };
        var nextCalled = false;

        Task<Result<bool>> Next(CancellationToken ct)
        {
            nextCalled = true;
            return Task.FromResult(Result<bool>.Success(true));
        }

        var result = await behaviour.Handle(request, Next, CancellationToken.None);

        nextCalled.Should().BeFalse();
        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().ContainKey("Value");
    }

    [Fact]
    public async Task Handle_ShouldPassCancellationTokenToValidators()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        var mockValidator = new Mock<IValidator<TestRequest>>();
        CancellationToken capturedToken = default;

        mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<IValidationContext>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult())
            .Callback<IValidationContext, CancellationToken>((_, ct) => capturedToken = ct);

        var validators = new[] { mockValidator.Object };
        var behaviour = new ValidationBehaviour<TestRequest, Result<bool>>(validators);
        var request = new TestRequest { Value = "test" };

        Task<Result<bool>> Next(CancellationToken ct)
        {
            return Task.FromResult(Result<bool>.Success(true));
        }

        await behaviour.Handle(request, Next, cancellationTokenSource.Token);

        capturedToken.Should().Be(cancellationTokenSource.Token);
    }

    [Fact]
    public async Task Handle_WithComplexValidationErrors_ShouldGroupByProperty()
    {
        var validationFailures = new List<ValidationFailure>
        {
            new("Email", "Email is required"),
            new("Email", "Email must be valid"),
            new("Password", "Password is required"),
            new("Password", "Password must be at least 8 characters"),
            new("Password", "Password must contain uppercase letter"),
            new("ConfirmPassword", "Passwords do not match")
        };

        var mockValidator = new Mock<IValidator<TestRequest>>();
        mockValidator.Setup(v => v.ValidateAsync(
                It.IsAny<ValidationContext<TestRequest>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult(validationFailures));

        var validators = new[] { mockValidator.Object };
        var behaviour = new ValidationBehaviour<TestRequest, Result<bool>>(validators);
        var request = new TestRequest { Value = "" };

        Task<Result<bool>> Next(CancellationToken ct)
        {
            return Task.FromResult(Result<bool>.Success(true));
        }

        var result = await behaviour.Handle(request, Next, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ValidationErrors.Should().HaveCount(3);
        result.ValidationErrors!["Email"].Should().HaveCount(2);
        result.ValidationErrors["Password"].Should().HaveCount(3);
        result.ValidationErrors["ConfirmPassword"].Should().ContainSingle();
    }

    // Test request class - must be internal for Moq to work with FluentValidation
    internal class TestRequest
    {
        public string? Value { get; set; }
        public string? Name { get; set; }
    }
}
