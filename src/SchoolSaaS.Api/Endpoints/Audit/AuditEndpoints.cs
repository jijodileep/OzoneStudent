using MediatR;
using SchoolSaaS.Application.Queries.Audit.ListAuditLogs;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints.Audit;

public static class AuditEndpoints
{
    public static RouteGroupBuilder MapAuditEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/audit-logs")
            .WithTags("Audit")
            .RequireAuthorization();

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            string? action,
            string? category,
            DateTime? fromUtc,
            DateTime? toUtc,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<ListAuditLogsResult> result = await mediator.Send(
                new ListAuditLogsQuery(
                    page ?? 1,
                    pageSize ?? 50,
                    action,
                    category,
                    fromUtc,
                    toUtc),
                ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
        })
        .WithName("ListAuditLogs")
        .Produces<ListAuditLogsResult>();

        return group;
    }
}
