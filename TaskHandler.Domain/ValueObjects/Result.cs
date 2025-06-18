namespace TaskHandler.Domain.ValueObjects;

public class Result
{
    public bool IsSuccess { get; set; }
    public string Error { get; }
    public bool HasError => !IsSuccess;

    protected Result(bool success, string error)
    {
        IsSuccess = success;
        Error = error;
    }
    
    public static Result Success() => new Result(true, string.Empty);
    public static Result Failure(string error) => new Result(false, error);
    public static Result<T> Success<T>(T value) => new Result<T>(value, true, string.Empty);
    public static Result<T?> Failure<T>(string error) => new Result<T?>(default, false, error);
}

public class Result<T> : Result
{
    private readonly T _value;
    public T Value => IsSuccess ? 
        _value : 
        throw new InvalidOperationException("Result is not success");
    
    protected internal Result(T value, bool success, string error) 
        : base(success, error)
    {
        _value = value;
    }
}
