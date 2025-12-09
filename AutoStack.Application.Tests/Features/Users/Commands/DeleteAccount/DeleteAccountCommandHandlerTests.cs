using AutoStack.Application.Common.Models;
using AutoStack.Application.Features.Users.Commands.DeleteAccount;
using AutoStack.Application.Tests.Builders;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Enums;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStack.Application.Tests.Features.Users.Commands.DeleteAccount;

public class DeleteAccountCommandHandlerTests : CommandHandlerTestBase
{
    private readonly DeleteAccountCommandHandler _handler;

    public DeleteAccountCommandHandlerTests()
    {
        _handler = new DeleteAccountCommandHandler(
            MockUserRepository.Object,
            MockUnitOfWork.Object,
            MockAuditLog.Object
        );
    }

    [Fact]
    public async Task Handle_WithoutUserId_ShouldReturnFailure()
    {
        var command = new DeleteAccountCommand(null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User ID is required");
    }

    [Fact]
    public async Task Handle_WithNonExistingUserId_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var command = new DeleteAccountCommand(userId);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found.");
        
        // Makes sure DeleteAsync was not called
        MockUserRepository.Verify(r => r.DeleteAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingUserId_ShouldReturnSuccess()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var command = new DeleteAccountCommand(user.Id);
        var result = await _handler.Handle(command, CancellationToken.None);
        result.IsSuccess.Should().BeTrue();
        
        MockUserRepository.Verify(r => r.DeleteAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        MockUnitOfWork.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        MockAuditLog.Verify(a => a.LogAsync(
                It.Is<AuditLogRequest>(req =>
                    req.Level == LogLevel.Information &&
                    req.Category == LogCategory.User &&
                    req.Message == "Account deleted" &&
                    req.UserIdOverride == user.Id &&
                    req.UsernameOverride == user.Username),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    
    [Fact]
    public async Task Handle_WhenDeleteThrowsException_ShouldReturnFailure()
    {
        var user = new UserBuilder().Build();

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.DeleteAsync(user, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Database error"));

        var command = new DeleteAccountCommand(user.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Database error");
        MockUnitOfWork.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenSaveChangesThrowsException_ShouldReturnFailure()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Save failed"));

        var command = new DeleteAccountCommand(user.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Save failed");
    }

    [Fact]
    public async Task Handle_WhenAuditLogFails_ShouldStillReturnSuccess()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockAuditLog.Setup(a => a.LogAsync(It.IsAny<AuditLogRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("Logging failed"));

        var command = new DeleteAccountCommand(user.Id);
        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        MockUserRepository.Verify(r => r.DeleteAsync(user, It.IsAny<CancellationToken>()), Times.Once);
        MockUnitOfWork.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}