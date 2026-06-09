using EnterpriseSmartHrm.Domain.Authentication;

namespace EnterpriseSmartHrm.Application.Authentication.Abstractions;

public interface ILoginHistoryRepository
{
    Task<long> CreateAsync(
        LoginHistory loginHistory,
        CancellationToken cancellationToken = default);
}
