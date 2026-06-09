USE [EnterpriseSmartHrmDb];
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
SET ANSI_NULLS ON;
SET QUOTED_IDENTIFIER ON;

IF EXISTS (
    SELECT 1
    FROM dbo.DatabaseMigrations
    WHERE MigrationId = N'003_SeedRolePermissions')
BEGIN
    RETURN;
END;

BEGIN TRY
    BEGIN TRANSACTION;

    DECLARE @RolePermissionKeys TABLE
    (
        RoleName NVARCHAR(100) NOT NULL,
        PermissionKey NVARCHAR(150) NOT NULL,
        PRIMARY KEY (RoleName, PermissionKey)
    );

    INSERT INTO @RolePermissionKeys (RoleName, PermissionKey)
    SELECT N'Admin', PermissionKey
    FROM auth.Permissions
    WHERE IsActive = 1;

    INSERT INTO @RolePermissionKeys (RoleName, PermissionKey)
    VALUES
        (N'HR', N'User.View'),
        (N'HR', N'User.Create'),
        (N'HR', N'User.Update'),
        (N'HR', N'User.Activate'),
        (N'HR', N'User.Deactivate'),
        (N'HR', N'Employee.View'),
        (N'HR', N'Employee.ViewSelf'),
        (N'HR', N'Employee.ViewTeam'),
        (N'HR', N'Employee.Create'),
        (N'HR', N'Employee.Update'),
        (N'HR', N'Employee.Delete'),
        (N'HR', N'Employee.UpdateStatus'),
        (N'HR', N'Employee.UploadDocument'),
        (N'HR', N'Department.View'),
        (N'HR', N'Department.Manage'),
        (N'HR', N'Designation.View'),
        (N'HR', N'Designation.Manage'),
        (N'HR', N'Location.View'),
        (N'HR', N'Location.Manage'),
        (N'HR', N'Shift.View'),
        (N'HR', N'Shift.Manage'),
        (N'HR', N'Holiday.View'),
        (N'HR', N'Holiday.Manage'),
        (N'HR', N'Setting.View'),
        (N'HR', N'Setting.Update'),
        (N'HR', N'Attendance.View'),
        (N'HR', N'Attendance.ViewSelf'),
        (N'HR', N'Attendance.ViewTeam'),
        (N'HR', N'Attendance.CheckIn'),
        (N'HR', N'Attendance.CheckOut'),
        (N'HR', N'Attendance.ManualUpdate'),
        (N'HR', N'Attendance.CorrectionRequest'),
        (N'HR', N'Attendance.ApproveCorrection'),
        (N'HR', N'Leave.View'),
        (N'HR', N'Leave.ViewSelf'),
        (N'HR', N'Leave.ViewTeam'),
        (N'HR', N'Leave.Apply'),
        (N'HR', N'Leave.Approve'),
        (N'HR', N'Leave.Reject'),
        (N'HR', N'Leave.Cancel'),
        (N'HR', N'LeaveType.Manage'),
        (N'HR', N'LeaveBalance.View'),
        (N'HR', N'Payroll.View'),
        (N'HR', N'Payroll.Generate'),
        (N'HR', N'Payroll.Finalize'),
        (N'HR', N'Payroll.Pay'),
        (N'HR', N'Payroll.ViewPayslip'),
        (N'HR', N'Payroll.DownloadPayslip'),
        (N'HR', N'SalaryComponent.Manage'),
        (N'HR', N'SalaryStructure.Assign'),
        (N'HR', N'Report.Employee'),
        (N'HR', N'Report.Attendance'),
        (N'HR', N'Report.Leave'),
        (N'HR', N'Report.Payroll'),
        (N'HR', N'Report.Export'),
        (N'HR', N'Notification.View'),
        (N'HR', N'Notification.Manage'),
        (N'HR', N'AuditLog.View'),
        (N'Manager', N'Employee.ViewTeam'),
        (N'Manager', N'Department.View'),
        (N'Manager', N'Designation.View'),
        (N'Manager', N'Location.View'),
        (N'Manager', N'Shift.View'),
        (N'Manager', N'Holiday.View'),
        (N'Manager', N'Attendance.ViewTeam'),
        (N'Manager', N'Attendance.ApproveCorrection'),
        (N'Manager', N'Leave.ViewTeam'),
        (N'Manager', N'Leave.Approve'),
        (N'Manager', N'Leave.Reject'),
        (N'Manager', N'Report.Employee'),
        (N'Manager', N'Report.Attendance'),
        (N'Manager', N'Report.Leave'),
        (N'Manager', N'Notification.View'),
        (N'Employee', N'Employee.ViewSelf'),
        (N'Employee', N'Shift.View'),
        (N'Employee', N'Holiday.View'),
        (N'Employee', N'Attendance.ViewSelf'),
        (N'Employee', N'Attendance.CheckIn'),
        (N'Employee', N'Attendance.CheckOut'),
        (N'Employee', N'Attendance.CorrectionRequest'),
        (N'Employee', N'Leave.ViewSelf'),
        (N'Employee', N'Leave.Apply'),
        (N'Employee', N'Leave.Cancel'),
        (N'Employee', N'LeaveBalance.View'),
        (N'Employee', N'Payroll.ViewPayslip'),
        (N'Employee', N'Payroll.DownloadPayslip'),
        (N'Employee', N'Notification.View');

    UPDATE target
    SET target.IsAllowed = 1,
        target.UpdatedAtUtc = SYSUTCDATETIME()
    FROM auth.RolePermissions AS target
    INNER JOIN auth.Roles AS role
        ON role.Id = target.RoleId
    INNER JOIN auth.Permissions AS permission
        ON permission.Id = target.PermissionId
    INNER JOIN @RolePermissionKeys AS source
        ON source.RoleName = role.Name
       AND source.PermissionKey = permission.PermissionKey;

    INSERT INTO auth.RolePermissions (RoleId, PermissionId, IsAllowed)
    SELECT role.Id, permission.Id, 1
    FROM @RolePermissionKeys AS source
    INNER JOIN auth.Roles AS role
        ON role.Name = source.RoleName
       AND role.IsDeleted = 0
    INNER JOIN auth.Permissions AS permission
        ON permission.PermissionKey = source.PermissionKey
       AND permission.IsActive = 1
    WHERE NOT EXISTS (
        SELECT 1
        FROM auth.RolePermissions AS target
        WHERE target.RoleId = role.Id
          AND target.PermissionId = permission.Id);

    INSERT INTO dbo.DatabaseMigrations (MigrationId, Description)
    VALUES (N'003_SeedRolePermissions', N'Seed initial role-permission access matrix.');

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
