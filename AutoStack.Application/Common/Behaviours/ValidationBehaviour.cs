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
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

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
        // If no validators are registered for this request type, skip validation
        if (!_validators.Any())
        {
            return await next(cancellationToken);
        }
        
        var context = new ValidationContext<TRequest>(request);

        // Run ALL validators for this request type in parallel
        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        // Collect all validation failures from all validators
        var failures = validationResults
            .Where(r => !r.IsValid)              // Only failed validations
            .SelectMany(r => r.Errors)           // Get all error messages
            .ToList();

        // If there are any validation failures, return error result
        if (failures.Count == 0) return await next(cancellationToken);
        
        // Group errors by property name for structured response
        // Example: { "Email": ["Email is required", "Invalid format"], "Password": ["Too short"] }
        var errors = failures
            .GroupBy(e => e.PropertyName)
            .ToDictionary(
                g => g.Key,
                g => g.Select(e => e.ErrorMessage).ToArray()
            );

        var resultType = typeof(TResponse);
        var failureMethod = resultType.GetMethod(
            nameof(Result.Failure),
            [typeof(Dictionary<string, string[]>)]
        );

        if (failureMethod != null)
        {
            return (TResponse)failureMethod.Invoke(null, [errors])!;
        }

        return await next(cancellationToken);
    }
}