using System.Collections.Concurrent;
using System.Linq.Expressions;
using AutoStack.Application.Common.Models;
using FluentValidation;
using MediatR;

namespace AutoStack.Application.Common.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that automatically validates requests using FluentValidation before they reach the handler.
/// </summary>
/// <typeparam name="TRequest">The request type (Command or Query)</typeparam>
/// <typeparam name="TResponse">The response type (must be a Result)</typeparam>
public class ValidationBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    private static readonly ConcurrentDictionary<Type, Func<Dictionary<string, string[]>, object>> FailureFactoryCache = new();

    /// <summary>
    /// Constructor - MediatR DI automatically injects all validators for TRequest.
    /// If no validators exist for TRequest, this will be an empty collection.
    /// </summary>
    public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    /// <summary>
    /// This method is called by MediatR pipeline automatically for EVERY request.
    /// </summary>
    /// <param name="request">The command/query being sent</param>
    /// <param name="next">Delegate to call the next behaviour or handler in the pipeline</param>
    /// <param name="cancellationToken">Cancellation token</param>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .Where(r => !r.IsValid)
            .SelectMany(r => r.Errors)
            .ToList();

        if (failures.Count == 0)
            return await next(cancellationToken);

        var errors = failures
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        return CreateValidationFailureResult<TResponse>(errors);
    }

    private static TResult CreateValidationFailureResult<TResult>(Dictionary<string, string[]> errors)
    {
        var resultType = typeof(TResult);

        var factory = FailureFactoryCache.GetOrAdd(resultType, type =>
        {
            if (type == typeof(Result))
            {
                return errorsDict => Result.Failure(errorsDict);
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(Result<>))
            {
                var failureMethod = type.GetMethod(nameof(Result.Failure), new[] { typeof(Dictionary<string, string[]>) });

                if (failureMethod == null)
                {
                    throw new InvalidOperationException($"Unable to find Failure method for type {type.Name}");
                }

                var errorsParam = Expression.Parameter(typeof(Dictionary<string, string[]>), "errors");
                var callExpression = Expression.Call(failureMethod, errorsParam);
                var lambda = Expression.Lambda<Func<Dictionary<string, string[]>, object>>(
                    Expression.Convert(callExpression, typeof(object)),
                    errorsParam
                );

                return lambda.Compile();
            }

            throw new InvalidOperationException($"Unable to create validation failure for type {type.Name}");
        });

        return (TResult)factory(errors);
    }
}