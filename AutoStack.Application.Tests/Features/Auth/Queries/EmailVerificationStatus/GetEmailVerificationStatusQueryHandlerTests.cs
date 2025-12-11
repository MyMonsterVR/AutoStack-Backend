using AutoStack.Application.Features.Auth.Queries.EmailVerificationStatus;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStack.Application.Tests.Features.Auth.Queries.EmailVerificationStatus;

public class GetEmailVerificationStatusQueryHandlerTests : QueryHandlerTestBase
{
    private readonly GetEmailVerificationStatusQueryHandler _handler;

    public GetEmailVerificationStatusQueryHandlerTests()
    {
        _handler = new GetEmailVerificationStatusQueryHandler(MockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_WithVerifiedUser_ShouldReturnVerifiedStatus()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");
        user.SetEmailVerificationCode("123456", DateTime.UtcNow.AddMinutes(15));
        user.VerifyEmail();

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetEmailVerificationStatusQuery(userId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsVerified.Should().BeTrue();
        result.Value.VerifiedAt.Should().NotBeNull();
        result.Value.HasPendingCode.Should().BeFalse();
        result.Value.CodeExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithUnverifiedUserWithValidCode_ShouldReturnCorrectStatus()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");
        var expiryTime = DateTime.UtcNow.AddMinutes(15);
        user.SetEmailVerificationCode("123456", expiryTime);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetEmailVerificationStatusQuery(userId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsVerified.Should().BeFalse();
        result.Value.VerifiedAt.Should().BeNull();
        result.Value.HasPendingCode.Should().BeTrue();
        result.Value.CodeExpiresAt.Should().BeCloseTo(expiryTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WithUnverifiedUserWithExpiredCode_ShouldReturnCorrectStatus()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");
        var expiryTime = DateTime.UtcNow.AddMinutes(-5); // Expired
        user.SetEmailVerificationCode("123456", expiryTime);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetEmailVerificationStatusQuery(userId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsVerified.Should().BeFalse();
        result.Value.VerifiedAt.Should().BeNull();
        result.Value.HasPendingCode.Should().BeFalse();
        result.Value.CodeExpiresAt.Should().BeCloseTo(expiryTime, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task Handle_WithUnverifiedUserWithNoCode_ShouldReturnCorrectStatus()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var query = new GetEmailVerificationStatusQuery(userId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.IsVerified.Should().BeFalse();
        result.Value.VerifiedAt.Should().BeNull();
        result.Value.HasPendingCode.Should().BeFalse();
        result.Value.CodeExpiresAt.Should().BeNull();
    }

    [Fact]
    public async Task Handle_WithNonExistentUser_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var query = new GetEmailVerificationStatusQuery(userId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found");
    }
}
