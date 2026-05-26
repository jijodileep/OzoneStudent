using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SchoolSaaS.IntegrationTests.Platform;

public class PingEndpointTests : IntegrationTestBase
{
    private readonly HttpClient _client;

    public PingEndpointTests(DatabaseFixture fixture)
        : base(fixture)
    {
        _client = Factory.CreateClient();
    }

    [Fact]
    public async Task Ping_ReturnsPong()
    {
        var response = await _client.GetAsync("/api/v1/ping");
        var bodyText = await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(HttpStatusCode.OK, because: bodyText);

        var body = await response.Content.ReadFromJsonAsync<PingResponse>();
        body!.Message.Should().Be("pong");
    }

    private sealed record PingResponse(string Message);
}
