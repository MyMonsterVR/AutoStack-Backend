using AutoStack.Application.Common.Interfaces.Auth;
using AutoStack.Application.Features.Auth.Commands.RefreshToken;
using AutoStack.Application.Tests.Builders;
using AutoStack.Application.Tests.Common;
using FluentAssertions;
using Moq;
using Xunit;
using RefreshTokenEntity = AutoStack.Domain.Entities.RefreshToken;

namespace AutoStack.Application.Tests.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandlerTests : CommandHandlerTestBase
{
    private readonly Mock<IToken> _mockToken;
    private readonly RefreshTokenCommandHandler _handler;

    public RefreshTokenCommandHandlerTests()
    {
        _mockToken = new Mock<IToken>();

        _handler = new RefreshTokenCommandHandler(
            MockUserRepository.Object,
            MockRefreshTokenRepository.Object,
            _mockToken.Object,
            MockUnitOfWork.Object,
            MockAuditLog.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidRefreshToken_ShouldReturnNewTokens()
    {
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .Build();

        var userId = user.Id;
        var existingRefreshToken = RefreshTokenEntity.Create("existing_token", userId, GetFutureUnixTime(30));
        var command = new RefreshTokenCommand("existing_token");

        MockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(
                "existing_token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRefreshToken);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var newAccessToken = "new_access_token";
        _mockToken.Setup(t => t.GenerateAccessToken(user.Id, user.Username, user.Email))
            .Returns(newAccessToken);

        var newRefreshToken = RefreshTokenEntity.Create("new_refresh_token", userId, GetFutureUnixTime(30));
        _mockToken.Setup(t => t.GenerateRefreshToken(userId))
            .Returns(newRefreshToken);

        MockRefreshTokenRepository.Setup(r => r.DeleteAsync(
                It.IsAny<RefreshTokenEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockRefreshTokenRepository.Setup(r => r.AddAsync(
                It.IsAny<RefreshTokenEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.AccessToken.Should().Be(newAccessToken);
        result.Value.RefreshToken.Should().Be("new_refresh_token");
    }

    [Fact]
    public async Task Handle_WithNonExistentRefreshToken_ShouldReturnFailure()
    {
        var command = new RefreshTokenCommand("non_existent_token");

        MockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(
                "non_existent_token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((RefreshTokenEntity?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("RefreshToken not found");

        MockUserRepository.Verify(r => r.GetByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithExpiredRefreshToken_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var command = new RefreshTokenCommand("expired_token");

        // Create an expired refresh token using reflection to bypass validation
        // We can't use Create() because it validates expiration is in future
        var expiredToken = (RefreshTokenEntity)Activator.CreateInstance(
            typeof(RefreshTokenEntity),
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic,
            null,
            new object[] { Guid.NewGuid(), "expired_token", userId, GetPastUnixTime(1) },
            null)!;

        MockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(
                "expired_token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expiredToken);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("RefreshToken has expired");

        MockUserRepository.Verify(r => r.GetByIdAsync(
            It.IsAny<Guid>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var refreshToken = RefreshTokenEntity.Create("valid_token", userId, GetFutureUnixTime(30));
        var command = new RefreshTokenCommand("valid_token");

        MockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(
                "valid_token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(refreshToken);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((AutoStack.Domain.Entities.User?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found");

        _mockToken.Verify(t => t.GenerateAccessToken(
            It.IsAny<Guid>(),
            It.IsAny<string>(),
            It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteOldRefreshToken()
    {
        var user = new UserBuilder().Build();
        var userId = user.Id;
        var existingRefreshToken = RefreshTokenEntity.Create("existing_token", userId, GetFutureUnixTime(30));
        var command = new RefreshTokenCommand("existing_token");

        MockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(
                "existing_token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRefreshToken);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockToken.Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("new_access_token");

        var newRefreshToken = RefreshTokenEntity.Create("new_refresh_token", userId, GetFutureUnixTime(30));
        _mockToken.Setup(t => t.GenerateRefreshToken(userId))
            .Returns(newRefreshToken);

        MockRefreshTokenRepository.Setup(r => r.DeleteAsync(
                It.IsAny<RefreshTokenEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockRefreshTokenRepository.Setup(r => r.AddAsync(
                It.IsAny<RefreshTokenEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockRefreshTokenRepository.Verify(r => r.DeleteAsync(
            It.Is<RefreshTokenEntity>(rt => rt.Token == "existing_token"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldAddNewRefreshToken()
    {
        var user = new UserBuilder().Build();
        var userId = user.Id;
        var existingRefreshToken = RefreshTokenEntity.Create("existing_token", userId, GetFutureUnixTime(30));
        var command = new RefreshTokenCommand("existing_token");

        MockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(
                "existing_token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRefreshToken);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockToken.Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("new_access_token");

        var newRefreshToken = RefreshTokenEntity.Create("new_refresh_token", userId, GetFutureUnixTime(30));
        _mockToken.Setup(t => t.GenerateRefreshToken(userId))
            .Returns(newRefreshToken);

        MockRefreshTokenRepository.Setup(r => r.DeleteAsync(
                It.IsAny<RefreshTokenEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockRefreshTokenRepository.Setup(r => r.AddAsync(
                It.IsAny<RefreshTokenEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockRefreshTokenRepository.Verify(r => r.AddAsync(
            It.Is<RefreshTokenEntity>(rt => rt.Token == "new_refresh_token" && rt.UserId == userId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGenerateNewAccessToken()
    {
        var user = new UserBuilder()
            .WithEmail("test@example.com")
            .WithUsername("testuser")
            .Build();

        var userId = user.Id;
        var existingRefreshToken = RefreshTokenEntity.Create("existing_token", userId, GetFutureUnixTime(30));
        var command = new RefreshTokenCommand("existing_token");

        MockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(
                "existing_token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRefreshToken);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockToken.Setup(t => t.GenerateAccessToken(user.Id, user.Username, user.Email))
            .Returns("new_access_token");

        var newRefreshToken = RefreshTokenEntity.Create("new_refresh_token", userId, GetFutureUnixTime(30));
        _mockToken.Setup(t => t.GenerateRefreshToken(userId))
            .Returns(newRefreshToken);

        MockRefreshTokenRepository.Setup(r => r.DeleteAsync(
                It.IsAny<RefreshTokenEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockRefreshTokenRepository.Setup(r => r.AddAsync(
                It.IsAny<RefreshTokenEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        _mockToken.Verify(t => t.GenerateAccessToken(
            user.Id,
            user.Username,
            user.Email), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var user = new UserBuilder().Build();
        var userId = user.Id;
        var existingRefreshToken = RefreshTokenEntity.Create("existing_token", userId, GetFutureUnixTime(30));
        var command = new RefreshTokenCommand("existing_token");

        MockRefreshTokenRepository.Setup(r => r.GetByTokenAsync(
                "existing_token",
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingRefreshToken);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        _mockToken.Setup(t => t.GenerateAccessToken(It.IsAny<Guid>(), It.IsAny<string>(), It.IsAny<string>()))
            .Returns("new_access_token");

        var newRefreshToken = RefreshTokenEntity.Create("new_refresh_token", userId, GetFutureUnixTime(30));
        _mockToken.Setup(t => t.GenerateRefreshToken(userId))
            .Returns(newRefreshToken);

        MockRefreshTokenRepository.Setup(r => r.DeleteAsync(
                It.IsAny<RefreshTokenEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockRefreshTokenRepository.Setup(r => r.AddAsync(
                It.IsAny<RefreshTokenEntity>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static int GetFutureUnixTime(int daysInFuture)
    {
        return (int)DateTime.UtcNow.AddDays(daysInFuture).Subtract(DateTime.UnixEpoch).TotalSeconds;
    }

    private static int GetPastUnixTime(int daysInPast)
    {
        return (int)DateTime.UtcNow.AddDays(-daysInPast).Subtract(DateTime.UnixEpoch).TotalSeconds;
    }
}
