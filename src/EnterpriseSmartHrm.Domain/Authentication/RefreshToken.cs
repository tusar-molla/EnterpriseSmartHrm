using EnterpriseSmartHrm.Domain.Common;

namespace EnterpriseSmartHrm.Domain.Authentication;

public sealed class RefreshToken : LongBaseEntity
{
    public int UserId { get; set; }

    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public string? CreatedByIp { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? RevokedByIp { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public string? RevokeReason { get; set; }

    public bool IsExpired(DateTime utcNow)
    {
        return ExpiresAtUtc <= utcNow;
    }

    public bool IsRevoked()
    {
        return RevokedAtUtc.HasValue;
    }

    public bool IsActive(DateTime utcNow)
    {
        return !IsRevoked() && !IsExpired(utcNow);
    }

    public void Revoke(
        DateTime utcNow,
        string? revokedByIp,
        string? reason,
        string? replacedByTokenHash = null)
    {
        if (IsRevoked())
        {
            return;
        }

        RevokedAtUtc = utcNow;
        RevokedByIp = revokedByIp;
        RevokeReason = reason;
        ReplacedByTokenHash = replacedByTokenHash;
    }
}
