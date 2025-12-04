using AutoStack.Application.Features.Users.Queries.GetUser;
using AutoStack.Application.Tests.Builders;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStack.Application.Tests.Features.Users.Queries.GetUser;

public class GetUserQueryHandlerTests : QueryHandlerTestBase
{
    private readonly GetUserQueryHandler _handler;

    public GetUserQueryHandlerTests()
    {
        _handler = new GetUserQueryHandler(MockUserRepository.Object);
    }

    [Fact]
    public async Task Handle_WithValidUserId_ShouldReturnUserResponse()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        var query = new GetUserQuery(user.Id);

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Username.Should().Be("testuser");
        result.Value.Email.Should().Be("test@example.com");
    }

    [Fact]
    public async Task Handle_WithNonExistentUserId_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var query = new GetUserQuery(userId);

        MockUserRepository.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task Handle_ShouldCallGetByIdAsync()
    {
        var user = new UserBuilder().Build();
        var query = new GetUserQuery(user.Id);

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        await _handler.Handle(query, CancellationToken.None);

        MockUserRepository.Verify(r => r.GetByIdAsync(
            user.Id,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectUserResponseFields()
    {
        var user = new UserBuilder()
            .WithUsername("johndoe")
            .WithEmail("john@example.com")
            .Build();

        var query = new GetUserQuery(user.Id);

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().Be(user.Id);
        result.Value.Username.Should().Be("johndoe");
        result.Value.Email.Should().Be("john@example.com");
        result.Value.AvatarUrl.Should().Be(user.AvatarUrl);
    }

    [Fact]
    public async Task Handle_WithUserWithAvatarUrl_ShouldIncludeAvatarUrl()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        // Use reflection to set the AvatarUrl since there's no builder method
        var avatarUrlProperty = typeof(User).GetProperty("AvatarUrl");
        avatarUrlProperty?.SetValue(user, "/uploads/avatars/test-avatar.jpg");

        var query = new GetUserQuery(user.Id);

        MockUserRepository.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.AvatarUrl.Should().Be("/uploads/avatars/test-avatar.jpg");
    }

    [Fact]
    public async Task Handle_WithEmptyGuid_ShouldReturnFailure()
    {
        var query = new GetUserQuery(Guid.Empty);

        MockUserRepository.Setup(r => r.GetByIdAsync(Guid.Empty, It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var result = await _handler.Handle(query, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found");
    }
}
