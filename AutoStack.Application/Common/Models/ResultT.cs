namespace AutoStack.Application.Common.Models;

public class Result<T> : Result
{
    private readonly T _value;
    
    public T Value
    {
        get
        {
            if (IsError)
                throw new InvalidOperationException("Cannot access the value of an error.");
            
            return _value;
        }
    }
    
    protected Result(bool isSuccess, T? value, string? error) : base(isSuccess, error)
    {
        _value = value;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public new static Result<T> Failure(string error) => new(false, default, error);
}