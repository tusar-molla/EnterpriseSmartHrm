namespace EnterpriseSmartHrm.Domain.Common;

public abstract class SoftDeletableEntity : AuditableEntity
{
    public bool IsDeleted { get; set; }

    public DateTime? DeletedAtUtc { get; set; }

    public int? DeletedBy { get; set; }
}
