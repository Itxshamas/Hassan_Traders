namespace HassanTraders.Desktop.Core.Common;

/// <summary>
/// Outcome of a use case. <see cref="Error"/> is a stable code (for example <c>Login_Invalid</c>)
/// that the UI maps to a localized message, so no user text lives below the Desktop project.
/// </summary>
public class Result
{
    protected Result(bool isSuccess, string? error)
    {
        if (isSuccess == !string.IsNullOrEmpty(error))
        {
            throw new DomainException("A result is either a success without error, or a failure with an error code.");
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }

    public static Result Success() => new(true, null);

    public static Result Failure(string error) => new(false, error);

    public static Result<T> Success<T>(T value) => new(value, true, null);

    public static Result<T> Failure<T>(string error) => new(default, false, error);
}

public sealed class Result<T> : Result
{
    private readonly T? _value;

    internal Result(T? value, bool isSuccess, string? error)
        : base(isSuccess, error)
    {
        _value = value;
    }

    public T Value => IsSuccess
        ? _value!
        : throw new DomainException("A failed result has no value.");
}
