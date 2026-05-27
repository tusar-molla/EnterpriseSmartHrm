namespace EnterpriseSmartHrm.Application.Common.Security;

public static class RoleConstants
{
    public const string Admin = "Admin";

    public const string Hr = "HR";

    public const string Manager = "Manager";

    public const string Employee = "Employee";

    public static readonly string[] All =
    [
        Admin,
        Hr,
        Manager,
        Employee
    ];
}
