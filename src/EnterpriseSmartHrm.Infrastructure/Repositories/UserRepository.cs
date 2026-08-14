using Dapper;
using EnterpriseSmartHrm.Application.Common.Interfaces;
using EnterpriseSmartHrm.Application.Features.Authentication.Interfaces;
using EnterpriseSmartHrm.Domain.Authentication;

namespace EnterpriseSmartHrm.Infrastructure.Repositories;

public sealed class UserRepository : IUserRepository
{
    private const string SelectColumns = """
        Id, Username, NormalizedUsername, Email, NormalizedEmail, PasswordHash,
        EmployeeId, IsActive, LastLoginAtUtc, FailedLoginCount, LockoutEndAtUtc,
        PasswordChangedAtUtc, CreatedAtUtc, CreatedBy, UpdatedAtUtc, UpdatedBy,
        IsDeleted, DeletedAtUtc, DeletedBy
        """;

    private readonly IDbConnectionFactory _connectionFactory;

    public UserRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<User?> GetByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM auth.Users
            WHERE Id = @UserId AND IsDeleted = 0;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
    }

    public async Task<User?> GetByNormalizedUsernameOrEmailAsync(
        string normalizedUsernameOrEmail,
        CancellationToken cancellationToken = default)
    {
        var sql = $"""
            SELECT {SelectColumns}
            FROM auth.Users
            WHERE (NormalizedUsername = @Value OR NormalizedEmail = @Value)
              AND IsDeleted = 0;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<User>(
            new CommandDefinition(sql, new { Value = normalizedUsernameOrEmail }, cancellationToken: cancellationToken));
    }

    public async Task<bool> UsernameExistsAsync(
        string normalizedUsername,
        int? excludingUserId = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM auth.Users
                WHERE NormalizedUsername = @NormalizedUsername
                  AND IsDeleted = 0
                  AND (@ExcludingUserId IS NULL OR Id <> @ExcludingUserId)
            ) THEN 1 ELSE 0 END;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                new { NormalizedUsername = normalizedUsername, ExcludingUserId = excludingUserId },
                cancellationToken: cancellationToken));
    }

    public async Task<bool> EmailExistsAsync(
        string normalizedEmail,
        int? excludingUserId = null,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM auth.Users
                WHERE NormalizedEmail = @NormalizedEmail
                  AND IsDeleted = 0
                  AND (@ExcludingUserId IS NULL OR Id <> @ExcludingUserId)
            ) THEN 1 ELSE 0 END;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<bool>(
            new CommandDefinition(
                sql,
                new { NormalizedEmail = normalizedEmail, ExcludingUserId = excludingUserId },
                cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyCollection<string>> GetRoleNamesAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT r.Name
            FROM auth.UserRoles ur
            INNER JOIN auth.Roles r ON r.Id = ur.RoleId
            WHERE ur.UserId = @UserId
              AND r.IsActive = 1
              AND r.IsDeleted = 0;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var roles = await connection.QueryAsync<string>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return roles.ToArray();
    }

    public async Task<IReadOnlyCollection<string>> GetPermissionKeysAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        const string sql = """
            SELECT DISTINCT p.PermissionKey
            FROM auth.UserRoles ur
            INNER JOIN auth.RolePermissions rp ON rp.RoleId = ur.RoleId
            INNER JOIN auth.Permissions p ON p.Id = rp.PermissionId
            INNER JOIN auth.Roles r ON r.Id = ur.RoleId
            WHERE ur.UserId = @UserId
              AND rp.IsAllowed = 1
              AND p.IsActive = 1
              AND r.IsActive = 1
              AND r.IsDeleted = 0;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        var permissions = await connection.QueryAsync<string>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return permissions.ToArray();
    }

    public async Task<int> CreateAsync(User user, CancellationToken cancellationToken = default)
    {
        const string sql = """
            INSERT INTO auth.Users
                (Username, NormalizedUsername, Email, NormalizedEmail, PasswordHash,
                 EmployeeId, IsActive, FailedLoginCount, CreatedAtUtc, CreatedBy)
            OUTPUT INSERTED.Id
            VALUES
                (@Username, @NormalizedUsername, @Email, @NormalizedEmail, @PasswordHash,
                 @EmployeeId, @IsActive, @FailedLoginCount, @CreatedAtUtc, @CreatedBy);
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, user, cancellationToken: cancellationToken));
    }

    public async Task UpdateAsync(User user, CancellationToken cancellationToken = default)
    {
        const string sql = """
            UPDATE auth.Users
            SET Username = @Username,
                NormalizedUsername = @NormalizedUsername,
                Email = @Email,
                NormalizedEmail = @NormalizedEmail,
                PasswordHash = @PasswordHash,
                EmployeeId = @EmployeeId,
                IsActive = @IsActive,
                LastLoginAtUtc = @LastLoginAtUtc,
                FailedLoginCount = @FailedLoginCount,
                LockoutEndAtUtc = @LockoutEndAtUtc,
                PasswordChangedAtUtc = @PasswordChangedAtUtc,
                UpdatedAtUtc = @UpdatedAtUtc,
                UpdatedBy = @UpdatedBy,
                IsDeleted = @IsDeleted,
                DeletedAtUtc = @DeletedAtUtc,
                DeletedBy = @DeletedBy
            WHERE Id = @Id;
            """;

        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(sql, user, cancellationToken: cancellationToken));
    }

    public async Task ReplaceRolesAsync(
        int userId,
        IReadOnlyCollection<int> roleIds,
        int? actorId,
        DateTime utcNow,
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await connection.ExecuteAsync(
            new CommandDefinition(
                "DELETE FROM auth.UserRoles WHERE UserId = @UserId;",
                new { UserId = userId },
                transaction,
                cancellationToken: cancellationToken));

        if (roleIds.Count > 0)
        {
            const string insertSql = """
                INSERT INTO auth.UserRoles (UserId, RoleId, CreatedAtUtc, CreatedBy)
                VALUES (@UserId, @RoleId, @CreatedAtUtc, @CreatedBy);
                """;

            var rows = roleIds
                .Distinct()
                .Select(roleId => new
                {
                    UserId = userId,
                    RoleId = roleId,
                    CreatedAtUtc = utcNow,
                    CreatedBy = actorId
                });

            await connection.ExecuteAsync(
                new CommandDefinition(insertSql, rows, transaction, cancellationToken: cancellationToken));
        }

        await transaction.CommitAsync(cancellationToken);
    }
}
