using AutoStack.Application.Common.Models;
using MediatR;

namespace AutoStack.Application.Common.Interfaces.Queries;

/// <summary>
/// Marker interface for queries in the CQRS pattern
/// </summary>
/// <typeparam name="TResponse">The type of response returned by the query</typeparam>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{}