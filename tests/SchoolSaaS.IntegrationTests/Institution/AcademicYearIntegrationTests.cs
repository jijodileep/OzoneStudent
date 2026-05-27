using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SchoolSaaS.Infrastructure.Identity;

namespace SchoolSaaS.IntegrationTests.Institution;

public sealed class AcademicYearIntegrationTests : IntegrationTestBase
{
    public AcademicYearIntegrationTests(DatabaseFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task CreateAndListAcademicYears_WorksForTenantAdmin()
    {
        await IdentityDataSeeder.SeedAsync(Factory.Services);

        var client = await CreateAuthenticatedClientAsync();

        var create = await client.PostAsJsonAsync(
            "/api/v1/academic-years",
            new
            {
                name = "2026-27",
                startDate = "2026-04-01",
                endDate = "2027-03-31",
                setAsCurrent = true
            });

        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var created = await create.Content.ReadFromJsonAsync<CreateAcademicYearResponse>();
        created.Should().NotBeNull();
        created!.IsCurrent.Should().BeTrue();

        var list = await client.GetAsync("/api/v1/academic-years");
        list.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await list.Content.ReadFromJsonAsync<ListAcademicYearsResponse>();
        body!.TotalCount.Should().BeGreaterThanOrEqualTo(1);
        body.Items.Should().ContainSingle(x => x.Name == "2026-27" && x.IsCurrent);
    }

    [Fact]
    public async Task CreateAcademicYear_WithOverlappingDates_ReturnsConflict()
    {
        await IdentityDataSeeder.SeedAsync(Factory.Services);

        var client = await CreateAuthenticatedClientAsync();

        await client.PostAsJsonAsync(
            "/api/v1/academic-years",
            new
            {
                name = "2025-26",
                startDate = "2025-04-01",
                endDate = "2026-03-31"
            });

        var overlap = await client.PostAsJsonAsync(
            "/api/v1/academic-years",
            new
            {
                name = "2025-26 overlap",
                startDate = "2026-01-01",
                endDate = "2026-06-30"
            });

        overlap.StatusCode.Should().Be(HttpStatusCode.Conflict);

        var errorBody = await overlap.Content.ReadFromJsonAsync<ApiErrorResponse>();
        errorBody.Should().NotBeNull();
        errorBody!.Errors.Should().Contain(e => e.Code == "institution.academic_year.dates_overlap");
    }

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = Factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Slug", IdentityDataSeeder.DefaultTenantSlug);

        var login = await client.PostAsJsonAsync(
            "/api/v1/auth/login",
            new { email = IdentityDataSeeder.DefaultAdminEmail, password = IdentityDataSeeder.DefaultAdminPassword });

        var tokens = await login.Content.ReadFromJsonAsync<LoginResponse>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", tokens!.AccessToken);

        return client;
    }

    private sealed record LoginResponse(string AccessToken);

    private sealed record CreateAcademicYearResponse(
        Guid Id,
        string Name,
        DateOnly StartDate,
        DateOnly EndDate,
        bool IsCurrent,
        string Status);

    private sealed record ListAcademicYearsResponse(
        IReadOnlyList<AcademicYearItem> Items,
        int TotalCount,
        int Page,
        int PageSize);

    private sealed record AcademicYearItem(
        Guid Id,
        string Name,
        DateOnly StartDate,
        DateOnly EndDate,
        bool IsCurrent,
        string Status);

    private sealed record ApiErrorResponse(IReadOnlyList<ApiErrorItem> Errors);

    private sealed record ApiErrorItem(string Code, string Message);
}
