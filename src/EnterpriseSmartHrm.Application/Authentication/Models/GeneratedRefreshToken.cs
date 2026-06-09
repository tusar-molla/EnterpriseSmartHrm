namespace EnterpriseSmartHrm.Application.Authentication.Models;

public sealed record GeneratedRefreshToken(
    string Value,
    string Hash,
    DateTime ExpiresAtUtc);
