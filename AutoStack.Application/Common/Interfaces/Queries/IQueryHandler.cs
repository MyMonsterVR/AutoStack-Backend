using AutoStack.Application.Common.Models;
using MediatR;

namespace AutoStack.Application.Common.Interfaces.Queries;

/// <summary>
/// Interface for query handlers in the CQRS pattern
/// </summary>
/// <typeparam name="TQuery">The type of query to handle</typeparam>
/// <typeparam name="TResponse">The type of response returned by the handler</typeparam>
public interface IQueryHandler<TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IRequest<Result<TResponse>>
{}