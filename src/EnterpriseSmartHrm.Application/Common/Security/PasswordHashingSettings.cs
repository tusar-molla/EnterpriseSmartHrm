namespace EnterpriseSmartHrm.Application.Common.Security;

public sealed class PasswordHashingSettings
{
    public const string SectionName = "PasswordHashing";

    public int Iterations { get; init; } = 210_000;

    public int SaltSize { get; init; } = 16;

    public int HashSize { get; init; } = 32;
}
