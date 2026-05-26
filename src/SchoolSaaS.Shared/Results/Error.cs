namespace SchoolSaaS.Shared.Results;

public record Error(string Code, string Message, string? Field = null)
{
    public static Error Validation(string code, string message, string propertyName) =>
        new ValidationError(code, message, propertyName);
}
