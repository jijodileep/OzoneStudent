namespace SchoolSaaS.Domain.Exceptions;

public sealed class CrossTenantWriteException : Exception
{
    public CrossTenantWriteException(Guid entityTenantId, Guid? contextTenantId)
        : base($"Cross-tenant write denied. Entity tenant: {entityTenantId}, context tenant: {contextTenantId}.")
    {
        EntityTenantId = entityTenantId;
        ContextTenantId = contextTenantId;
    }

    public Guid EntityTenantId { get; }

    public Guid? ContextTenantId { get; }
}
