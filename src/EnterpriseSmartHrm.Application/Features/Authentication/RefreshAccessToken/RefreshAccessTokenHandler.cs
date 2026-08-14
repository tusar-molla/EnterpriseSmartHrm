using EnterpriseSmartHrm.Application.Common.Interfaces;
using EnterpriseSmartHrm.Application.Common.Models;
using EnterpriseSmartHrm.Application.Features.Authentication.Interfaces;
using EnterpriseSmartHrm.Domain.Authentication;
using MediatR;

namespace EnterpriseSmartHrm.Application.Features.Authentication.RefreshAccessToken;

public sealed class RefreshAccessTokenHandler
    : IRequestHandler<RefreshAccessTokenCommand, Result<AuthenticationResponse>>
{
    private const string InvalidRefreshTokenMessage = "Refresh token is invalid or has expired.";

    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RefreshAccessTokenHandler(
        IRefreshTokenRepository refreshTokenRepository,
        IUserRepository userRepository,
        ITokenService tokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _userRepository = userRepository;
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<AuthenticationResponse>> Handle(
        RefreshAccessTokenCommand request,
        CancellationToken cancellationToken)
    {
        var utcNow = _dateTimeProvider.UtcNow;
        var incomingHash = _tokenService.HashRefreshToken(request.RefreshToken);

        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(incomingHash, cancellationToken);

        if (storedToken is null)
        {
            return Result<AuthenticationResponse>.Unauthorized(InvalidRefreshTokenMessage);
        }

        // An already-revoked token is being replayed. Rotation means the legitimate client would
        // never send this, so treat it as stolen and revoke the user's whole active chain.
        if (storedToken.IsRevoked())
        {
            await _refreshTokenRepository.RevokeAllActiveForUserAsync(
                storedToken.UserId,
                utcNow,
                request.IpAddress,
                "A revoked refresh token was reused.",
                cancellationToken);

            return Result<AuthenticationResponse>.Unauthorized(InvalidRefreshTokenMessage);
        }

        if (storedToken.IsExpired(utcNow))
        {
            return Result<AuthenticationResponse>.Unauthorized(InvalidRefreshTokenMessage);
        }

        var user = await _userRepository.GetByIdAsync(storedToken.UserId, cancellationToken);

        if (user is null)
        {
            return Result<AuthenticationResponse>.Unauthorized(InvalidRefreshTokenMessage);
        }

        if (user.IsLockedOut(utcNow))
        {
            return Result<AuthenticationResponse>.Forbidden(
                "Account is temporarily locked due to multiple failed login attempts. Please try again later.");
        }

        if (!user.IsActive)
        {
            return Result<AuthenticationResponse>.Forbidden(
                "Account is inactive. Please contact your administrator.");
        }

        // Re-read from the database so a role or permission change takes effect on the next
        // refresh, rather than persisting until the user logs in again.
        var roles = await _userRepository.GetRoleNamesAsync(user.Id, cancellationToken);
        var permissions = await _userRepository.GetPermissionKeysAsync(user.Id, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var rotatedToken = _tokenService.GenerateRefreshToken();

        storedToken.Revoke(
            utcNow,
            request.IpAddress,
            "Replaced by a rotated refresh token.",
            rotatedToken.Hash);

        await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);

        await _refreshTokenRepository.CreateAsync(
            new RefreshToken
            {
                UserId = user.Id,
                TokenHash = rotatedToken.Hash,
                ExpiresAtUtc = rotatedToken.ExpiresAtUtc,
                CreatedAtUtc = utcNow,
                CreatedByIp = request.IpAddress
            },
            cancellationToken);

        var response = new AuthenticationResponse
        {
            AccessToken = accessToken.Value,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = rotatedToken.Value,
            RefreshTokenExpiresAtUtc = rotatedToken.ExpiresAtUtc,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Roles = roles,
            Permissions = permissions
        };

        return Result<AuthenticationResponse>.Success(response, "Token refreshed successfully.");
    }
}
