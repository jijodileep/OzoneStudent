using MediatR;
using SchoolSaaS.Application.Commands.Institution.CreateClass;
using SchoolSaaS.Application.Commands.Institution.CreateSection;
using SchoolSaaS.Application.Queries.Institution.ListClasses;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints.Institution;

public static class ClassEndpoints
{
    public static RouteGroupBuilder MapClassEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/classes")
            .WithTags("Institution")
            .RequireAuthorization();

        group.MapGet("/", async (
            Guid? gradeId,
            Guid? academicYearId,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<ListClassesResult> result = await mediator.Send(
                new ListClassesQuery(gradeId, academicYearId),
                ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
        })
        .WithName("ListClasses")
        .Produces<ListClassesResult>();

        group.MapPost("/", async (CreateClassRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<CreateClassResult> result = await mediator.Send(
                new CreateClassCommand(
                    body.GradeId,
                    body.AcademicYearId,
                    body.Name,
                    body.Capacity,
                    body.ClassTeacherId),
                ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/classes/{result.Value!.Id}", result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : result.Errors.Any(e => e.Code == ErrorFactory.ConflictCode)
                        ? Results.Conflict(result.Errors)
                        : Results.BadRequest(result.Errors);
        })
        .WithName("CreateClass")
        .Produces<CreateClassResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/{classId:guid}/sections", async (
            Guid classId,
            CreateSectionRequest body,
            IMediator mediator,
            CancellationToken ct) =>
        {
            Result<CreateSectionResult> result = await mediator.Send(
                new CreateSectionCommand(classId, body.Name, body.Capacity),
                ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/classes/{classId}/sections/{result.Value!.Id}", result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.NotFoundCode)
                    ? Results.NotFound(result.Errors)
                    : result.Errors.Any(e => e.Code == ErrorFactory.ConflictCode)
                        ? Results.Conflict(result.Errors)
                        : Results.BadRequest(result.Errors);
        })
        .WithName("CreateSection")
        .Produces<CreateSectionResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private sealed record CreateClassRequest(
        Guid GradeId,
        Guid AcademicYearId,
        string Name,
        int Capacity,
        Guid? ClassTeacherId = null);

    private sealed record CreateSectionRequest(string Name, int Capacity);
}
