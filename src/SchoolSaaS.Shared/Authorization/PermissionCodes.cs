namespace SchoolSaaS.Shared.Authorization;

public static class PermissionCodes
{
    public const string AuthProfileRead = "auth.profile.read";
    public const string AuthProfileUpdate = "auth.profile.update";
    public const string AuthPasswordChange = "auth.password.change";
    public const string UsersInvite = "users.invite";
    public const string UsersManage = "users.manage";

    public const string InstitutionTenantCreate = "institution.tenant.create";
    public const string InstitutionAcademicYearsManage = "institution.academic-years.manage";
    public const string InstitutionClassesManage = "institution.classes.manage";
    public const string InstitutionSubjectsManage = "institution.subjects.manage";
    public const string InstitutionStaffManage = "institution.staff.manage";
    public const string InstitutionStaffFieldsManage = "institution.staff.fields.manage";
    public const string InstitutionStaffDocumentsManage = "institution.staff.documents.manage";
    public const string StudentsDocumentsManage = "students.documents.manage";

    public const string RolesRoleRead = "roles.role.read";
    public const string RolesRoleCreate = "roles.role.create";
    public const string RolesPermissionsAssign = "roles.permissions.assign";
    public const string RolesUserRoleAssign = "roles.user-role.assign";

    public const string AuditLogsRead = "audit.logs.read";
}
