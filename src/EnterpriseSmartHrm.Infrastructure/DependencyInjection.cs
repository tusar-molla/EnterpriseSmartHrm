using EnterpriseSmartHrm.Application.Common.Interfaces;
using EnterpriseSmartHrm.Application.Common.Security;
using EnterpriseSmartHrm.Application.Features.Authentication.Interfaces;
using EnterpriseSmartHrm.Infrastructure.Database;
using EnterpriseSmartHrm.Infrastructure.Repositories;
using EnterpriseSmartHrm.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace EnterpriseSmartHrm.Infrastructure;

public static class DependencyInjection
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

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<ILoginHistoryRepository, LoginHistoryRepository>();

        return services;
    }
}
