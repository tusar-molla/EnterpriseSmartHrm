namespace EnterpriseSmartHrm.Application.Common.Interfaces;

public interface ICurrentUserService
{
    bool IsAuthenticated { get; }

    int? UserId { get; }

    int? EmployeeId { get; }

    string? Email { get; }

    IReadOnlyCollection<string> Roles { get; }

    IReadOnlyCollection<string> Permissions { get; }

    bool IsInRole(string role);

    bool HasPermission(string permission);
}
