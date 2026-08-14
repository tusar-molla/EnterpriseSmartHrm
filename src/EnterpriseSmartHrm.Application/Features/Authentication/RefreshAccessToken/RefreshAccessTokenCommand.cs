using EnterpriseSmartHrm.Application.Common.Models;
using MediatR;

namespace EnterpriseSmartHrm.Application.Features.Authentication.RefreshAccessToken;

public sealed record RefreshAccessTokenCommand : IRequest<Result<AuthenticationResponse>>
{
    public string RefreshToken { get; init; } = string.Empty;

    public string? IpAddress { get; init; }
}
