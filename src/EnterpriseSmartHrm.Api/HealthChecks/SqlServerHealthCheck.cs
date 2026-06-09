using EnterpriseSmartHrm.Application.Common.Abstractions;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EnterpriseSmartHrm.Api.HealthChecks;

public sealed class SqlServerHealthCheck(
    IDbConnectionFactory connectionFactory,
    ILogger<SqlServerHealthCheck> logger) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = connectionFactory.CreateConnection();
            await connection.OpenAsync(cancellationToken);

            return HealthCheckResult.Healthy("SQL Server connection succeeded.");
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "SQL Server health check failed.");

            return HealthCheckResult.Unhealthy(
                "SQL Server connection failed.",
                exception);
        }
    }
}
