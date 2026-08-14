using EnterpriseSmartHrm.Application.Common.Interfaces;
using EnterpriseSmartHrm.Application.Common.Models;
using EnterpriseSmartHrm.Application.Features.Authentication.Interfaces;
using EnterpriseSmartHrm.Domain.Authentication;
using MediatR;

namespace EnterpriseSmartHrm.Application.Features.Authentication.Login;

public sealed class LoginHandler : IRequestHandler<LoginCommand, Result<AuthenticationResponse>>
{
    private const int MaximumFailedAttempts = 5;
    private static readonly TimeSpan LockoutDuration = TimeSpan.FromMinutes(15);
    private const string InvalidCredentialsMessage = "Invalid username/email or password.";

    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly ILoginHistoryRepository _loginHistoryRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LoginHandler(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        ILoginHistoryRepository loginHistoryRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IDateTimeProvider dateTimeProvider)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _loginHistoryRepository = loginHistoryRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<Result<AuthenticationResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var utcNow = _dateTimeProvider.UtcNow;
        var normalized = request.UsernameOrEmail.Trim().ToUpperInvariant();

        var user = await _userRepository.GetByNormalizedUsernameOrEmailAsync(normalized, cancellationToken);

        if (user is null)
        {
            await RecordLoginAsync(request, null, false, "User not found.", utcNow, cancellationToken);
            return Result<AuthenticationResponse>.Unauthorized(InvalidCredentialsMessage);
        }

        if (user.IsLockedOut(utcNow))
        {
            await RecordLoginAsync(request, user.Id, false, "Account locked out.", utcNow, cancellationToken);
            return Result<AuthenticationResponse>.Forbidden(
                "Account is temporarily locked due to multiple failed login attempts. Please try again later.");
        }

        if (!user.IsActive)
        {
            await RecordLoginAsync(request, user.Id, false, "Account inactive.", utcNow, cancellationToken);
            return Result<AuthenticationResponse>.Forbidden("Account is inactive. Please contact your administrator.");
        }

        var verification = _passwordHasher.Verify(request.Password, user.PasswordHash);

        if (verification == PasswordVerificationResult.Failed)
        {
            user.RecordFailedLogin(utcNow, MaximumFailedAttempts, LockoutDuration);
            await _userRepository.UpdateAsync(user, cancellationToken);
            await RecordLoginAsync(request, user.Id, false, "Invalid password.", utcNow, cancellationToken);
            return Result<AuthenticationResponse>.Unauthorized(InvalidCredentialsMessage);
        }

        if (verification == PasswordVerificationResult.SuccessRehashNeeded)
        {
            user.PasswordHash = _passwordHasher.Hash(request.Password);
        }

        var roles = await _userRepository.GetRoleNamesAsync(user.Id, cancellationToken);
        var permissions = await _userRepository.GetPermissionKeysAsync(user.Id, cancellationToken);

        var accessToken = _tokenService.GenerateAccessToken(user, roles, permissions);
        var refreshToken = _tokenService.GenerateRefreshToken();

        await _refreshTokenRepository.CreateAsync(
            new RefreshToken
            {
                UserId = user.Id,
                TokenHash = refreshToken.Hash,
                ExpiresAtUtc = refreshToken.ExpiresAtUtc,
                CreatedAtUtc = utcNow,
                CreatedByIp = request.IpAddress
            },
            cancellationToken);

        user.RecordSuccessfulLogin(utcNow);
        await _userRepository.UpdateAsync(user, cancellationToken);
        await RecordLoginAsync(request, user.Id, true, null, utcNow, cancellationToken);

        var response = new AuthenticationResponse
        {
            AccessToken = accessToken.Value,
            AccessTokenExpiresAtUtc = accessToken.ExpiresAtUtc,
            RefreshToken = refreshToken.Value,
            RefreshTokenExpiresAtUtc = refreshToken.ExpiresAtUtc,
            UserId = user.Id,
            Username = user.Username,
            Email = user.Email,
            Roles = roles,
            Permissions = permissions
        };

        return Result<AuthenticationResponse>.Success(response, "Login successful.");
    }

    private Task RecordLoginAsync(
        LoginCommand request,
        int? userId,
        bool isSuccessful,
        string? failureReason,
        DateTime utcNow,
        CancellationToken cancellationToken)
    {
        return _loginHistoryRepository.CreateAsync(
            new LoginHistory
            {
                UserId = userId,
                UsernameOrEmail = request.UsernameOrEmail,
                IsSuccessful = isSuccessful,
                FailureReason = failureReason,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
                OccurredAtUtc = utcNow
            },
            cancellationToken);
    }
}
