using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Features.Auth.Commands.Register;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AutoStack.Application.Tests.Features.Auth.Commands.Register;

public class RegisterCommandHandlerTests : CommandHandlerTestBase
{
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly Mock<IEmailService> _mockEmailService;
    private readonly Mock<IConfiguration> _mockConfiguration;
    private readonly RegisterCommandHandler _handler;

    public RegisterCommandHandlerTests()
    {
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _mockEmailService = new Mock<IEmailService>();
        _mockConfiguration = new Mock<IConfiguration>();

        _mockConfiguration.Setup(c => c.GetSection("EmailVerification:ExpiryMinutes").Value)
            .Returns("15");

        _handler = new RegisterCommandHandler(
            MockUserRepository.Object,
            _mockPasswordHasher.Object,
            MockUnitOfWork.Object,
            MockAuditLog.Object,
            _mockEmailService.Object,
            _mockConfiguration.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidData_ShouldReturnSuccess()
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            "password123",
            "password123"
        );

        MockUserRepository.Setup(r => r.EmailExists(
                "test@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        MockUserRepository.Setup(r => r.UsernameExists(
                "testuser",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockPasswordHasher.Setup(p => p.HashPassword("password123"))
            .Returns("hashed_password_123");

        MockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Handle_WithExistingEmail_ShouldReturnFailure()
    {
        var command = new RegisterCommand(
            "existing@example.com",
            "testuser",
            "password123",
            "password123"
        );

        MockUserRepository.Setup(r => r.EmailExists(
                "existing@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Email already exists");

        MockUserRepository.Verify(r => r.UsernameExists(
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExistingUsername_ShouldReturnFailure()
    {
        var command = new RegisterCommand(
            "test@example.com",
            "existinguser",
            "password123",
            "password123"
        );

        MockUserRepository.Setup(r => r.EmailExists(
                "test@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        MockUserRepository.Setup(r => r.UsernameExists(
                "existinguser",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Username already exists");

        _mockPasswordHasher.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithMismatchedPasswords_ShouldReturnFailure()
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            "password123",
            "differentpassword"
        );

        MockUserRepository.Setup(r => r.EmailExists(
                "test@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        MockUserRepository.Setup(r => r.UsernameExists(
                "testuser",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Passwords do not match");

        _mockPasswordHasher.Verify(p => p.HashPassword(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldConvertEmailToLowerCase()
    {
        var command = new RegisterCommand(
            "Test@Example.COM",
            "testuser",
            "password123",
            "password123"
        );

        MockUserRepository.Setup(r => r.EmailExists(
                "test@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        MockUserRepository.Setup(r => r.UsernameExists(
                "testuser",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        MockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUserRepository.Verify(r => r.EmailExists(
            "test@example.com",
            It.IsAny<CancellationToken>()), Times.Once);

        MockUserRepository.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Email == "test@example.com"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldConvertUsernameToLowerCase()
    {
        var command = new RegisterCommand(
            "test@example.com",
            "TestUser",
            "password123",
            "password123"
        );

        MockUserRepository.Setup(r => r.EmailExists(
                "test@example.com",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        MockUserRepository.Setup(r => r.UsernameExists(
                "testuser",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        MockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUserRepository.Verify(r => r.UsernameExists(
            "testuser",
            It.IsAny<CancellationToken>()), Times.Once);

        MockUserRepository.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Username == "testuser"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldHashPassword()
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            "password123",
            "password123"
        );

        MockUserRepository.Setup(r => r.EmailExists(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        MockUserRepository.Setup(r => r.UsernameExists(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockPasswordHasher.Setup(p => p.HashPassword("password123"))
            .Returns("hashed_password_123");

        MockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        _mockPasswordHasher.Verify(p => p.HashPassword("password123"), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCreateUserWithHashedPassword()
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            "password123",
            "password123"
        );

        MockUserRepository.Setup(r => r.EmailExists(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        MockUserRepository.Setup(r => r.UsernameExists(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var hashedPassword = "hashed_password_123";
        _mockPasswordHasher.Setup(p => p.HashPassword("password123"))
            .Returns(hashedPassword);

        MockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUserRepository.Verify(r => r.AddAsync(
            It.Is<User>(u => u.PasswordHash == hashedPassword),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddUserToRepository()
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            "password123",
            "password123"
        );

        MockUserRepository.Setup(r => r.EmailExists(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        MockUserRepository.Setup(r => r.UsernameExists(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        MockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUserRepository.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Email == "test@example.com" && u.Username == "testuser"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            "password123",
            "password123"
        );

        MockUserRepository.Setup(r => r.EmailExists(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        MockUserRepository.Setup(r => r.UsernameExists(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        MockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task Handle_ShouldCheckEmailExistenceBeforeUsername()
    {
        var command = new RegisterCommand(
            "test@example.com",
            "testuser",
            "password123",
            "password123"
        );

        var callOrder = new List<string>();

        MockUserRepository.Setup(r => r.EmailExists(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Callback(() => callOrder.Add("EmailExists"));

        MockUserRepository.Setup(r => r.UsernameExists(
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false)
            .Callback(() => callOrder.Add("UsernameExists"));

        _mockPasswordHasher.Setup(p => p.HashPassword(It.IsAny<string>()))
            .Returns("hashed_password");

        MockUserRepository.Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        callOrder.Should().HaveCount(2);
        callOrder[0].Should().Be("EmailExists");
        callOrder[1].Should().Be("UsernameExists");
    }
}
