namespace EnterpriseSmartHrm.Application.Common.Interfaces;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    DateTime LocalNow { get; }

    DateOnly Today { get; }
}
