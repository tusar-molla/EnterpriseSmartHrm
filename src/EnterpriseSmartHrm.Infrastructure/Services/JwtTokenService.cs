using EnterpriseSmartHrm.Application.Authentication.Abstractions;
using EnterpriseSmartHrm.Application.Authentication.Models;
using EnterpriseSmartHrm.Application.Common.Abstractions;
using EnterpriseSmartHrm.Application.Common.Security;
using EnterpriseSmartHrm.Domain.Authentication;
using Microsoft.IdentityModel.Tokens;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EnterpriseSmartHrm.Infrastructure.Authentication;

public sealed class JwtTokenService : ITokenService
{
    private const int RefreshTokenSize = 64;

    private readonly JwtSettings _settings;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly SigningCredentials _signingCredentials;

    public JwtTokenService(
        JwtSettings settings,
        IDateTimeProvider dateTimeProvider)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(dateTimeProvider);

        if (string.IsNullOrWhiteSpace(settings.Issuer))
        {
            throw new InvalidOperationException("JWT issuer is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Audience))
        {
            throw new InvalidOperationException("JWT audience is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.Secret)
            || Encoding.UTF8.GetByteCount(settings.Secret) < 32)
        {
            throw new InvalidOperationException("JWT secret must contain at least 32 bytes.");
        }

        if (settings.AccessTokenExpirationMinutes <= 0)
        {
            throw new InvalidOperationException("JWT access token expiration must be greater than zero.");
        }

        if (settings.RefreshTokenExpirationDays <= 0)
        {
            throw new InvalidOperationException("JWT refresh token expiration must be greater than zero.");
        }

        _settings = settings;
        _dateTimeProvider = dateTimeProvider;
        _signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.Secret)),
            SecurityAlgorithms.HmacSha256);
    }

    public GeneratedAccessToken GenerateAccessToken(
        User user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        ArgumentNullException.ThrowIfNull(user);
        ArgumentNullException.ThrowIfNull(roles);
        ArgumentNullException.ThrowIfNull(permissions);

        var issuedAtUtc = _dateTimeProvider.UtcNow;
        var expiresAtUtc = issuedAtUtc.AddMinutes(_settings.AccessTokenExpirationMinutes);
        var claims = CreateClaims(user, roles, permissions);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: issuedAtUtc,
            expires: expiresAtUtc,
            signingCredentials: _signingCredentials);

        return new GeneratedAccessToken(
            new JwtSecurityTokenHandler().WriteToken(token),
            expiresAtUtc);
    }

    public GeneratedRefreshToken GenerateRefreshToken()
    {
        var value = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(RefreshTokenSize));
        var hash = HashRefreshToken(value);
        var expiresAtUtc = _dateTimeProvider.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);

        return new GeneratedRefreshToken(value, hash, expiresAtUtc);
    }

    public string HashRefreshToken(string refreshToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(refreshToken);

        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToHexStringLower(hash);
    }

    private static IReadOnlyCollection<Claim> CreateClaims(
        User user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions)
    {
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimConstants.UserId, user.Id.ToString(CultureInfo.InvariantCulture)),
            new(ClaimConstants.Email, user.Email)
        };

        if (user.EmployeeId.HasValue)
        {
            claims.Add(new Claim(
                ClaimConstants.EmployeeId,
                user.EmployeeId.Value.ToString(CultureInfo.InvariantCulture)));
        }

        claims.AddRange(
            roles
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(role => new Claim(ClaimTypes.Role, role)));

        claims.AddRange(
            permissions
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(permission => new Claim(ClaimConstants.Permission, permission)));

        return claims;
    }
}
