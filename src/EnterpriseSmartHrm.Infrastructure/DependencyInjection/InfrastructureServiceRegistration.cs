using EnterpriseSmartHrm.Application.Authentication.Abstractions;
using EnterpriseSmartHrm.Application.Common.Abstractions;
using EnterpriseSmartHrm.Application.Common.Security;
using EnterpriseSmartHrm.Infrastructure.Authentication;
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

        var jwtSettings = configuration
            .GetSection(JwtSettings.SectionName)
            .Get<JwtSettings>()
            ?? throw new InvalidOperationException(
                $"Configuration section '{JwtSettings.SectionName}' was not found.");

        var passwordHashingSettings = configuration
            .GetSection(PasswordHashingSettings.SectionName)
            .Get<PasswordHashingSettings>()
            ?? new PasswordHashingSettings();

        services.AddSingleton<IDbConnectionFactory>(_ =>
            new SqlServerConnectionFactory(connectionString));
        services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.AddSingleton<IPasswordHasher>(_ =>
            new Pbkdf2PasswordHasher(passwordHashingSettings));
        services.AddSingleton<ITokenService>(serviceProvider =>
            new JwtTokenService(
                jwtSettings,
                serviceProvider.GetRequiredService<IDateTimeProvider>()));
        services.AddScoped<IAuditLogService, AuditLogService>();

        return services;
    }
}
