using FluentValidation;
using MediatR;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Behaviors;

public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next(cancellationToken);
        }

        var context = new ValidationContext<TRequest>(request);
        var validationResults = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next(cancellationToken);
        }

        var errors = failures
            .Select(f => Error.Validation(
                string.IsNullOrWhiteSpace(f.ErrorCode) ? "validation.failed" : f.ErrorCode,
                f.ErrorMessage,
                f.PropertyName))
            .ToArray();

        return CreateFailure(errors);
    }

    private static TResponse CreateFailure(Error[] errors)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Failure(errors);
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethod(nameof(Result<object>.Failure), [typeof(Error[])])!;

            return (TResponse)failureMethod.Invoke(null, [errors])!;
        }

        throw new InvalidOperationException(
            $"ValidationBehavior requires TResponse to be Result or Result<T>, but was {responseType.Name}.");
    }
}
