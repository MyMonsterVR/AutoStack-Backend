using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace AutoStack.Application.Common.Behaviours;

/// <summary>
/// Pipeline behavior that logs request and response information for all requests.
/// Logs the request name, timestamp, duration, and success/failure status.
/// </summary>
/// <typeparam name="TRequest">The type of request.</typeparam>
/// <typeparam name="TResponse">The type of response.</typeparam>
public class LoggingBehaviour<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="LoggingBehaviour{TRequest,TResponse}"/> class.
    /// </summary>
    /// <param name="logger">The logger instance.</param>
    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Logs request and response information. 
    /// </summary>
    /// <param name="request">The request being processed.</param>
    /// <param name="next">The next behavior or handler in the pipeline.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The response from the handler.</returns>
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var timestamp = DateTime.UtcNow;

        _logger.LogInformation(
            "Handling {RequestName} at {Timestamp}",
            requestName,
            timestamp);

        var stopwatch = Stopwatch.StartNew();

        try
        {
            var response = await next(cancellationToken);

            stopwatch.Stop();

            _logger.LogInformation(
                "Completed {RequestName} in {Duration}ms with success",
                requestName,
                stopwatch.ElapsedMilliseconds);

            return response;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(
                ex,
                "Completed {RequestName} in {Duration}ms with failure: {ErrorMessage}",
                requestName,
                stopwatch.ElapsedMilliseconds,
                ex.Message);

            throw;
        }
    }
}