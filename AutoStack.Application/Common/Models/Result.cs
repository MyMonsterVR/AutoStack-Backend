namespace AutoStack.Application.Common.Models;

/// <summary>
/// Represents the result of an operation that can succeed or fail
/// </summary>
public class Result
{
    /// <summary>
    /// Gets whether the operation was successful
    /// </summary>
    public bool IsSuccess { get; }

    /// <summary>
    /// Gets whether the operation failed
    /// </summary>
    protected bool IsError => !IsSuccess;

    /// <summary>
    /// Gets the error message or success message
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Gets the validation errors if the operation failed due to validation
    /// </summary>
    public Dictionary<string, string[]>? ValidationErrors { get; }

    protected Result(bool isSuccess, string? error, Dictionary<string, string[]>? validationErrors = null)
    {
        if (isSuccess && !string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("A successful result can not have an error");
        }

        if (!isSuccess && string.IsNullOrWhiteSpace(error))
        {
            throw new ArgumentException("A failed result must have an error");
        }

        IsSuccess = isSuccess;
        Message = error ?? string.Empty;
        ValidationErrors = validationErrors;
    }

    /// <summary>
    /// Creates a successful result
    /// </summary>
    /// <returns>A successful Result instance</returns>
    public static Result Success() => new(true, string.Empty);

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    /// <param name="errorMsg">The error message</param>
    /// <returns>A failed Result instance</returns>
    public static Result Failure(string errorMsg) => new(false, errorMsg);

    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    /// <param name="validationErrors">The validation errors dictionary</param>
    /// <returns>A failed Result instance with validation errors</returns>
    public static Result Failure(Dictionary<string, string[]> validationErrors)
        => new(false, "Validation failed", validationErrors);
}