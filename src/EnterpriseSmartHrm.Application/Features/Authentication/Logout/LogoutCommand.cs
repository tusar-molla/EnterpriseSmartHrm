using EnterpriseSmartHrm.Application.Common.Models;
using MediatR;

namespace EnterpriseSmartHrm.Application.Features.Authentication.Logout;

public sealed record LogoutCommand : IRequest<Result>
{
    public string RefreshToken { get; init; } = string.Empty;

    public string? IpAddress { get; init; }
}
