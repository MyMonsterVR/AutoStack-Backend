using AutoStack.Application.Common.Interfaces;
using AutoStack.Application.Common.Models;
using AutoStack.Application.Features.Stacks.Commands.DeleteStack;
using AutoStack.Application.Tests.Builders;
using AutoStack.Application.Tests.Common;
using AutoStack.Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;

namespace AutoStack.Application.Tests.Features.Stacks.Commands.DeleteStack;

public class DeleteStackCommandHandlerTests : CommandHandlerTestBase
{
    private readonly DeleteStackCommandHandler _handler;

    public DeleteStackCommandHandlerTests()
    {
        _handler = new DeleteStackCommandHandler(
            MockStackRepository.Object,
            MockUnitOfWork.Object,
            MockAuditLog.Object,
            MockUserRepository.Object
        );
    }

    [Fact]
    public async Task Handle_WithValidStackId_ShouldReturnSuccess()
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

        var command = new DeleteStackCommand(stack.Id, user.Id);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockStackRepository.Setup(r => r.DeleteAsync(
                It.IsAny<Stack>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithNonExistentStackId_ShouldReturnFailure()
    {
        var stackId = Guid.NewGuid();
        var command = new DeleteStackCommand(stackId, Guid.NewGuid());

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stackId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Stack?)null);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.Message.Should().Be("Stack not found");

        MockStackRepository.Verify(r => r.DeleteAsync(
            It.IsAny<Stack>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ShouldDeleteStackFromRepository()
    {
        var user = new UserBuilder().Build();
        var stack = new StackBuilder()
            .WithUser(user)
            .Build();

        var command = new DeleteStackCommand(stack.Id, user.Id);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockStackRepository.Setup(r => r.DeleteAsync(
                It.IsAny<Stack>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockStackRepository.Verify(r => r.DeleteAsync(
            It.Is<Stack>(s => s.Id == stack.Id),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldSaveChanges()
    {
        var user = new UserBuilder().Build();
        var stack = new StackBuilder()
            .WithUser(user)
            .Build();

        var command = new DeleteStackCommand(stack.Id, user.Id);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockStackRepository.Setup(r => r.DeleteAsync(
                It.IsAny<Stack>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

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

        var command = new DeleteStackCommand(stack.Id, user.Id);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockStackRepository.Setup(r => r.DeleteAsync(
                It.IsAny<Stack>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockStackRepository.Verify(r => r.GetByIdWithInfoAsync(
            stack.Id,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldGetUserForAuditLog()
    {
        var user = new UserBuilder().Build();
        var stack = new StackBuilder()
            .WithUser(user)
            .Build();

        var command = new DeleteStackCommand(stack.Id, user.Id);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockStackRepository.Setup(r => r.DeleteAsync(
                It.IsAny<Stack>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockUserRepository.Verify(r => r.GetByIdAsync(
            user.Id,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WhenUserNotFound_ShouldStillDeleteStack()
    {
        var userId = Guid.NewGuid();
        var stack = new StackBuilder()
            .WithUserId(userId)
            .Build();

        var command = new DeleteStackCommand(stack.Id, userId);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                userId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        MockStackRepository.Setup(r => r.DeleteAsync(
                It.IsAny<Stack>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        MockStackRepository.Verify(r => r.DeleteAsync(
            It.IsAny<Stack>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_ShouldCallAuditLogService()
    {
        var user = new UserBuilder()
            .WithUsername("testuser")
            .Build();

        var stack = new StackBuilder()
            .WithName("My Stack")
            .WithType("BACKEND")
            .WithUser(user)
            .Build();

        var command = new DeleteStackCommand(stack.Id, user.Id);

        MockStackRepository.Setup(r => r.GetByIdWithInfoAsync(
                stack.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(stack);

        MockUserRepository.Setup(r => r.GetByIdAsync(
                user.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        MockStackRepository.Setup(r => r.DeleteAsync(
                It.IsAny<Stack>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        MockUnitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        await _handler.Handle(command, CancellationToken.None);

        MockAuditLog.Verify(a => a.LogAsync(
            It.IsAny<AuditLogRequest>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
