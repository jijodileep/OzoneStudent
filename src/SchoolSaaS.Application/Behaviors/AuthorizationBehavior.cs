using System.Reflection;
using MediatR;
using Microsoft.Extensions.Logging;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Abstractions.Rbac;
using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;
using SchoolSaaS.Shared.MultiTenancy;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Behaviors;

public sealed class AuthorizationBehavior<TRequest, TResponse>(
    ITenantContext tenantContext,
    IPermissionResolver permissionResolver,
    IAuditService auditService,
    ILogger<AuthorizationBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var permissions = request.GetType()
            .GetCustomAttributes<RequirePermissionAttribute>(inherit: true)
            .Select(a => a.Permission)
            .ToList();

        if (permissions.Count == 0)
        {
            return await next(cancellationToken);
        }

        if (tenantContext.UserId is null)
        {
            return CreateFailure(Result.Forbidden("Authentication is required."));
        }

        if (tenantContext.IsSuperAdmin)
        {
            return await next(cancellationToken);
        }

        var isPlatformCommand = request is IPlatformCommand;
        if (isPlatformCommand && tenantContext.TenantId is null)
        {
            return CreateFailure(Result.Forbidden("Tenant context is required for this operation."));
        }

        if (!isPlatformCommand && tenantContext.TenantId is null)
        {
            return CreateFailure(Result.Forbidden("Authentication is required."));
        }

        foreach (var permission in permissions)
        {
            var allowed = await permissionResolver.HasPermissionAsync(
                tenantContext.UserId.Value,
                tenantContext.TenantId!.Value,
                permission,
                cancellationToken);

            if (!allowed)
            {
                logger.LogWarning(
                    "Access denied for user {UserId} — missing permission {Permission}",
                    tenantContext.UserId,
                    permission);

                await auditService.LogAsync(
                    new AuditEntry(
                        AuditActions.AccessDenied,
                        AuditCategories.Rbac,
                        Description: $"Missing permission {permission} for {typeof(TRequest).Name}",
                        Outcome: AuditOutcomes.Denied),
                    cancellationToken);

                return CreateFailure(Result.Forbidden($"Missing permission: {permission}"));
            }
        }

        return await next(cancellationToken);
    }

    private static TResponse CreateFailure(Result result)
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)(object)result;
        }

        if (responseType.IsGenericType && responseType.GetGenericTypeDefinition() == typeof(Result<>))
        {
            var valueType = responseType.GetGenericArguments()[0];
            var failureMethod = typeof(Result<>)
                .MakeGenericType(valueType)
                .GetMethod(nameof(Result<object>.Failure), [typeof(Error[])])!;

            return (TResponse)failureMethod.Invoke(null, [result.Errors])!;
        }

        throw new InvalidOperationException(
            $"AuthorizationBehavior requires TResponse to be Result or Result<T>, but was {responseType.Name}.");
    }
}
