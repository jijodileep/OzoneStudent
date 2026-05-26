namespace SchoolSaaS.Shared.Authorization;

[AttributeUsage(AttributeTargets.Class, Inherited = true)]
public sealed class AuditReadAttribute(string category, string? entityType = null) : Attribute
{
    public string Category { get; } = category;

    public string? EntityType { get; } = entityType;
}
