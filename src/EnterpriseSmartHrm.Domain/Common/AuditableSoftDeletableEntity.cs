namespace EnterpriseSmartHrm.Domain.Common;

public abstract class AuditableSoftDeletableEntity : SoftDeletableEntity
{
    public bool IsActive { get; set; } = true;
}
