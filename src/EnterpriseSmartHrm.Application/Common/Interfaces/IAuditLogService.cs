using EnterpriseSmartHrm.Application.Common.Models;

namespace EnterpriseSmartHrm.Application.Common.Interfaces;

public interface IAuditLogService
{
    Task LogAsync(AuditLogEntry entry, CancellationToken cancellationToken = default);
}
