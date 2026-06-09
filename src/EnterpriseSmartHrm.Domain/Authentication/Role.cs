using EnterpriseSmartHrm.Domain.Common;

namespace EnterpriseSmartHrm.Domain.Authentication;

public sealed class Role : AuditableSoftDeletableEntity
{
    public string Name { get; set; } = string.Empty;

    public string NormalizedName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public bool IsSystemRole { get; set; }

    public void UpdateDetails(
        string name,
        string normalizedName,
        string? description,
        DateTime utcNow,
        int? actorId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(normalizedName);

        Name = name;
        NormalizedName = normalizedName;
        Description = description;
        SetUpdated(utcNow, actorId);
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

    private void SetUpdated(DateTime utcNow, int? actorId)
    {
        UpdatedAtUtc = utcNow;
        UpdatedBy = actorId;
    }
}
