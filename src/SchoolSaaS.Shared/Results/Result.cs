namespace SchoolSaaS.Shared.Results;

public class Result
{
    public static readonly Error[] EmptyErrors = [];

    protected Result(bool isSuccess, Error[] errors)
    {
        IsSuccess = isSuccess;
        Errors = errors;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error[] Errors { get; }

    public static Result Success() => new(true, EmptyErrors);

    public static Result Failure(params Error[] errors) =>
        new(false, errors.Length > 0 ? errors : [ErrorFactory.Unknown()]);

    public static Result Failure(string code, string message) =>
        Failure(new Error(code, message));

    public static Result NotFound(string message = "Resource not found.") =>
        Failure(ErrorFactory.NotFound(message));

    public static Result Forbidden(string message = "Access denied.") =>
        Failure(ErrorFactory.Forbidden(message));

    public static Result Conflict(string message = "Resource conflict.") =>
        Failure(ErrorFactory.Conflict(message));
}

public static class ErrorFactory
{
    public const string NotFoundCode = "not_found";
    public const string ForbiddenCode = "forbidden";
    public const string ConflictCode = "conflict";
    public const string UnknownCode = "unknown";

    public static Error NotFound(string message) => new(NotFoundCode, message);

    public static Error Forbidden(string message) => new(ForbiddenCode, message);

    public static Error Conflict(string message) => new(ConflictCode, message);

    public static Error Unknown() => new(UnknownCode, "An unexpected error occurred.");
}
