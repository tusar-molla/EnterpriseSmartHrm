using EnterpriseSmartHrm.Application.Common.Abstractions;
using EnterpriseSmartHrm.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;

namespace EnterpriseSmartHrm.Api.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly ICurrentUserService _currentUser;

    public PermissionAuthorizationHandler(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (!_currentUser.IsAuthenticated)
        {
            return Task.CompletedTask;
        }

        if (_currentUser.IsInRole(RoleConstants.Admin) ||
            _currentUser.HasPermission(requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
