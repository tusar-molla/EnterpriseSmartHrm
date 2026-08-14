using EnterpriseSmartHrm.Application.Authentication.Models;

namespace EnterpriseSmartHrm.Application.Authentication.Abstractions;

public interface IPasswordHasher
{
    string Hash(string password);

    PasswordVerificationResult Verify(
        string password,
        string passwordHash);
}
