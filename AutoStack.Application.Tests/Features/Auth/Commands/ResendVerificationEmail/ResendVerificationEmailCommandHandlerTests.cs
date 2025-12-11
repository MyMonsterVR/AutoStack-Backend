using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Features.Auth.Commands.ResendVerificationEmail;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AutoStack.Application.Tests.Features.Auth.Commands.ResendVerificationEmail;

public class ResendVerificationEmailCommandHandlerTests : CommandHandlerTestBase
{
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly ResendVerificationEmailCommandHandler _handler;

    public ResendVerificationEmailCommandHandlerTests()
    {
        _mockEmailService = new Mock<IEmailService>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration.Setup(c => c.GetSection("EmailVerification:ExpiryMinutes").Value)
            .Returns("15");

        _handler = new ResendVerificationEmailCommandHandler(
            MockUserRepository.Object,
            MockUnitOfWork.Object,
            _mockEmailService.Object,
            MockAuditLog.Object,
            _mockConfiguration.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldGenerateAndSendCode()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockEmailService.Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new ResendVerificationEmailCommand(userId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.EmailVerificationCode.Should().NotBeNullOrEmpty();
        user.EmailVerificationCode.Should().HaveLength(6);
        user.EmailVerificationCodeExpiry.Should().BeAfter(DateTime.UtcNow);
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = new ResendVerificationEmailCommand(userId);
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

        var command = new ResendVerificationEmailCommand(userId);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Email is already verified");
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldSendEmail()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockEmailService.Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new ResendVerificationEmailCommand(userId);
        await _handler.Handle(command, CancellationToken.None);

        _mockEmailService.Verify(e => e.SendEmailAsync(
            "test@example.com",
            "Verify Your Email - AutoStack",
            It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldSaveChanges()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockEmailService.Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new ResendVerificationEmailCommand(userId);
        await _handler.Handle(command, CancellationToken.None);

        MockUserRepository.Verify(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithValidUser_ShouldUseConfiguredExpiry()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");

        _mockConfiguration.Setup(c => c.GetSection("EmailVerification:ExpiryMinutes").Value)
            .Returns("30");

        var handler = new ResendVerificationEmailCommandHandler(
            MockUserRepository.Object,
            MockUnitOfWork.Object,
            _mockEmailService.Object,
            MockAuditLog.Object,
            _mockConfiguration.Object
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _mockEmailService.Setup(e => e.SendEmailAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string>()))
            .Returns(Task.CompletedTask);

        var command = new ResendVerificationEmailCommand(userId);
        await handler.Handle(command, CancellationToken.None);

        user.EmailVerificationCodeExpiry.Should().BeCloseTo(DateTime.UtcNow.AddMinutes(30), TimeSpan.FromSeconds(5));
    }
}
