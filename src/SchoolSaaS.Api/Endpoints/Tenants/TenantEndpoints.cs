using MediatR;
using SchoolSaaS.Application.Commands.Tenants.CreateTenant;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints.Tenants;

public static class TenantEndpoints
{
    public static RouteGroupBuilder MapTenantEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/tenants")
            .WithTags("Tenants")
            .RequireAuthorization();

        group.MapPost("/", async (CreateTenantRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<CreateTenantResult> result = await mediator.Send(
                new CreateTenantCommand(
                    body.Name,
                    body.Slug,
                    body.DbServer,
                    body.DbPort,
                    body.DbName,
                    body.DbUser,
                    body.DbPassword,
                    body.Plan ?? "free",
                    body.AdminEmail,
                    body.AdminPassword,
                    body.AdminFirstName,
                    body.AdminLastName),
                ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/tenants/{result.Value!.TenantId}", result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.ConflictCode)
                    ? Results.Conflict(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("CreateTenant")
        .Produces<CreateTenantResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private sealed record CreateTenantRequest(
        string Name,
        string Slug,
        string? DbServer = null,
        int? DbPort = null,
        string? DbName = null,
        string? DbUser = null,
        string? DbPassword = null,
        string? Plan = null,
        string? AdminEmail = null,
        string? AdminPassword = null,
        string? AdminFirstName = null,
        string? AdminLastName = null);
}
