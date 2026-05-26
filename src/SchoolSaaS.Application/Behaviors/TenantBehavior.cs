using MediatR;
using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Behaviors;

public sealed class TenantBehavior<TRequest, TResponse>(ITenantContext tenantContext)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is ITenantScopedRequest && tenantContext.TenantId is null && !tenantContext.IsSuperAdmin)
        {
            return CreateForbidden();
        }

        return await next(cancellationToken);
    }

    private static TResponse CreateForbidden()
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)Result.Forbidden();
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var forbiddenMethod = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethod(nameof(Result<object>.Forbidden), [typeof(string)])!;

            return (TResponse)forbiddenMethod.Invoke(null, ["Tenant context is required."])!;
        }

        throw new InvalidOperationException(
            $"TenantBehavior requires TResponse to be Result or Result<T>, but was {responseType.Name}.");
    }
}
