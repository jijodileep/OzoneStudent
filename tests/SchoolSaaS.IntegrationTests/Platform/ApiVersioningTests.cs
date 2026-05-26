using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace SchoolSaaS.IntegrationTests.Platform;

public class ApiVersioningTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiVersioningTests(WebApplicationFactory<Program> factory) =>
        _client = factory.WithWebHostBuilder(b =>
        {
            b.UseSetting("Outbox:Enabled", "false");
            b.UseSetting("Testing:SkipDataSeed", "true");
        }).CreateClient();

    [Fact]
    public async Task Health_V1_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/v1/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Health_V2_ReturnsNotFound()
    {
        var response = await _client.GetAsync("/api/v2/health");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
