namespace MyProject.Domain;

public class Result<T>
{
    public bool IsSuccess { get; }
    public string? Error { get; }
    public T? Value { get; }

    private Result(bool isSuccess, string? error, T? value)
    {
        IsSuccess = isSuccess;
        Error = error;
        Value = value;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(true, null, value);
    }

    public static Result<T> Failure(string? error)
    {
        return new Result<T>(false, error, default);
    }
}

public class Result
{
    public bool IsSuccess { get; }
    public string? Error { get; }

    private Result(bool isSuccess, string? error = null)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success()
    {
        return new Result(true);
    }

    public static Result Failure(string error)
    {
        return new Result(false, error);
    }
}