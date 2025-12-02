using AutoStack.Application.Common.Interfaces.Commands;

namespace AutoStack.Application.Features.Stacks.Commands.DeleteStack;

public record DeleteStackCommand(Guid StackId, Guid? UserId) : ICommand<bool>;