using EnterpriseSmartHrm.Application.Common.Models;
using MediatR;

namespace EnterpriseSmartHrm.Application.Features.Users.GetCurrentUser;

public sealed record GetCurrentUserQuery : IRequest<Result<CurrentUserResponse>>;

public sealed record CurrentUserResponse
{
    public int UserId { get; init; }

    public int? EmployeeId { get; init; }

    public string? Email { get; init; }

    public IReadOnlyCollection<string> Roles { get; init; } = Array.Empty<string>();

    public IReadOnlyCollection<string> Permissions { get; init; } = Array.Empty<string>();
}
