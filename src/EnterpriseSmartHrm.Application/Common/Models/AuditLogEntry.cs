namespace EnterpriseSmartHrm.Application.Common.Models;

public sealed record AuditLogEntry
{
    public int? UserId { get; init; }

    public int? EmployeeId { get; init; }

    public string ModuleName { get; init; } = string.Empty;

    public string Action { get; init; } = string.Empty;

    public string Description { get; init; } = string.Empty;

    public string? OldValue { get; init; }

    public string? NewValue { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    public DateTime CreatedAt { get; init; }
}
