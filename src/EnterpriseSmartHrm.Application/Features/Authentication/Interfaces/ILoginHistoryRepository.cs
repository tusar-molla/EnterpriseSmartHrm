using EnterpriseSmartHrm.Domain.Authentication;

namespace EnterpriseSmartHrm.Application.Features.Authentication.Interfaces;

public interface ILoginHistoryRepository
{
    Task<long> CreateAsync(
        LoginHistory loginHistory,
        CancellationToken cancellationToken = default);
}
