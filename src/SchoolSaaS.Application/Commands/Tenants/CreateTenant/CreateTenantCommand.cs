using SchoolSaaS.Application.Common;
using SchoolSaaS.Shared.Authorization;

namespace SchoolSaaS.Application.Commands.Tenants.CreateTenant;

[RequirePermission(PermissionCodes.InstitutionTenantCreate)]
public sealed record CreateTenantCommand(
    string Name,
    string Slug,
    string? DbServer = null,
    int? DbPort = null,
    string? DbName = null,
    string? DbUser = null,
    string? DbPassword = null,
    string Plan = "free",
    string? AdminEmail = null,
    string? AdminPassword = null,
    string? AdminFirstName = null,
    string? AdminLastName = null) : IPlatformCommand<CreateTenantResult>;

public sealed record CreateTenantResult(
    Guid TenantId,
    string Slug,
    string DbName,
    string DbServer,
    Guid? AdminUserId);

