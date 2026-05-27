using MediatR;
using SchoolSaaS.Application.Commands.Institution.CreateAcademicYear;
using SchoolSaaS.Application.Commands.Institution.SetCurrentAcademicYear;
using SchoolSaaS.Application.Queries.Institution.ListAcademicYears;
using SchoolSaaS.Shared.Errors;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints.Institution;

public static class AcademicYearEndpoints
{
    public static RouteGroupBuilder MapAcademicYearEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/academic-years")
            .WithTags("Institution")
            .RequireAuthorization();

        group.MapGet("/", async (
            int? page,
            int? pageSize,
            string? search,
            string? sortBy,
            string? sortDirection,
            string? status,
            bool? isCurrent,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<ListAcademicYearsResult> result = await mediator.Send(
                new ListAcademicYearsQuery(
                    page ?? 1,
                    pageSize ?? 10,
                    search,
                    sortBy,
                    sortDirection ?? "asc",
                    status,
                    isCurrent),
                ct);

            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
        })
        .WithName("ListAcademicYears")
        .Produces<ListAcademicYearsResult>();

        group.MapPost("/", async (CreateAcademicYearRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<CreateAcademicYearResult> result = await mediator.Send(
                new CreateAcademicYearCommand(
                    body.Name,
                    body.StartDate,
                    body.EndDate,
                    body.SetAsCurrent ?? false),
                ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/academic-years/{result.Value!.Id}", result.Value)
                : result.Errors.Any(e => BusinessErrorCodes.IsConflict(e.Code))
                    ? Results.Conflict(new { errors = result.Errors })
                    : Results.BadRequest(new { errors = result.Errors });
        })
        .WithName("CreateAcademicYear")
        .Produces<CreateAcademicYearResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{id:guid}/set-current", async (Guid id, IMediator mediator, CancellationToken ct) =>
        {
            Result<SetCurrentAcademicYearResult> result = await mediator.Send(
                new SetCurrentAcademicYearCommand(id),
                ct);

            return result.IsSuccess
                ? Results.Ok(result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("SetCurrentAcademicYear")
        .Produces<SetCurrentAcademicYearResult>()
        .ProducesProblem(StatusCodes.Status404NotFound);

        return group;
    }

    private sealed record CreateAcademicYearRequest(
        string Name,
        DateOnly StartDate,
        DateOnly EndDate,
        bool? SetAsCurrent = null);
}
