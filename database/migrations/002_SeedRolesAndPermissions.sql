USE [EnterpriseSmartHrmDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF EXISTS (
    SELECT 1
    FROM dbo.DatabaseMigrations
    WHERE MigrationId = N'002_SeedRolesAndPermissions')
BEGIN
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @Roles TABLE
    (
        Name NVARCHAR(100) NOT NULL,
        NormalizedName NVARCHAR(100) NOT NULL,
        Description NVARCHAR(500) NOT NULL
    );

    INSERT INTO @Roles (Name, NormalizedName, Description)
    VALUES
        (N'Admin', N'ADMIN', N'Full system administration access.'),
        (N'HR', N'HR', N'Human resources operational access.'),
        (N'Manager', N'MANAGER', N'Team management and approval access.'),
        (N'Employee', N'EMPLOYEE', N'Employee self-service access.');

    UPDATE target
    SET target.Name = source.Name,
        target.Description = source.Description,
        target.IsSystemRole = 1,
        target.IsActive = 1,
        target.IsDeleted = 0,
        target.DeletedAtUtc = NULL,
        target.DeletedBy = NULL,
        target.UpdatedAtUtc = SYSUTCDATETIME()
    FROM auth.Roles AS target
    INNER JOIN @Roles AS source
        ON source.NormalizedName = target.NormalizedName;

    INSERT INTO auth.Roles (Name, NormalizedName, Description, IsSystemRole)
    SELECT source.Name, source.NormalizedName, source.Description, 1
    FROM @Roles AS source
    WHERE NOT EXISTS (
        SELECT 1
        FROM auth.Roles AS target
        WHERE target.NormalizedName = source.NormalizedName);

    DECLARE @Permissions TABLE
    (
        ModuleName NVARCHAR(100) NOT NULL,
        PermissionKey NVARCHAR(150) NOT NULL,
        DisplayName NVARCHAR(150) NOT NULL
    );

    INSERT INTO @Permissions (ModuleName, PermissionKey, DisplayName)
    VALUES
        (N'Users', N'User.View', N'View users'),
        (N'Users', N'User.Create', N'Create users'),
        (N'Users', N'User.Update', N'Update users'),
        (N'Users', N'User.Activate', N'Activate users'),
        (N'Users', N'User.Deactivate', N'Deactivate users'),
        (N'Users', N'User.AssignRole', N'Assign user roles'),
        (N'Roles', N'Role.View', N'View roles'),
        (N'Roles', N'Role.Create', N'Create roles'),
        (N'Roles', N'Role.Update', N'Update roles'),
        (N'Permissions', N'Permission.View', N'View permissions'),
        (N'Permissions', N'Permission.Assign', N'Assign permissions'),
        (N'Employees', N'Employee.View', N'View employees'),
        (N'Employees', N'Employee.ViewSelf', N'View own employee profile'),
        (N'Employees', N'Employee.ViewTeam', N'View team employees'),
        (N'Employees', N'Employee.Create', N'Create employees'),
        (N'Employees', N'Employee.Update', N'Update employees'),
        (N'Employees', N'Employee.Delete', N'Delete employees'),
        (N'Employees', N'Employee.UpdateStatus', N'Update employee status'),
        (N'Employees', N'Employee.UploadDocument', N'Upload employee documents'),
        (N'Organization', N'Department.View', N'View departments'),
        (N'Organization', N'Department.Manage', N'Manage departments'),
        (N'Organization', N'Designation.View', N'View designations'),
        (N'Organization', N'Designation.Manage', N'Manage designations'),
        (N'Organization', N'Location.View', N'View locations'),
        (N'Organization', N'Location.Manage', N'Manage locations'),
        (N'Organization', N'Shift.View', N'View shifts'),
        (N'Organization', N'Shift.Manage', N'Manage shifts'),
        (N'Organization', N'Holiday.View', N'View holidays'),
        (N'Organization', N'Holiday.Manage', N'Manage holidays'),
        (N'Organization', N'Setting.View', N'View settings'),
        (N'Organization', N'Setting.Update', N'Update settings'),
        (N'Attendance', N'Attendance.View', N'View attendance'),
        (N'Attendance', N'Attendance.ViewSelf', N'View own attendance'),
        (N'Attendance', N'Attendance.ViewTeam', N'View team attendance'),
        (N'Attendance', N'Attendance.CheckIn', N'Check in'),
        (N'Attendance', N'Attendance.CheckOut', N'Check out'),
        (N'Attendance', N'Attendance.ManualUpdate', N'Manually update attendance'),
        (N'Attendance', N'Attendance.CorrectionRequest', N'Request attendance correction'),
        (N'Attendance', N'Attendance.ApproveCorrection', N'Approve attendance correction'),
        (N'Leave', N'Leave.View', N'View leave requests'),
        (N'Leave', N'Leave.ViewSelf', N'View own leave requests'),
        (N'Leave', N'Leave.ViewTeam', N'View team leave requests'),
        (N'Leave', N'Leave.Apply', N'Apply for leave'),
        (N'Leave', N'Leave.Approve', N'Approve leave'),
        (N'Leave', N'Leave.Reject', N'Reject leave'),
        (N'Leave', N'Leave.Cancel', N'Cancel leave'),
        (N'Leave', N'LeaveType.Manage', N'Manage leave types'),
        (N'Leave', N'LeaveBalance.View', N'View leave balances'),
        (N'Payroll', N'Payroll.View', N'View payroll'),
        (N'Payroll', N'Payroll.Generate', N'Generate payroll'),
        (N'Payroll', N'Payroll.Finalize', N'Finalize payroll'),
        (N'Payroll', N'Payroll.Pay', N'Mark payroll as paid'),
        (N'Payroll', N'Payroll.ViewPayslip', N'View payslips'),
        (N'Payroll', N'Payroll.DownloadPayslip', N'Download payslips'),
        (N'Payroll', N'SalaryComponent.Manage', N'Manage salary components'),
        (N'Payroll', N'SalaryStructure.Assign', N'Assign salary structures'),
        (N'Reports', N'Report.Employee', N'View employee reports'),
        (N'Reports', N'Report.Attendance', N'View attendance reports'),
        (N'Reports', N'Report.Leave', N'View leave reports'),
        (N'Reports', N'Report.Payroll', N'View payroll reports'),
        (N'Reports', N'Report.Export', N'Export reports'),
        (N'Notifications', N'Notification.View', N'View notifications'),
        (N'Notifications', N'Notification.Manage', N'Manage notifications'),
        (N'AuditLogs', N'AuditLog.View', N'View audit logs');

    UPDATE target
    SET target.ModuleName = source.ModuleName,
        target.DisplayName = source.DisplayName,
        target.IsActive = 1,
        target.UpdatedAtUtc = SYSUTCDATETIME()
    FROM auth.Permissions AS target
    INNER JOIN @Permissions AS source
        ON source.PermissionKey = target.PermissionKey;

    INSERT INTO auth.Permissions (ModuleName, PermissionKey, DisplayName)
    SELECT source.ModuleName, source.PermissionKey, source.DisplayName
    FROM @Permissions AS source
    WHERE NOT EXISTS (
        SELECT 1
        FROM auth.Permissions AS target
        WHERE target.PermissionKey = source.PermissionKey);

    INSERT INTO dbo.DatabaseMigrations (MigrationId, Description)
    VALUES (N'002_SeedRolesAndPermissions', N'Seed system roles and permission catalog.');

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
    BEGIN
        ROLLBACK TRANSACTION;
    END;

    THROW;
END CATCH;
GO
