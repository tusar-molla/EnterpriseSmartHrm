using Asp.Versioning;
using EnterpriseSmartHrm.Application.Features.Authentication.Login;
using EnterpriseSmartHrm.Application.Features.Authentication.Logout;
using EnterpriseSmartHrm.Application.Features.Authentication.RefreshAccessToken;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseSmartHrm.Api.Controllers;

[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class AuthController : BaseApiController
{
    private readonly ISender _sender;

    public AuthController(ISender sender)
    {
        _sender = sender;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginCommand
        {
            UsernameOrEmail = request.UsernameOrEmail,
            Password = request.Password,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent = Request.Headers.UserAgent.ToString()
        };

        var result = await _sender.Send(command, cancellationToken);

        return FromResult(result);
    }

    // Anonymous on purpose: the caller's access token has usually expired by the time they
    // get here. The refresh token itself is the credential.
    [AllowAnonymous]
    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RefreshAccessTokenCommand
        {
            RefreshToken = request.RefreshToken,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        var result = await _sender.Send(command, cancellationToken);

        return FromResult(result);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var command = new LogoutCommand
        {
            RefreshToken = request.RefreshToken,
            IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString()
        };

        var result = await _sender.Send(command, cancellationToken);

        return FromResult(result);
    }
}

public sealed record LoginRequest(string UsernameOrEmail, string Password);

public sealed record RefreshTokenRequest(string RefreshToken);

public sealed record LogoutRequest(string RefreshToken);
