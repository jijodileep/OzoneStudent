using MediatR;
using SchoolSaaS.Application.Abstractions.Audit;
using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Application.Behaviors;

/// <summary>
/// Audits successful write commands (<see cref="ICommand{T}"/>).
/// </summary>
public sealed class AuditBehavior<TRequest, TResponse>(
    IAuditService auditService,
    IEntityChangeCapture entityChangeCapture) : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var isCommand = request.GetType()
            .GetInterfaces()
            .Any(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(ICommand<>));

        var response = await next(cancellationToken);

        if (!isCommand || response.IsFailure)
        {
            return response;
        }

        var changes = entityChangeCapture.GetChanges();
        if (changes.Count > 0)
        {
            foreach (var change in changes)
            {
                await auditService.LogAsync(
                    new AuditEntry(
                        change.Action,
                        change.Category,
                        change.EntityType,
                        change.EntityId,
                        Description: $"{change.EntityType} {change.Action}",
                        BeforeJson: change.BeforeJson,
                        AfterJson: change.AfterJson),
                    cancellationToken);
            }

            entityChangeCapture.Clear();
            return response;
        }

        var requestName = typeof(TRequest).Name;
        await auditService.LogAsync(
            new AuditEntry(
                AuditActions.Create,
                AuditCategories.Platform,
                EntityType: requestName,
                Description: $"{requestName} completed successfully"),
            cancellationToken);

        return response;
    }
}
