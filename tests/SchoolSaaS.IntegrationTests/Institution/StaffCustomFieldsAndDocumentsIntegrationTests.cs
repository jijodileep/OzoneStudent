using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using FluentAssertions;
using SchoolSaaS.Domain.Institution;
using SchoolSaaS.Infrastructure.Identity;

namespace SchoolSaaS.IntegrationTests.Institution;

public sealed class StaffCustomFieldsAndDocumentsIntegrationTests : IntegrationTestBase
{
    public StaffCustomFieldsAndDocumentsIntegrationTests(DatabaseFixture fixture)
        : base(fixture)
    {
    }

    [Fact]
    public async Task StaffCustomFieldsAndDocuments_WorkEndToEnd()
    {
        await IdentityDataSeeder.SeedAsync(Factory.Services);
        var client = await CreateAuthenticatedClientAsync();
        var suffix = Guid.NewGuid().ToString("N")[..8];

        var defineField = await client.PostAsJsonAsync(
            "/api/v1/custom-fields",
            new
            {
                entityType = CustomFieldEntityType.Staff.ToString(),
                fieldKey = $"department_{suffix}",
                label = "Department",
                dataType = CustomFieldDataType.Text.ToString(),
                isRequired = true
            });
        defineField.StatusCode.Should().Be(HttpStatusCode.Created);

        var createStaff = await client.PostAsJsonAsync(
            "/api/v1/staff",
            new
            {
                employeeCode = $"DOC{suffix}",
                designation = "Coordinator",
                joinDate = "2026-04-01",
                customFields = new Dictionary<string, string?>
                {
                    [$"department_{suffix}"] = "Science"
                }
            });
        createStaff.StatusCode.Should().Be(HttpStatusCode.Created);
        var staff = await createStaff.Content.ReadFromJsonAsync<StaffResponse>();

        var getStaff = await client.GetAsync($"/api/v1/staff/{staff!.Id}");
        getStaff.StatusCode.Should().Be(HttpStatusCode.OK);
        var staffBody = await getStaff.Content.ReadFromJsonAsync<StaffDetailResponse>();
        staffBody!.CustomFields.Should().ContainSingle(x => x.Value == "Science");

        using var content = new MultipartFormDataContent();
        content.Add(new StringContent("contract"), "documentType");
        content.Add(new ByteArrayContent(Encoding.UTF8.GetBytes("sample pdf content"))
        {
            Headers = { ContentType = new MediaTypeHeaderValue("application/pdf") }
        }, "file", "contract.pdf");

        var upload = await client.PostAsync($"/api/v1/staff/{staff.Id}/documents", content);
        upload.StatusCode.Should().Be(HttpStatusCode.Created);

        var listDocs = await client.GetAsync($"/api/v1/staff/{staff.Id}/documents");
        listDocs.StatusCode.Should().Be(HttpStatusCode.OK);
        var docs = await listDocs.Content.ReadFromJsonAsync<DocumentListResponse>();
        docs!.Items.Should().ContainSingle(x => x.DocumentType == "contract");
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

    private sealed record StaffResponse(Guid Id);

    private sealed record StaffDetailResponse(IReadOnlyList<CustomFieldItem> CustomFields);

    private sealed record CustomFieldItem(string FieldKey, string? Value);

    private sealed record DocumentListResponse(IReadOnlyList<DocumentItem> Items);

    private sealed record DocumentItem(string DocumentType);
}
