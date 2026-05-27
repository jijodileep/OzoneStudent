using MediatR;
using SchoolSaaS.Application.Commands.Rbac.AssignPermissionsToRole;
using SchoolSaaS.Application.Commands.Rbac.AssignRoleToUser;
using SchoolSaaS.Application.Commands.Rbac.CreateRole;
using SchoolSaaS.Application.Queries.Rbac.GetUserPermissions;
using SchoolSaaS.Application.Queries.Rbac.ListRoles;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints.Rbac;

public static class RoleEndpoints
{
    public static RouteGroupBuilder MapRoleEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/roles")
            .WithTags("RBAC")
            .RequireAuthorization();

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            Result<ListRolesResult> result = await mediator.Send(new ListRolesQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
        })
        .WithName("ListRoles")
        .Produces<ListRolesResult>();

        group.MapPost("/", async (CreateRoleRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<CreateRoleResult> result = await mediator.Send(
                new CreateRoleCommand(body.Name, body.Description),
                ct);
            return result.IsSuccess
                ? Results.Created($"/api/v1/roles/{result.Value!.RoleId}", result.Value)
                : Results.BadRequest(result.Errors);
        })
        .WithName("CreateRole")
        .Produces<CreateRoleResult>(StatusCodes.Status201Created);

        group.MapPut("/{roleId:guid}/permissions", async (
            Guid roleId,
            AssignPermissionsRequest body,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<bool> result = await mediator.Send(
                new AssignPermissionsToRoleCommand(roleId, body.Codes),
                ct);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Errors);
        })
        .WithName("AssignPermissionsToRole");

        group.MapPost("/{roleId:guid}/users/{userId:guid}", async (
            Guid roleId,
            Guid userId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<bool> result = await mediator.Send(new AssignRoleToUserCommand(userId, roleId), ct);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Errors);
        })
        .WithName("AssignRoleToUser");

        group.MapGet("/users/{userId:guid}/permissions", async (
            Guid userId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<GetUserPermissionsResult> result = await mediator.Send(
                new GetUserPermissionsQuery(userId),
                ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.ForbiddenCode)
                    ? Results.Json(new { errors = result.Errors }, statusCode: StatusCodes.Status403Forbidden)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("GetUserPermissions")
        .Produces<GetUserPermissionsResult>();

        group.MapGet("/me/permissions", async (IMediator mediator, CancellationToken ct) =>
        {
            Result<GetUserPermissionsResult> result = await mediator.Send(new GetUserPermissionsQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
        })
        .WithName("GetMyPermissions")
        .Produces<GetUserPermissionsResult>();

        return group;
    }

    private sealed record CreateRoleRequest(string Name, string? Description);

    private sealed record AssignPermissionsRequest(IReadOnlyList<string> Codes);
}
