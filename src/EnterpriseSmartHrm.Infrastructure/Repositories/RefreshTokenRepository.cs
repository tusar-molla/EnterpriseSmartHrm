using Dapper;
using EnterpriseSmartHrm.Application.Common.Interfaces;
using EnterpriseSmartHrm.Application.Features.Authentication.Interfaces;
using EnterpriseSmartHrm.Domain.Authentication;

namespace EnterpriseSmartHrm.Infrastructure.Repositories;

public sealed class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RefreshTokenRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(
        string tokenHash,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT Id, UserId, TokenHash, ExpiresAtUtc, CreatedAtUtc, CreatedByIp,
                   RevokedAtUtc, RevokedByIp, ReplacedByTokenHash, RevokeReason
            FROM auth.RefreshTokens
            WHERE TokenHash = @TokenHash;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<RefreshToken>(
            new CommandDefinition(sql, new { TokenHash = tokenHash }, cancellationToken: cancellationToken));
    }

    public async Task<long> CreateAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO auth.RefreshTokens
                (UserId, TokenHash, ExpiresAtUtc, CreatedAtUtc, CreatedByIp)
            OUTPUT INSERTED.Id
            VALUES
                (@UserId, @TokenHash, @ExpiresAtUtc, @CreatedAtUtc, @CreatedByIp);
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, refreshToken, cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(
        RefreshToken refreshToken,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE auth.RefreshTokens
            SET RevokedAtUtc = @RevokedAtUtc,
                RevokedByIp = @RevokedByIp,
                ReplacedByTokenHash = @ReplacedByTokenHash,
                RevokeReason = @RevokeReason
            WHERE Id = @Id;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(sql, refreshToken, cancellationToken: cancellationToken));
    }

    public async Task RevokeAllActiveForUserAsync(
        int userId,
        DateTime revokedAtUtc,
        string? revokedByIp,
        string? reason,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE auth.RefreshTokens
            SET RevokedAtUtc = @RevokedAtUtc,
                RevokedByIp = @RevokedByIp,
                RevokeReason = @Reason
            WHERE UserId = @UserId
              AND RevokedAtUtc IS NULL
              AND ExpiresAtUtc > @RevokedAtUtc;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { UserId = userId, RevokedAtUtc = revokedAtUtc, RevokedByIp = revokedByIp, Reason = reason },
                cancellationToken: cancellationToken));
    }
}
