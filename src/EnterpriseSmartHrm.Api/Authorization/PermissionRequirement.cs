using Microsoft.AspNetCore.Authorization;

namespace EnterpriseSmartHrm.Api.Authorization;

public sealed record PermissionRequirement(string Permission) : IAuthorizationRequirement;
