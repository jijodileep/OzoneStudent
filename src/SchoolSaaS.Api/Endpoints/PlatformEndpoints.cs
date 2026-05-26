using Asp.Versioning;
using MediatR;
using SchoolSaaS.Application.Platform.Queries;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints;

public static class PlatformEndpoints
{
    public static RouteGroupBuilder MapPlatformEndpoints(this RouteGroupBuilder app)
    {
        RouteGroupBuilder group = app.MapGroup("/")
            .WithTags("Platform")
            .HasApiVersion(1, 0);

        group.MapGet("/ping", async (IMediator mediator, CancellationToken ct) =>
        {
            Result<string> result = await mediator.Send(new PingQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(new { message = result.Value })
                : Results.BadRequest(result.Errors);
        })
        .WithName("Ping")
        .Produces<object>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest);

        group.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("HealthV1")
            .Produces<object>(StatusCodes.Status200OK);

        return group;
    }
}
