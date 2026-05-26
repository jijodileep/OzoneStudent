namespace SchoolSaaS.Shared.Results;

public sealed class Result<T> : Result
{
    private Result(T value) : base(true, EmptyErrors) => Value = value;

    private Result(Error[] errors) : base(false, errors) => Value = default;

    public T? Value { get; }

    public static Result<T> Success(T value) => new(value);

    public new static Result<T> Failure(params Error[] errors) =>
        new(errors.Length > 0 ? errors : [ErrorFactory.Unknown()]);

    public new static Result<T> Failure(string code, string message) =>
        Failure(new Error(code, message));

    public new static Result<T> NotFound(string message = "Resource not found.") =>
        Failure(ErrorFactory.NotFound(message));

    public new static Result<T> Forbidden(string message = "Access denied.") =>
        Failure(ErrorFactory.Forbidden(message));

    public new static Result<T> Conflict(string message = "Resource conflict.") =>
        Failure(ErrorFactory.Conflict(message));

    public static implicit operator Result<T>(T value) => Success(value);
}
