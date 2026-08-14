using EnterpriseSmartHrm.Application.Common.Models;
using MediatR;

namespace EnterpriseSmartHrm.Application.Features.Authentication.Login;

public sealed record LoginCommand : IRequest<Result<AuthenticationResponse>>
{
    public string UsernameOrEmail { get; init; } = string.Empty;

    public string Password { get; init; } = string.Empty;

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }
}
