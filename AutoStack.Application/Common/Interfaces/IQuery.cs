using AutoStack.Application.Common.Models;
using MediatR;

namespace AutoStack.Application.Common.Interfaces;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{}