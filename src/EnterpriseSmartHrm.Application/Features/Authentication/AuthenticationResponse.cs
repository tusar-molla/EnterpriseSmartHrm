namespace EnterpriseSmartHrm.Application.Features.Authentication;

// Shared by Login and RefreshAccessToken: both hand the client the same token pair,
// so the shape is declared once at the module root instead of per slice.
public sealed record AuthenticationResponse
{
    public string AccessToken { get; init; } = string.Empty;

    public DateTime AccessTokenExpiresAtUtc { get; init; }

    public string RefreshToken { get; init; } = string.Empty;

    public DateTime RefreshTokenExpiresAtUtc { get; init; }

    public int UserId { get; init; }

    public string Username { get; init; } = string.Empty;

    public string Email { get; init; } = string.Empty;

    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();
}
