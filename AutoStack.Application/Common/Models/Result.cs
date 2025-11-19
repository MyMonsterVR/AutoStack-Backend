namespace AutoStack.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    protected bool IsError => !IsSuccess;

    public string Message { get; }
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

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string errorMsg) => new(false, errorMsg);
    public static Result Failure(Dictionary<string, string[]> validationErrors) 
        => new(false, "Validation failed", validationErrors);
}