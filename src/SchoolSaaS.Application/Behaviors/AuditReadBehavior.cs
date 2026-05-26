using System.Reflection;
using MediatR;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Behaviors;

public sealed class AuditReadBehavior<TRequest, TResponse>(
    IAuditService auditService) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var response = await next(cancellationToken);

        var auditRead = request.GetType().GetCustomAttribute<AuditReadAttribute>(inherit: true);
        if (auditRead is null || response.IsFailure)
        {
            return response;
        }

        await auditService.LogAsync(
            new AuditEntry(
                AuditActions.Read,
                auditRead.Category,
                EntityType: auditRead.EntityType ?? typeof(TRequest).Name,
                Description: $"{typeof(TRequest).Name} executed"),
            cancellationToken);

        return response;
    }
}
