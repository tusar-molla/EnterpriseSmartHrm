using EnterpriseSmartHrm.Application.Common.Abstractions;

namespace EnterpriseSmartHrm.Infrastructure.System;

public sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;

    public DateTime LocalNow => DateTime.Now;

    public DateOnly Today => DateOnly.FromDateTime(LocalNow);
}
