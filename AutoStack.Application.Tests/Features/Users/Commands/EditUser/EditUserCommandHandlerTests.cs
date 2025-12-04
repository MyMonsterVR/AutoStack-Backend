using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Common.Models;
using AutoStack.Application.Features.Users.Commands.EditUser;
using AutoStack.Application.Tests.Builders;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStack.Application.Tests.Features.Users.Commands.EditUser;

public class EditUserCommandHandlerTests : CommandHandlerTestBase
{
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly EditUserCommandHandler _handler;

    public EditUserCommandHandlerTests()
    {
        _mockPasswordHasher = new Mock<IPasswordHasher>();

        _handler = new EditUserCommandHandler(
            MockUserRepository.Object,
            MockUnitOfWork.Object,
            _mockPasswordHasher.Object,
            MockAuditLog.Object
        );
    }

    [Fact]
    public async Task Handle_WithoutUserId_ShouldReturnFailure()
    {
        var command = new EditUserCommand(
            "newusername",
            "newemail@example.com",
            null,
            null,
            null,
            null
        );

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User ID is required");
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var command = new EditUserCommand(
            "newusername",
            "newemail@example.com",
            null,
            null,
            null,
            userId
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found.");
    }

    [Fact]
    public async Task Handle_WithUsernameChange_ShouldUpdateUsername()
    {
        var user = new UserBuilder()
            .WithUsername("oldusername")
            .WithEmail("test@example.com")
            .Build();

        var command = new EditUserCommand(
            "newusername",
            "test@example.com",
            null,
            null,
            null,
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("newusername");
    }

    [Fact]
    public async Task Handle_WithEmailChange_ShouldUpdateEmail()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("old@example.com")
            .Build();

        var command = new EditUserCommand(
            "testuser",
            "new@example.com",
            null,
            null,
            null,
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("new@example.com");
    }

    [Fact]
    public async Task Handle_WithPasswordChange_AndValidCurrentPassword_ShouldUpdatePassword()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        user.SetPassword("old_hashed_password");

        var command = new EditUserCommand(
            "testuser",
            "test@example.com",
            "currentpassword",
            "newpassword",
            "newpassword",
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(p => p.VerifyPassword("currentpassword", "old_hashed_password"))
            .Returns(true);

        _mockPasswordHasher.Setup(p => p.HashPassword("newpassword"))
            .Returns("new_hashed_password");

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        user.PasswordHash.Should().Be("new_hashed_password");
    }

    [Fact]
    public async Task Handle_WithPasswordChange_AndInvalidCurrentPassword_ShouldReturnFailure()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        user.SetPassword("old_hashed_password");

        var command = new EditUserCommand(
            "testuser",
            "test@example.com",
            "wrongcurrentpassword",
            "newpassword",
            "newpassword",
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(p => p.VerifyPassword("wrongcurrentpassword", "old_hashed_password"))
            .Returns(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Current password is invalid");

        MockUserRepository.Verify(r => r.UpdateAsync(
            It.IsAny<User>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithPasswordChange_AndMismatchedNewPasswords_ShouldReturnFailure()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        user.SetPassword("old_hashed_password");

        var command = new EditUserCommand(
            "testuser",
            "test@example.com",
            "currentpassword",
            "newpassword",
            "differentpassword",
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(p => p.VerifyPassword("currentpassword", "old_hashed_password"))
            .Returns(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Passwords do not match");

        _mockPasswordHasher.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithNoChanges_ShouldReturnSuccess()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        var command = new EditUserCommand(
            "testuser",
            "test@example.com",
            null,
            null,
            null,
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_ShouldUpdateUserInRepository()
    {
        var user = new UserBuilder()
            .WithUsername("oldusername")
            .WithEmail("test@example.com")
            .Build();

        var command = new EditUserCommand(
            "newusername",
            "test@example.com",
            null,
            null,
            null,
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUserRepository.Verify(r => r.UpdateAsync(
            It.Is<User>(u => u.Username == "newusername"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var user = new UserBuilder()
            .WithUsername("oldusername")
            .WithEmail("test@example.com")
            .Build();

        var command = new EditUserCommand(
            "newusername",
            "test@example.com",
            null,
            null,
            null,
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithChanges_ShouldCallAuditLogService()
    {
        var user = new UserBuilder()
            .WithUsername("oldusername")
            .WithEmail("test@example.com")
            .Build();

        var command = new EditUserCommand(
            "newusername",
            "test@example.com",
            null,
            null,
            null,
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockAuditLog.Verify(a => a.LogAsync(
            It.IsAny<AuditLogRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithInvalidPasswordChange_ShouldLogWarning()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        user.SetPassword("old_hashed_password");

        var command = new EditUserCommand(
            "testuser",
            "test@example.com",
            "wrongpassword",
            "newpassword",
            "newpassword",
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(p => p.VerifyPassword("wrongpassword", "old_hashed_password"))
            .Returns(false);

        await _handler.Handle(command, CancellationToken.None);

        MockAuditLog.Verify(a => a.LogAsync(
            It.IsAny<AuditLogRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithMultipleChanges_ShouldUpdateAllFields()
    {
        var user = new UserBuilder()
            .WithUsername("oldusername")
            .WithEmail("old@example.com")
            .Build();

        user.SetPassword("old_hashed_password");

        var command = new EditUserCommand(
            "newusername",
            "new@example.com",
            "currentpassword",
            "newpassword",
            "newpassword",
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockPasswordHasher.Setup(p => p.VerifyPassword("currentpassword", "old_hashed_password"))
            .Returns(true);

        _mockPasswordHasher.Setup(p => p.HashPassword("newpassword"))
            .Returns("new_hashed_password");

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Username.Should().Be("newusername");
        result.Value.Email.Should().Be("new@example.com");
        user.PasswordHash.Should().Be("new_hashed_password");
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectUserResponse()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        var command = new EditUserCommand(
            "newusername",
            "test@example.com",
            null,
            null,
            null,
            user.Id
        );

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUserRepository.Setup(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Username.Should().Be("newusername");
        result.Value.Email.Should().Be("test@example.com");
    }
}
