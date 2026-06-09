namespace EnterpriseSmartHrm.Application.Authentication.Models;

public sealed record GeneratedAccessToken(
    string Value,
    DateTime ExpiresAtUtc);
