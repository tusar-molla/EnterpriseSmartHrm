namespace EnterpriseSmartHrm.Application.Common.Abstractions;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }

    DateTime LocalNow { get; }

    DateOnly Today { get; }
}
