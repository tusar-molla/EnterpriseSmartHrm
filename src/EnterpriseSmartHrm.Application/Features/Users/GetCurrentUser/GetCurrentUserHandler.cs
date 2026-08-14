using EnterpriseSmartHrm.Application.Common.Interfaces;
using EnterpriseSmartHrm.Application.Common.Models;
using MediatR;

namespace EnterpriseSmartHrm.Application.Features.Users.GetCurrentUser;

public sealed class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, Result<CurrentUserResponse>>
{
    private readonly ICurrentUserService _currentUser;

    public GetCurrentUserHandler(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public Task<Result<CurrentUserResponse>> Handle(
        GetCurrentUserQuery request,
        CancellationToken cancellationToken)
    {
        if (_currentUser.UserId is not int userId)
        {
            return Task.FromResult(
                Result<CurrentUserResponse>.Unauthorized("Authenticated user could not be resolved."));
        }

        var response = new CurrentUserResponse
        {
            UserId = userId,
            EmployeeId = _currentUser.EmployeeId,
            Email = _currentUser.Email,
            Roles = _currentUser.Roles,
            Permissions = _currentUser.Permissions
        };

        return Task.FromResult(Result<CurrentUserResponse>.Success(response));
    }
}
