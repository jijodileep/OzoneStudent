namespace SchoolSaaS.Application.Abstractions.Audit;

public interface IAuditService
{
    Task LogAsync(AuditEntry entry, CancellationToken cancellationToken = default);
}

public sealed record AuditEntry(
    string Action,
    string Category,
    string? EntityType = null,
    Guid? EntityId = null,
    string? Description = null,
    string? BeforeJson = null,
    string? AfterJson = null,
    string? MetadataJson = null,
    string Outcome = AuditOutcomes.Success,
    string Source = AuditSources.Api);

public static class AuditOutcomes
{
    public const string Success = "Success";
    public const string Failed = "Failed";
    public const string Denied = "Denied";
}

public static class AuditSources
{
    public const string Api = "Api";
    public const string Job = "Job";
    public const string EventHandler = "EventHandler";
}

public static class AuditActions
{
    public const string Create = "Create";
    public const string Update = "Update";
    public const string Delete = "Delete";
    public const string Read = "Read";
    public const string Login = "Login";
    public const string LoginFailed = "LoginFailed";
    public const string Logout = "Logout";
    public const string TokenRefresh = "TokenRefresh";
    public const string TokenRefreshFailed = "TokenRefreshFailed";
    public const string AccessDenied = "AccessDenied";
    public const string PasswordResetRequest = "PasswordResetRequest";
    public const string PasswordResetComplete = "PasswordResetComplete";
    public const string PasswordChange = "PasswordChange";
    public const string UserInvited = "UserInvited";
    public const string InvitationAccepted = "InvitationAccepted";
}

public static class AuditCategories
{
    public const string Auth = "Auth";
    public const string Rbac = "Rbac";
    public const string Institution = "Institution";
    public const string Platform = "Platform";
    public const string Audit = "Audit";
}
