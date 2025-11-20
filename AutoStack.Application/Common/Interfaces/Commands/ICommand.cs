using AutoStack.Application.Common.Models;
using MediatR;

namespace AutoStack.Application.Common.Interfaces.Commands;

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{}