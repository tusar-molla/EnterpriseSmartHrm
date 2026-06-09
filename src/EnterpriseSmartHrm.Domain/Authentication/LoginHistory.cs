using EnterpriseSmartHrm.Domain.Common;

namespace EnterpriseSmartHrm.Domain.Authentication;

public sealed class LoginHistory : LongBaseEntity
{
    public int? UserId { get; set; }

    public string UsernameOrEmail { get; set; } = string.Empty;

    public bool IsSuccessful { get; set; }

    public string? FailureReason { get; set; }

    public string? IpAddress { get; set; }

    public string? UserAgent { get; set; }

    public DateTime OccurredAtUtc { get; set; }
}
