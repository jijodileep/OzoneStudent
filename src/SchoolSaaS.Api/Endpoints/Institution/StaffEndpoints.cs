using MediatR;
using SchoolSaaS.Application.Commands.Institution.CreateStaff;
using SchoolSaaS.Application.Commands.Institution.LinkStaffToUser;
using SchoolSaaS.Application.Commands.Institution.UpdateStaff;
using SchoolSaaS.Application.Queries.Institution.GetStaffById;
using SchoolSaaS.Application.Queries.Institution.ListStaff;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints.Institution;

public static class StaffEndpoints
{
    public static RouteGroupBuilder MapStaffEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/staff")
            .WithTags("Institution")
            .RequireAuthorization();

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            Result<ListStaffResult> result = await mediator.Send(new ListStaffQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
        })
        .WithName("ListStaff")
        .Produces<ListStaffResult>();

        group.MapGet("/{staffId:guid}", async (Guid staffId, IMediator mediator, CancellationToken ct) =>
        {
            Result<GetStaffByIdResult> result = await mediator.Send(new GetStaffByIdQuery(staffId), ct);
            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("GetStaffById")
        .Produces<GetStaffByIdResult>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/", async (CreateStaffRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<CreateStaffResult> result = await mediator.Send(
                new CreateStaffCommand(
                    body.EmployeeCode,
                    body.Designation,
                    body.JoinDate,
                    body.UserId,
                    body.CustomFields),
                ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/staff/{result.Value!.Id}", result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : result.Errors.Any(e => e.Code == ErrorFactory.ConflictCode)
                        ? Results.Conflict(result.Errors)
                        : Results.BadRequest(result.Errors);
        })
        .WithName("CreateStaff")
        .Produces<CreateStaffResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{staffId:guid}", async (
            Guid staffId,
            UpdateStaffRequest body,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<UpdateStaffResult> result = await mediator.Send(
                new UpdateStaffCommand(
                    staffId,
                    body.Designation,
                    body.Status,
                    body.JoinDate,
                    body.CustomFields),
                ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("UpdateStaff")
        .Produces<UpdateStaffResult>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPost("/{staffId:guid}/link-user", async (
            Guid staffId,
            LinkStaffToUserRequest body,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<LinkStaffToUserResult> result = await mediator.Send(
                new LinkStaffToUserCommand(staffId, body.UserId),
                ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : result.Errors.Any(e => e.Code == ErrorFactory.ConflictCode)
                        ? Results.Conflict(result.Errors)
                        : Results.BadRequest(result.Errors);
        })
        .WithName("LinkStaffToUser")
        .Produces<LinkStaffToUserResult>()
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private sealed record CreateStaffRequest(
        string EmployeeCode,
        string Designation,
        DateOnly JoinDate,
        Guid? UserId = null,
        IReadOnlyDictionary<string, string?>? CustomFields = null);

    private sealed record UpdateStaffRequest(
        string Designation,
        StaffStatus Status,
        DateOnly JoinDate,
        IReadOnlyDictionary<string, string?>? CustomFields = null);

    private sealed record LinkStaffToUserRequest(Guid UserId);
}
