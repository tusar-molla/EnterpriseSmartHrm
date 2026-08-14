using EnterpriseSmartHrm.Application.Common.Interfaces;

namespace EnterpriseSmartHrm.Infrastructure.Services;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime LocalNow => DateTime.Now;

    public DateOnly Today => DateOnly.FromDateTime(LocalNow);
}
