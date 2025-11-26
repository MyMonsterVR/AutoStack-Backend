namespace AutoStack.Application.Common.Models;

/// <summary>
/// Represents the result of an operation that can succeed or fail and returns a value on success
/// </summary>
/// <typeparam name="T">The type of value returned on success</typeparam>
public class Result<T> : Result
{
    private readonly T? _value;

    /// <summary>
    /// Gets the value of a successful result
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when attempting to access the value of a failed result</exception>
    public T Value
    {
        get
        {
            if (IsError)
                throw new InvalidOperationException("Cannot access the value of an error.");

            return _value!;
        }
    }

    private Result(bool isSuccess, T? value, string? error, Dictionary<string, string[]>? validationErrors = null)
        : base(isSuccess, error, validationErrors)
    {
        _value = value;
    }

    /// <summary>
    /// Creates a successful result with a value
    /// </summary>
    /// <param name="value">The value to return</param>
    /// <returns>A successful Result instance containing the value</returns>
    public static Result<T> Success(T value) => new(true, value, string.Empty);

    /// <summary>
    /// Creates a failed result with an error message
    /// </summary>
    /// <param name="error">The error message</param>
    /// <returns>A failed Result instance</returns>
    public new static Result<T> Failure(string error) => new(false, default, error);

    /// <summary>
    /// Creates a failed result with validation errors
    /// </summary>
    /// <param name="validationErrors">The validation errors dictionary</param>
    /// <returns>A failed Result instance with validation errors</returns>
    public new static Result<T> Failure(Dictionary<string, string[]> validationErrors)
        => new(false, default, "Validation failed", validationErrors);
}