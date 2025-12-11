using AutoStack.Application.Features.Auth.Commands.VerifyEmail;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStack.Application.Tests.Features.Auth.Commands.VerifyEmail;

public class VerifyEmailCommandHandlerTests : CommandHandlerTestBase
{
    private readonly VerifyEmailCommandHandler _handler;

    public VerifyEmailCommandHandlerTests()
    {
        _handler = new VerifyEmailCommandHandler(
            MockUserRepository.Object,
            MockUnitOfWork.Object,
            MockAuditLog.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCode_ShouldVerifyEmail()
    {
        var userId = Guid.NewGuid();
        var code = "123456";
        var user = User.CreateUser("test@example.com", "testuser");
        user.SetEmailVerificationCode(code, DateTime.UtcNow.AddMinutes(15));

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new VerifyEmailCommand(userId, code);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.EmailVerified.Should().BeTrue();
        user.EmailVerificationCode.Should().BeNull();
        user.EmailVerifiedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new VerifyEmailCommand(userId, "123456");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task Handle_WithAlreadyVerifiedEmail_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");
        user.SetEmailVerificationCode("123456", DateTime.UtcNow.AddMinutes(15));
        user.VerifyEmail();

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new VerifyEmailCommand(userId, "123456");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Email is already verified");
    }

    [Fact]
    public async Task Handle_WithNoVerificationCode_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new VerifyEmailCommand(userId, "123456");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("No verification code found. Please request a new code.");
    }

    [Fact]
    public async Task Handle_WithExpiredCode_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");
        user.SetEmailVerificationCode("123456", DateTime.UtcNow.AddMinutes(-1)); // Expired

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new VerifyEmailCommand(userId, "123456");
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Verification code has expired. Please request a new code.");
    }

    [Fact]
    public async Task Handle_WithInvalidCode_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");
        user.SetEmailVerificationCode("123456", DateTime.UtcNow.AddMinutes(15));

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new VerifyEmailCommand(userId, "654321"); // Wrong code
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid verification code");
    }

    [Fact]
    public async Task Handle_WithValidCode_ShouldSaveChanges()
    {
        var userId = Guid.NewGuid();
        var code = "123456";
        var user = User.CreateUser("test@example.com", "testuser");
        user.SetEmailVerificationCode(code, DateTime.UtcNow.AddMinutes(15));

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new VerifyEmailCommand(userId, code);
        await _handler.Handle(command, CancellationToken.None);

        MockUserRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
