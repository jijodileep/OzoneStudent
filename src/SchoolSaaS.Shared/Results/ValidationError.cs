namespace SchoolSaaS.Shared.Results;

public sealed record ValidationError(string Code, string Message, string PropertyName)
    : Error(Code, Message, PropertyName);
