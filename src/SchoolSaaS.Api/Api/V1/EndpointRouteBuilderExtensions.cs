using Asp.Versioning;
using Asp.Versioning.Builder;
using SchoolSaaS.Api.Endpoints.Audit;
using SchoolSaaS.Api.Endpoints.Auth;
using SchoolSaaS.Api.Endpoints.Institution;
using SchoolSaaS.Api.Endpoints.Platform;
using SchoolSaaS.Api.Endpoints.Rbac;
using SchoolSaaS.Api.Endpoints.Tenants;
using SchoolSaaS.Api.Endpoints.Users;

namespace SchoolSaaS.Api.Api.V1;

public static class EndpointRouteBuilderExtensions
{
    public static void MapV1Endpoints(this IEndpointRouteBuilder app)
    {
        ApiVersionSet versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        RouteGroupBuilder versionedApi = app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet);

        versionedApi.MapPlatformEndpoints();
        versionedApi.MapAuthEndpoints();
        versionedApi.MapUserEndpoints();
        versionedApi.MapAuditEndpoints();
        versionedApi.MapTenantEndpoints();
        versionedApi.MapAcademicYearEndpoints();
        versionedApi.MapGradeEndpoints();
        versionedApi.MapClassEndpoints();
        versionedApi.MapStaffEndpoints();
        versionedApi.MapStaffDocumentEndpoints();
        versionedApi.MapStudentDocumentEndpoints();
        versionedApi.MapCustomFieldEndpoints();
        versionedApi.MapRoleEndpoints();
    }
}
