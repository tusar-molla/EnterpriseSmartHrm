using EnterpriseSmartHrm.Application.Common.Abstractions;
using EnterpriseSmartHrm.Infrastructure.AuditLogs;
using EnterpriseSmartHrm.Infrastructure.Database;
using EnterpriseSmartHrm.Infrastructure.System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseSmartHrm.Infrastructure.DependencyInjection;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
        }

        services.AddSingleton<IDbConnectionFactory>(_ =>
            new SqlServerConnectionFactory(connectionString));
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}
