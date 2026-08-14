using EnterpriseSmartHrm.Domain.Authentication;

namespace EnterpriseSmartHrm.Application.Features.Authentication.Interfaces;

public interface ITokenService
{
    GeneratedAccessToken GenerateAccessToken(
        User user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions);

    GeneratedRefreshToken GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}

public sealed record GeneratedAccessToken(
    string Value,
    DateTime ExpiresAtUtc);

public sealed record GeneratedRefreshToken(
    string Value,
    string Hash,
    DateTime ExpiresAtUtc);
