using Asp.Versioning;
using EnterpriseSmartHrm.Api.Authorization;
using EnterpriseSmartHrm.Application.Common.Security;
using EnterpriseSmartHrm.Application.Features.Users.GetCurrentUser;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseSmartHrm.Api.Controllers;

[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/[controller]")]
public sealed class UsersController : BaseApiController
{
    private readonly ISender _sender;

    public UsersController(ISender sender)
    {
        _sender = sender;
    }

    [HasPermission(PermissionConstants.Users.View)]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser(CancellationToken cancellationToken)
    {
        var result = await _sender.Send(new GetCurrentUserQuery(), cancellationToken);

        return FromResult(result);
    }
}
