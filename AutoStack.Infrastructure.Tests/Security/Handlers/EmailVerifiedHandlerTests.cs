using System.Security.Claims;
using AutoStack.Domain.Entities;
using AutoStack.Domain.Repositories;
using AutoStack.Infrastructure.Security.Handlers;
using AutoStack.Infrastructure.Security.Requirements;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Moq;
using Xunit;

namespace AutoStack.Infrastructure.Tests.Security.Handlers;

public class EmailVerifiedHandlerTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly EmailVerifiedHandler _handler;

    public EmailVerifiedHandlerTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _handler = new EmailVerifiedHandler(_mockUserRepository.Object);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithVerifiedUser_ShouldSucceed()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");
        user.SetEmailVerificationCode("123456", DateTime.UtcNow.AddMinutes(15));
        user.VerifyEmail();

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync(user);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var requirement = new EmailVerifiedRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            claimsPrincipal,
            null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithUnverifiedUser_ShouldFail()
    {
        var userId = Guid.NewGuid();
        var user = User.CreateUser("test@example.com", "testuser");

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync(user);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var requirement = new EmailVerifiedRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            claimsPrincipal,
            null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNonExistentUser_ShouldFail()
    {
        var userId = Guid.NewGuid();

        _mockUserRepository.Setup(r => r.GetByIdAsync(userId, default))
            .ReturnsAsync((User?)null);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var requirement = new EmailVerifiedRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            claimsPrincipal,
            null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithNoUserIdClaim_ShouldFail()
    {
        var claims = Array.Empty<Claim>();
        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var requirement = new EmailVerifiedRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            claimsPrincipal,
            null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }

    [Fact]
    public async Task HandleRequirementAsync_WithInvalidUserIdClaim_ShouldFail()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "not-a-guid")
        };

        var identity = new ClaimsIdentity(claims);
        var claimsPrincipal = new ClaimsPrincipal(identity);

        var requirement = new EmailVerifiedRequirement();
        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            claimsPrincipal,
            null);

        await _handler.HandleAsync(context);

        context.HasSucceeded.Should().BeFalse();
        context.HasFailed.Should().BeTrue();
    }
}
