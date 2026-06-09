using EnterpriseSmartHrm.Domain.Common;

namespace EnterpriseSmartHrm.Domain.Authentication;

public sealed class User : AuditableSoftDeletableEntity
{
    public string Username { get; set; } = string.Empty;

    public string NormalizedUsername { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string NormalizedEmail { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public int? EmployeeId { get; set; }

    public DateTime? LastLoginAtUtc { get; set; }

    public int FailedLoginCount { get; set; }

    public DateTime? LockoutEndAtUtc { get; set; }

    public DateTime? PasswordChangedAtUtc { get; set; }

    public bool IsLockedOut(DateTime utcNow)
    {
        return LockoutEndAtUtc.HasValue && LockoutEndAtUtc.Value > utcNow;
    }

    public void Activate(DateTime utcNow, int? actorId)
    {
        IsActive = true;
        SetUpdated(utcNow, actorId);
    }

    public void Deactivate(DateTime utcNow, int? actorId)
    {
        IsActive = false;
        SetUpdated(utcNow, actorId);
    }

    public void RecordSuccessfulLogin(DateTime utcNow)
    {
        LastLoginAtUtc = utcNow;
        FailedLoginCount = 0;
        LockoutEndAtUtc = null;
        UpdatedAtUtc = utcNow;
    }

    public void RecordFailedLogin(
        DateTime utcNow,
        int maximumFailedAttempts,
        TimeSpan lockoutDuration)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumFailedAttempts);

        if (lockoutDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lockoutDuration),
                "Lockout duration must be greater than zero.");
        }

        FailedLoginCount++;
        UpdatedAtUtc = utcNow;

        if (FailedLoginCount >= maximumFailedAttempts)
        {
            LockoutEndAtUtc = utcNow.Add(lockoutDuration);
            FailedLoginCount = 0;
        }
    }

    public void Unlock(DateTime utcNow, int? actorId)
    {
        FailedLoginCount = 0;
        LockoutEndAtUtc = null;
        SetUpdated(utcNow, actorId);
    }

    public void ChangePassword(string passwordHash, DateTime utcNow, int? actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        PasswordHash = passwordHash;
        PasswordChangedAtUtc = utcNow;
        FailedLoginCount = 0;
        LockoutEndAtUtc = null;
        SetUpdated(utcNow, actorId);
    }

    public void SoftDelete(DateTime utcNow, int? actorId)
    {
        IsDeleted = true;
        IsActive = false;
        DeletedAtUtc = utcNow;
        DeletedBy = actorId;
        SetUpdated(utcNow, actorId);
    }

    private void SetUpdated(DateTime utcNow, int? actorId)
    {
        UpdatedAtUtc = utcNow;
        UpdatedBy = actorId;
    }
}
