namespace SchoolSaaS.Shared.Authorization;

/// <summary>
/// MVP permission codes: {module}.{resource}.{action}
/// </summary>
public static class PermissionCatalog
{
    public static IReadOnlyList<PermissionDefinition> All { get; } =
    [
        // Auth / users
        new("auth.profile.read", "Read own profile", "auth"),
        new("auth.profile.update", "Update own profile", "auth"),
        new("auth.password.change", "Change own password", "auth"),
        new("users.invite", "Invite users", "auth"),
        new("users.manage", "Manage users", "auth"),

        // RBAC
        new("roles.role.read", "View roles", "rbac"),
        new("roles.role.create", "Create roles", "rbac"),
        new("roles.role.update", "Update roles", "rbac"),
        new("roles.role.delete", "Delete roles", "rbac"),
        new("roles.permissions.assign", "Assign permissions to roles", "rbac"),
        new("roles.user-role.assign", "Assign roles to users", "rbac"),

        // Audit
        new("audit.logs.read", "View audit logs", "audit"),
        new("audit.logs.export", "Export audit logs", "audit"),

        // Platform (Sprint 1)
        new("platform.tenant.read", "View tenant settings", "platform"),
        new("platform.tenant.update", "Update tenant settings", "platform"),

        // Institution / tenant provisioning
        new("institution.tenant.create", "Create and provision a new tenant", "institution"),
        new("institution.academic-years.manage", "Manage academic years and terms", "institution"),
        new("institution.classes.manage", "Manage grades, classes, and sections", "institution"),
        new("institution.subjects.manage", "Manage subjects and curriculum mapping", "institution"),
        new("institution.staff.manage", "Manage staff profiles", "institution"),
        new("institution.staff.fields.manage", "Manage staff custom field definitions", "institution"),
        new("institution.staff.documents.manage", "Manage staff documents", "institution"),
        new("students.documents.manage", "Manage student documents", "students"),
    ];

    public sealed record PermissionDefinition(string Code, string Name, string Module);
}
