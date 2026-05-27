using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using SchoolSaaS.Infrastructure.Identity;

namespace SchoolSaaS.IntegrationTests.Institution;

public sealed class InstitutionStructureIntegrationTests : IntegrationTestBase
{
    public InstitutionStructureIntegrationTests(DatabaseFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task CreateGradeClassSectionAndStaff_SucceedsEndToEnd()
    {
        await IdentityDataSeeder.SeedAsync(Factory.Services);
        var client = await CreateAuthenticatedClientAsync();

        var suffix = Guid.NewGuid().ToString("N")[..8];

        var academicYear = await client.PostAsJsonAsync(
            "/api/v1/academic-years",
            new
            {
                name = $"AY-{suffix}",
                startDate = "2028-04-01",
                endDate = "2029-03-31",
                setAsCurrent = false
            });
        academicYear.StatusCode.Should().Be(HttpStatusCode.Created);
        var year = await academicYear.Content.ReadFromJsonAsync<AcademicYearResponse>();

        var grade = await client.PostAsJsonAsync(
            "/api/v1/grades",
            new { name = $"Grade {suffix}", code = suffix[..Math.Min(8, suffix.Length)], sortOrder = 1 });
        grade.StatusCode.Should().Be(HttpStatusCode.Created);
        var gradeBody = await grade.Content.ReadFromJsonAsync<GradeResponse>();

        var staff = await client.PostAsJsonAsync(
            "/api/v1/staff",
            new
            {
                employeeCode = $"EMP{suffix}",
                designation = "Teacher",
                joinDate = "2026-04-01"
            });
        staff.StatusCode.Should().Be(HttpStatusCode.Created);
        var staffBody = await staff.Content.ReadFromJsonAsync<StaffResponse>();

        var schoolClass = await client.PostAsJsonAsync(
            "/api/v1/classes",
            new
            {
                gradeId = gradeBody!.Id,
                academicYearId = year!.Id,
                name = $"C-{suffix}",
                capacity = 40,
                classTeacherId = staffBody!.Id
            });
        schoolClass.StatusCode.Should().Be(HttpStatusCode.Created);
        var classBody = await schoolClass.Content.ReadFromJsonAsync<ClassResponse>();

        var section = await client.PostAsJsonAsync(
            $"/api/v1/classes/{classBody!.Id}/sections",
            new { name = "A", capacity = 40 });
        section.StatusCode.Should().Be(HttpStatusCode.Created);

        var listClasses = await client.GetAsync(
            $"/api/v1/classes?gradeId={gradeBody.Id}&academicYearId={year.Id}");
        listClasses.StatusCode.Should().Be(HttpStatusCode.OK);
        var classes = await listClasses.Content.ReadFromJsonAsync<ListClassesResponse>();
        classes!.Items.Should().ContainSingle(x => x.Name == $"C-{suffix}" && x.Sections.Count == 1);
    }

    [Fact]
    public async Task CreateClass_WithZeroCapacity_ReturnsBadRequest()
    {
        await IdentityDataSeeder.SeedAsync(Factory.Services);
        var client = await CreateAuthenticatedClientAsync();

        var suffix = Guid.NewGuid().ToString("N")[..8];

        var year = await client.PostAsJsonAsync(
            "/api/v1/academic-years",
            new { name = $"AY2-{suffix}", startDate = "2030-04-01", endDate = "2031-03-31" });
        var yearBody = await year.Content.ReadFromJsonAsync<AcademicYearResponse>();

        var grade = await client.PostAsJsonAsync(
            "/api/v1/grades",
            new { name = $"Grade2-{suffix}", code = $"X{suffix}"[..Math.Min(9, $"X{suffix}".Length)] });
        var gradeBody = await grade.Content.ReadFromJsonAsync<GradeResponse>();

        var response = await client.PostAsJsonAsync(
            "/api/v1/classes",
            new
            {
                gradeId = gradeBody!.Id,
                academicYearId = yearBody!.Id,
                name = "2-A",
                capacity = 0
            });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
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

    private sealed record AcademicYearResponse(Guid Id);

    private sealed record GradeResponse(Guid Id);

    private sealed record StaffResponse(Guid Id);

    private sealed record ClassResponse(Guid Id);

    private sealed record ListClassesResponse(IReadOnlyList<ClassItem> Items);

    private sealed record ClassItem(string Name, IReadOnlyList<SectionItem> Sections);

    private sealed record SectionItem(string Name);
}
