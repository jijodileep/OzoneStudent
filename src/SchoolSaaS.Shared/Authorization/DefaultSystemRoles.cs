namespace SchoolSaaS.Shared.Authorization;

public static class DefaultSystemRoles
{
    public const string TenantAdmin = "tenant_admin";
    public const string Teacher = "teacher";
    public const string Accountant = "accountant";
    public const string Parent = "parent";
    public const string Student = "student";

    public static IReadOnlyList<(string Name, string Description)> All { get; } =
    [
        (TenantAdmin, "Full tenant administrator"),
        (Teacher, "Class teacher — scoped to assigned classes"),
        (Accountant, "Fee and finance operations"),
        (Parent, "Parent portal — linked children only"),
        (Student, "Student portal — own record only")
    ];
}
