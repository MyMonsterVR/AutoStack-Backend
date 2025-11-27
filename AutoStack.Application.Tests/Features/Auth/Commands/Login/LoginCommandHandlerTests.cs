using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Features.Auth.Commands.Login;
using AutoStack.Application.Tests.Builders;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using RefreshTokenEntity = AutoStack.Domain.Entities.RefreshToken;

namespace AutoStack.Application.Tests.Features.Auth.Commands.Login;

public class LoginCommandHandlerTests : CommandHandlerTestBase
{
    private readonly Mock<IAuthentication> _mockAuthentication;
    private readonly Mock<IToken> _mockToken;
    private readonly LoginCommandHandler _handler;

    public LoginCommandHandlerTests()
    {
        _mockAuthentication = new Mock<IAuthentication>();
        _mockToken = new Mock<IToken>();

        _handler = new LoginCommandHandler(
            _mockAuthentication.Object,
            MockUserRepository.Object,
            MockRefreshTokenRepository.Object,
            _mockToken.Object,
            MockUnitOfWork.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSuccess()
    {
        var userId = Guid.NewGuid();
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .Build();

        var command = new LoginCommand("testuser", "password123");

        _mockAuthentication.Setup(a => a.ValidateAuthenticationAsync(
                "testuser",
                "password123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var accessToken = "mock_access_token";
        _mockToken.Setup(t => t.GenerateAccessToken(userId, user.Username, user.Email))
            .Returns(accessToken);

        var refreshToken = RefreshTokenEntity.Create("mock_refresh_token", userId, GetFutureUnixTime(30));
        _mockToken.Setup(t => t.GenerateRefreshToken(userId))
            .Returns(refreshToken);

        MockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be(accessToken);
        result.Value.RefreshToken.Should().Be("mock_refresh_token");
    }

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldReturnFailure()
    {
        var command = new LoginCommand("testuser", "wrongpassword");

        _mockAuthentication.Setup(a => a.ValidateAuthenticationAsync(
                "testuser",
                "wrongpassword",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Guid?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid username or password");

        MockUserRepository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithEmptyGuidFromAuthentication_ShouldReturnFailure()
    {
        var command = new LoginCommand("testuser", "password123");

        _mockAuthentication.Setup(a => a.ValidateAuthenticationAsync(
                "testuser",
                "password123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(Guid.Empty);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Invalid username or password");
    }

    [Fact]
    public async Task Handle_WhenUserNotFoundInRepository_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var command = new LoginCommand("testuser", "password123");

        _mockAuthentication.Setup(a => a.ValidateAuthenticationAsync(
                "testuser",
                "password123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found");

        _mockToken.Verify(t => t.GenerateAccessToken(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldConvertUsernameToLowerCase()
    {
        var userId = Guid.NewGuid();
        var user = new UserBuilder().Build();
        var command = new LoginCommand("TestUser", "password123");

        _mockAuthentication.Setup(a => a.ValidateAuthenticationAsync(
                "testuser", // Should be lowercase
                "password123",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockToken.Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("token");

        var refreshToken = RefreshTokenEntity.Create("refresh", userId, GetFutureUnixTime(30));
        _mockToken.Setup(t => t.GenerateRefreshToken(userId))
            .Returns(refreshToken);

        MockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        _mockAuthentication.Verify(a => a.ValidateAuthenticationAsync(
            "testuser",
            "password123",
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGenerateAccessTokenWithCorrectParameters()
    {
        var userId = Guid.NewGuid();
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .Build();

        var command = new LoginCommand("testuser", "password123");

        _mockAuthentication.Setup(a => a.ValidateAuthenticationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockToken.Setup(t => t.GenerateAccessToken(userId, user.Username, user.Email))
            .Returns("access_token");

        var refreshToken = RefreshTokenEntity.Create("refresh_token", userId, GetFutureUnixTime(30));
        _mockToken.Setup(t => t.GenerateRefreshToken(userId))
            .Returns(refreshToken);

        MockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        _mockToken.Verify(t => t.GenerateAccessToken(
            userId,
            user.Username,
            user.Email), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGenerateRefreshToken()
    {
        var userId = Guid.NewGuid();
        var user = new UserBuilder().Build();
        var command = new LoginCommand("testuser", "password123");

        _mockAuthentication.Setup(a => a.ValidateAuthenticationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockToken.Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("access_token");

        var refreshToken = RefreshTokenEntity.Create("refresh_token", userId, GetFutureUnixTime(30));
        _mockToken.Setup(t => t.GenerateRefreshToken(userId))
            .Returns(refreshToken);

        MockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        _mockToken.Verify(t => t.GenerateRefreshToken(userId), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveRefreshTokenToRepository()
    {
        var userId = Guid.NewGuid();
        var user = new UserBuilder().Build();
        var command = new LoginCommand("testuser", "password123");

        _mockAuthentication.Setup(a => a.ValidateAuthenticationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockToken.Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("access_token");

        var refreshToken = RefreshTokenEntity.Create("refresh_token", userId, GetFutureUnixTime(30));
        _mockToken.Setup(t => t.GenerateRefreshToken(userId))
            .Returns(refreshToken);

        MockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockRefreshTokenRepository.Verify(r => r.AddAsync(
            It.Is<RefreshTokenEntity>(rt => rt.Token == "refresh_token" && rt.UserId == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var userId = Guid.NewGuid();
        var user = new UserBuilder().Build();
        var command = new LoginCommand("testuser", "password123");

        _mockAuthentication.Setup(a => a.ValidateAuthenticationAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockToken.Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("access_token");

        var refreshToken = RefreshTokenEntity.Create("refresh_token", userId, GetFutureUnixTime(30));
        _mockToken.Setup(t => t.GenerateRefreshToken(userId))
            .Returns(refreshToken);

        MockRefreshTokenRepository.Setup(r => r.AddAsync(It.IsAny<RefreshTokenEntity>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    // Helper method
    private static int GetFutureUnixTime(int daysInFuture)
    {
        return (int)DateTime.UtcNow.AddDays(daysInFuture).Subtract(new DateTime(1970, 1, 1)).TotalSeconds;
    }
}
