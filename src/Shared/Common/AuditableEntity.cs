namespace Shared.Common;

public abstract class AuditableEntity
{
    public DateTimeOffset Created { get; set; } = DateTimeOffset.UtcNow;

    public string? CreatedBy { get; protected set; }

    public DateTimeOffset? LastModified { get; set; }

    public string? LastModifiedBy { get; protected set; }
}