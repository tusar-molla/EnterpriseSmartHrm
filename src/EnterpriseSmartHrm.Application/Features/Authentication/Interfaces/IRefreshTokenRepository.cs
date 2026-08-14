using EnterpriseSmartHrm.Domain.Authentication;

namespace EnterpriseSmartHrm.Application.Authentication.Abstractions;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default);

    Task<long> CreateAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default);

    Task RevokeAllActiveForUserAsync(
        int userId,
        DateTime revokedAtUtc,
        string? revokedByIp,
        string? reason,
        CancellationToken cancellationToken = default);
}
