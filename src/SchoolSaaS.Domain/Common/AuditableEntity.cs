namespace SchoolSaaS.Domain.Common;

public abstract class AuditableEntity : BaseEntity
{
    public string? LastModifiedReason { get; set; }
}
