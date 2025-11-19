namespace AutoStack.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public bool IsError => !IsSuccess;

    public string Message { get; }

    protected Result(bool isSuccess, string? error)
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
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string errorMsg) => new(false, errorMsg);
}