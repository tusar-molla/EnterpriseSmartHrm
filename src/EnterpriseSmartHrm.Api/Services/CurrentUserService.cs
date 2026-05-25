using EnterpriseSmartHrm.Application.Common.Abstractions;
using EnterpriseSmartHrm.Application.Common.Security;
using System.Security.Claims;

namespace EnterpriseSmartHrm.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public int? UserId =>
        GetIntClaim(ClaimTypes.NameIdentifier) ??
        GetIntClaim(ClaimConstants.UserId);

    public int? EmployeeId => GetIntClaim(ClaimConstants.EmployeeId);

    public string? Email =>
        GetClaimValue(ClaimTypes.Email) ??
        GetClaimValue(ClaimConstants.Email);

    public IReadOnlyCollection<string> Roles =>
        GetClaimValues(ClaimTypes.Role)
            .Concat(GetClaimValues("role"))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public IReadOnlyCollection<string> Permissions =>
        GetClaimValues(ClaimConstants.Permission)
            .Concat(GetClaimValues("permissions"))
            .SelectMany(SplitClaimValue)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

    public bool IsInRole(string role)
    {
        return !string.IsNullOrWhiteSpace(role) &&
               Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
    }

    public bool HasPermission(string permission)
    {
        return !string.IsNullOrWhiteSpace(permission) &&
               Permissions.Contains(permission, StringComparer.OrdinalIgnoreCase);
    }

    private string? GetClaimValue(string claimType)
    {
        return User?.FindFirst(claimType)?.Value;
    }

    private IEnumerable<string> GetClaimValues(string claimType)
    {
        return User?.FindAll(claimType).Select(claim => claim.Value) ??
               Enumerable.Empty<string>();
    }

    private int? GetIntClaim(string claimType)
    {
        var value = GetClaimValue(claimType);

        return int.TryParse(value, out var parsedValue)
            ? parsedValue
            : null;
    }

    private static IEnumerable<string> SplitClaimValue(string claimValue)
    {
        return claimValue
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
