using EnterpriseSmartHrm.Application.Authentication.Models;
using EnterpriseSmartHrm.Domain.Authentication;

namespace EnterpriseSmartHrm.Application.Authentication.Abstractions;

public interface ITokenService
{
    GeneratedAccessToken GenerateAccessToken(
        User user,
        IReadOnlyCollection<string> roles,
        IReadOnlyCollection<string> permissions);

    GeneratedRefreshToken GenerateRefreshToken();

    string HashRefreshToken(string refreshToken);
}
