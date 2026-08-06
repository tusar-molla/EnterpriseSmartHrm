using Asp.Versioning;
using EnterpriseSmartHrm.Api.Controllers.Common;
using EnterpriseSmartHrm.Application.Features.Authentication.Login;
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
    public async Task<IActionResult> Login([FromBody] LoginRequest request,CancellationToken cancellationToken)
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
}

public sealed record LoginRequest(string UsernameOrEmail, string Password);
