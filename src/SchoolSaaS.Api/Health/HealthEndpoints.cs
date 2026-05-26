using StackExchange.Redis;

namespace SchoolSaaS.Api.Health;

public static class HealthEndpoints
{
    public static IEndpointRouteBuilder MapHealthEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }))
            .ExcludeFromDescription();

        app.MapGet("/health/ready", async (IConnectionMultiplexer redis) =>
        {
            var ping = await redis.GetDatabase().PingAsync();
            return Results.Ok(new
            {
                status = "healthy",
                redis = new
                {
                    connected = redis.IsConnected,
                    pingMs = ping.TotalMilliseconds
                }
            });
        })
        .WithName("HealthReady")
        .WithTags("Health")
        .ExcludeFromDescription();

        return app;
    }
}
