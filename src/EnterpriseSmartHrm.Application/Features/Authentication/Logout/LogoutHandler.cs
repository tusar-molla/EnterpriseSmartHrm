using EnterpriseSmartHrm.Application.Common.Interfaces;
using EnterpriseSmartHrm.Application.Common.Models;
using EnterpriseSmartHrm.Application.Features.Authentication.Interfaces;
using MediatR;

namespace EnterpriseSmartHrm.Application.Features.Authentication.Logout;

public sealed class LogoutHandler : IRequestHandler<LogoutCommand, Result>
{
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LogoutHandler(
        IRefreshTokenRepository refreshTokenRepository,
        ITokenService tokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _refreshTokenRepository = refreshTokenRepository;
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _dateTimeProvider.UtcNow;
        var tokenHash = _tokenService.HashRefreshToken(request.RefreshToken);

        var storedToken = await _refreshTokenRepository.GetByTokenHashAsync(tokenHash, cancellationToken);

        // Reports success even for an unknown or already-revoked token, so this endpoint cannot
        // be used to probe which refresh tokens exist.
        if (storedToken is not null && storedToken.IsActive(utcNow))
        {
            storedToken.Revoke(utcNow, request.IpAddress, "User logged out.");

            await _refreshTokenRepository.UpdateAsync(storedToken, cancellationToken);
        }

        return Result.Success("Logged out successfully.");
    }
}
