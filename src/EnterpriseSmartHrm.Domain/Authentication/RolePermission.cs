using EnterpriseSmartHrm.Domain.Common;

namespace EnterpriseSmartHrm.Domain.Authentication;

public sealed class RolePermission : AuditableEntity
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public bool IsAllowed { get; set; } = true;

    public void Allow(DateTime utcNow, int? actorId)
    {
        IsAllowed = true;
        UpdatedAtUtc = utcNow;
        UpdatedBy = actorId;
    }

    public void Deny(DateTime utcNow, int? actorId)
    {
        IsAllowed = false;
        UpdatedAtUtc = utcNow;
        UpdatedBy = actorId;
    }
}
