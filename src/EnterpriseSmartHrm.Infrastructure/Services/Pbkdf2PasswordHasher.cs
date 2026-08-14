using EnterpriseSmartHrm.Application.Common.Security;
using EnterpriseSmartHrm.Application.Features.Authentication.Interfaces;
using System.Globalization;
using System.Security.Cryptography;

namespace EnterpriseSmartHrm.Infrastructure.Services;

public sealed class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const string Algorithm = "pbkdf2-sha512";
    private const int FormatVersion = 1;

    private readonly PasswordHashingSettings _settings;

    public Pbkdf2PasswordHasher(PasswordHashingSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (settings.Iterations < 100_000)
        {
            throw new InvalidOperationException("Password hashing iterations must be at least 100,000.");
        }

        if (settings.SaltSize < 16)
        {
            throw new InvalidOperationException("Password hashing salt size must be at least 16 bytes.");
        }

        if (settings.HashSize < 32)
        {
            throw new InvalidOperationException("Password hashing hash size must be at least 32 bytes.");
        }

        _settings = settings;
    }

    public string Hash(string password)
    {
        ArgumentException.ThrowIfNullOrEmpty(password);

        var salt = RandomNumberGenerator.GetBytes(_settings.SaltSize);
        var hash = DeriveHash(password, salt, _settings.Iterations, _settings.HashSize);

        return string.Join(
            '$',
            Algorithm,
            FormatVersion.ToString(CultureInfo.InvariantCulture),
            _settings.Iterations.ToString(CultureInfo.InvariantCulture),
            Convert.ToBase64String(salt),
            Convert.ToBase64String(hash));
    }

    public PasswordVerificationResult Verify(string password, string passwordHash)
    {
        if (string.IsNullOrEmpty(password) || string.IsNullOrWhiteSpace(passwordHash))
        {
            return PasswordVerificationResult.Failed;
        }

        var parts = passwordHash.Split('$');

        if (parts.Length != 5
            || !string.Equals(parts[0], Algorithm, StringComparison.Ordinal)
            || !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var version)
            || version != FormatVersion
            || !int.TryParse(parts[2], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations)
            || iterations <= 0)
        {
            return PasswordVerificationResult.Failed;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[3]);
            var expectedHash = Convert.FromBase64String(parts[4]);

            if (salt.Length < 16 || expectedHash.Length < 32)
            {
                return PasswordVerificationResult.Failed;
            }

            var actualHash = DeriveHash(password, salt, iterations, expectedHash.Length);

            if (!CryptographicOperations.FixedTimeEquals(actualHash, expectedHash))
            {
                return PasswordVerificationResult.Failed;
            }

            var needsRehash = iterations < _settings.Iterations
                || salt.Length != _settings.SaltSize
                || expectedHash.Length != _settings.HashSize;

            return needsRehash
                ? PasswordVerificationResult.SuccessRehashNeeded
                : PasswordVerificationResult.Success;
        }
        catch (FormatException)
        {
            return PasswordVerificationResult.Failed;
        }
        catch (ArgumentException)
        {
            return PasswordVerificationResult.Failed;
        }
    }

    private static byte[] DeriveHash(
        string password,
        byte[] salt,
        int iterations,
        int hashSize)
    {
        return Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA512,
            hashSize);
    }
}
