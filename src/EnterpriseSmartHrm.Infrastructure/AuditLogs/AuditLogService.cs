using EnterpriseSmartHrm.Application.Common.Abstractions;
using EnterpriseSmartHrm.Application.Common.Models;
using Microsoft.Extensions.Logging;

namespace EnterpriseSmartHrm.Infrastructure.AuditLogs;

public sealed class AuditLogService : IAuditLogService
{
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTimeProvider _dateTime;
    private readonly ILogger<AuditLogService> _logger;

    public AuditLogService(
        ICurrentUserService currentUser,
        IDateTimeProvider dateTime,
        ILogger<AuditLogService> logger)
    {
        _currentUser = currentUser;
        _dateTime = dateTime;
        _logger = logger;
    }

    public Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default)
    {
        var enrichedEntry = entry with
        {
            UserId = entry.UserId ?? _currentUser.UserId,
            EmployeeId = entry.EmployeeId ?? _currentUser.EmployeeId,
            CreatedAt = entry.CreatedAt == default ? _dateTime.UtcNow : entry.CreatedAt
        };

        _logger.LogInformation(
            "AuditLog: UserId={UserId}, EmployeeId={EmployeeId}, Module={ModuleName}, Action={Action}, Description={Description}, CreatedAt={CreatedAt}",
            enrichedEntry.UserId,
            enrichedEntry.EmployeeId,
            enrichedEntry.ModuleName,
            enrichedEntry.Action,
            enrichedEntry.Description,
            enrichedEntry.CreatedAt);

        return Task.CompletedTask;
    }
}
