using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Models;
using AutoStack.Application.Features.Stacks.Commands.TrackDownload;
using AutoStack.Application.Tests.Builders;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStack.Application.Tests.Features.Stacks.Commands.TrackDownload;

public class TrackDownloadCommandHandlerTests : CommandHandlerTestBase
{
    private readonly TrackDownloadCommandHandler _handler;

    public TrackDownloadCommandHandlerTests()
    {
        _handler = new TrackDownloadCommandHandler(
            MockStackRepository.Object,
            MockUserRepository.Object,
            MockUnitOfWork.Object,
            MockAuditLog.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidStackId_ShouldReturnSuccessWithStackResponse()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        var stack = new StackBuilder()
            .WithName("Test Stack")
            .WithType("FRONTEND")
            .WithUser(user)
            .Build();

        var command = new TrackDownloadCommand(stack.Id);

        MockStackRepository.Setup(r => r.GetByIdAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(stack.Id);
        result.Value.Name.Should().Be(stack.Name);
        result.Value.Username.Should().Be(user.Username);
    }

    [Fact]
    public async Task Handle_WithNonExistentStackId_ShouldReturnFailure()
    {
        var stackId = Guid.NewGuid();
        var command = new TrackDownloadCommand(stackId);

        MockStackRepository.Setup(r => r.GetByIdAsync(
                stackId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stack?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Stack not found");

        MockUnitOfWork.Verify(u => u.SaveChangesAsync(
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldIncrementDownloadCount()
    {
        var user = new UserBuilder().Build();
        var stack = new StackBuilder()
            .WithUser(user)
            .Build();

        var initialDownloads = stack.Downloads;
        var command = new TrackDownloadCommand(stack.Id);

        MockStackRepository.Setup(r => r.GetByIdAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        stack.Downloads.Should().Be(initialDownloads + 1);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var user = new UserBuilder().Build();
        var stack = new StackBuilder()
            .WithUser(user)
            .Build();

        var command = new TrackDownloadCommand(stack.Id);

        MockStackRepository.Setup(r => r.GetByIdAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGetStackWithInfo()
    {
        var user = new UserBuilder().Build();
        var stack = new StackBuilder()
            .WithUser(user)
            .Build();

        var command = new TrackDownloadCommand(stack.Id);

        MockStackRepository.Setup(r => r.GetByIdAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockStackRepository.Verify(r => r.GetByIdWithInfoAsync(
            stack.Id,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenStackNotFoundAfterIncrement_ShouldReturnFailure()
    {
        var user = new UserBuilder().Build();
        var stack = new StackBuilder()
            .WithUser(user)
            .Build();

        var command = new TrackDownloadCommand(stack.Id);

        MockStackRepository.Setup(r => r.GetByIdAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stack?)null);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Stack not found");
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldReturnFailure()
    {
        var userId = Guid.NewGuid();
        var stack = new StackBuilder()
            .WithUserId(userId)
            .Build();

        var command = new TrackDownloadCommand(stack.Id);

        MockStackRepository.Setup(r => r.GetByIdAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("User not found");
    }

    [Fact]
    public async Task Handle_ShouldCallAuditLogService()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .Build();

        var stack = new StackBuilder()
            .WithName("My Stack")
            .WithUser(user)
            .Build();

        var command = new TrackDownloadCommand(stack.Id);

        MockStackRepository.Setup(r => r.GetByIdAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockAuditLog.Verify(a => a.LogAsync(
            It.IsAny<AuditLogRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldReturnCorrectStackResponse()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .WithEmail("test@example.com")
            .Build();

        var stack = new StackBuilder()
            .WithName("My Stack")
            .WithDescription("Test Description")
            .WithType("BACKEND")
            .WithUser(user)
            .Build();

        var command = new TrackDownloadCommand(stack.Id);

        MockStackRepository.Setup(r => r.GetByIdAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Id.Should().Be(stack.Id);
        result.Value.Name.Should().Be("My Stack");
        result.Value.Description.Should().Be("Test Description");
        result.Value.Type.ToString().Should().Be("BACKEND");
        result.Value.UserId.Should().Be(user.Id);
        result.Value.Username.Should().Be("testuser");
    }
}
