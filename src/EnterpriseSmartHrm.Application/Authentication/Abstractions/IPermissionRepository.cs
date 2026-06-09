using EnterpriseSmartHrm.Domain.Authentication;

namespace EnterpriseSmartHrm.Application.Authentication.Abstractions;

public interface IPermissionRepository
{
    Task<IReadOnlyCollection<Permission>> GetAllAsync(
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Permission>> GetByRoleIdAsync(
        int roleId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<Permission>> GetByIdsAsync(
        IReadOnlyCollection<int> permissionIds,
        CancellationToken cancellationToken = default);

    Task ReplaceRolePermissionsAsync(
        int roleId,
        IReadOnlyCollection<int> permissionIds,
        int? actorId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
