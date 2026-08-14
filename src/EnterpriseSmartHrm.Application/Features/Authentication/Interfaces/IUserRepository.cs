using EnterpriseSmartHrm.Domain.Authentication;

namespace EnterpriseSmartHrm.Application.Features.Authentication.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<User?> GetByNormalizedUsernameOrEmailAsync(
        string normalizedUsernameOrEmail,
        CancellationToken cancellationToken = default);

    Task<bool> UsernameExistsAsync(
        string normalizedUsername,
        int? excludingUserId = null,
        CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(
        string normalizedEmail,
        int? excludingUserId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetRoleNamesAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyCollection<string>> GetPermissionKeysAsync(
        int userId,
        CancellationToken cancellationToken = default);

    Task<int> CreateAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        User user,
        CancellationToken cancellationToken = default);

    Task ReplaceRolesAsync(
        int userId,
        IReadOnlyCollection<int> roleIds,
        int? actorId,
        DateTime utcNow,
        CancellationToken cancellationToken = default);
}
