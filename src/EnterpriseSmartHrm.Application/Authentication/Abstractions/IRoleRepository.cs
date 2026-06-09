using EnterpriseSmartHrm.Domain.Authentication;

namespace EnterpriseSmartHrm.Application.Authentication.Abstractions;

public interface IRoleRepository
{
    Task<Role?> GetByIdAsync(
        int roleId,
        CancellationToken cancellationToken = default);

    Task<Role?> GetByNormalizedNameAsync(
        string normalizedName,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Role>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Role>> GetByIdsAsync(
        IReadOnlyCollection<int> roleIds,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        Role role,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Role role,
        CancellationToken cancellationToken = default);
}
