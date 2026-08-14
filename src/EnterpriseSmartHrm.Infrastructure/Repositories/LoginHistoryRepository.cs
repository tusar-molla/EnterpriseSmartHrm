using Dapper;
using EnterpriseSmartHrm.Application.Authentication.Abstractions;
using EnterpriseSmartHrm.Application.Common.Abstractions;
using EnterpriseSmartHrm.Domain.Authentication;

namespace EnterpriseSmartHrm.Infrastructure.Authentication.Repositories;

public sealed class LoginHistoryRepository : ILoginHistoryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LoginHistoryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<long> CreateAsync(
        LoginHistory loginHistory,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO auth.LoginHistories
                (UserId, UsernameOrEmail, IsSuccessful, FailureReason, IpAddress, UserAgent, OccurredAtUtc)
            OUTPUT INSERTED.Id
            VALUES
                (@UserId, @UsernameOrEmail, @IsSuccessful, @FailureReason, @IpAddress, @UserAgent, @OccurredAtUtc);
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<long>(
            new CommandDefinition(sql, loginHistory, cancellationToken: cancellationToken));
    }
}
