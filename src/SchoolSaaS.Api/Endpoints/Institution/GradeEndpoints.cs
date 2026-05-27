using MediatR;
using SchoolSaaS.Application.Commands.Institution.CreateGrade;
using SchoolSaaS.Application.Queries.Institution.ListGrades;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints.Institution;

public static class GradeEndpoints
{
    public static RouteGroupBuilder MapGradeEndpoints(this RouteGroupBuilder app)
    {
        var group = app.MapGroup("/grades")
            .WithTags("Institution")
            .RequireAuthorization();

        group.MapGet("/", async (IMediator mediator, CancellationToken ct) =>
        {
            Result<ListGradesResult> result = await mediator.Send(new ListGradesQuery(), ct);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Errors);
        })
        .WithName("ListGrades")
        .Produces<ListGradesResult>();

        group.MapPost("/", async (CreateGradeRequest body, IMediator mediator, CancellationToken ct) =>
        {
            Result<CreateGradeResult> result = await mediator.Send(
                new CreateGradeCommand(body.Name, body.Code, body.SortOrder ?? 0),
                ct);

            return result.IsSuccess
                ? Results.Created($"/api/v1/grades/{result.Value!.Id}", result.Value)
                : result.Errors.Any(e => e.Code == ErrorFactory.ConflictCode)
                    ? Results.Conflict(result.Errors)
                    : Results.BadRequest(result.Errors);
        })
        .WithName("CreateGrade")
        .Produces<CreateGradeResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status409Conflict);

        return group;
    }

    private sealed record CreateGradeRequest(string Name, string Code, int? SortOrder = null);
}
