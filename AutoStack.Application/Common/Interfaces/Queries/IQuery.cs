using AutoStack.Application.Common.Models;
using MediatR;

namespace AutoStack.Application.Common.Interfaces.Queries;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{}