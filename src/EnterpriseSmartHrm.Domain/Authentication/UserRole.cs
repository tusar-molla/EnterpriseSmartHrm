using EnterpriseSmartHrm.Domain.Common;

namespace EnterpriseSmartHrm.Domain.Authentication;

public sealed class UserRole : BaseEntity
{
    public int UserId { get; set; }

    public int RoleId { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public int? CreatedBy { get; set; }
}
