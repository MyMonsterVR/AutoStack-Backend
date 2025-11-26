using AutoStack.Application.Common.Models;
using MediatR;

namespace AutoStack.Application.Common.Interfaces.Commands;

/// <summary>
/// Marker interface for commands in the CQRS pattern
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the command</typeparam>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{}