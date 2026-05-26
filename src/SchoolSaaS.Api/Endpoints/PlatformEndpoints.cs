using MediatR;
using SchoolSaaS.Application.Platform.Queries;
using SchoolSaaS.Shared.Results;

namespace SchoolSaaS.Api.Endpoints;

public static class PlatformEndpoints
{
    public static RouteGroupBuilder MapPlatformEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/v1")
            .WithTags("Platform");

        group.MapGet("/ping", async (IMediator mediator, CancellationToken ct) =>
        {
            var result = await mediator.Send(new PingQuery(), ct);
            return result.IsSuccess
                ? Results.Ok(new { message = result.Value })
                : Results.BadRequest(result.Errors);
        })
        .WithName("Ping")
        .Produces<object>(StatusCodes.Status200OK);

        group.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .WithName("HealthV1")
            .ExcludeFromDescription();

        return group;
    }
}
