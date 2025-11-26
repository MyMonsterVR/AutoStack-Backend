using AutoStack.Application.Common.Models;
using MediatR;

namespace AutoStack.Application.Common.Interfaces.Commands;

/// <summary>
/// Interface for command handlers in the CQRS pattern
/// </summary>
/// <typeparam name="TCommand">The type of command to handle</typeparam>
/// <typeparam name="TResponse">The type of response returned by the handler</typeparam>
public interface ICommandHandler<TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>
{
}