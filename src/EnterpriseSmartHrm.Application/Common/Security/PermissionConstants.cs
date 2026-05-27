namespace EnterpriseSmartHrm.Application.Common.Security;

public static class PermissionConstants
{
    public static class Users
    {
        public const string View = "User.View";
        public const string Create = "User.Create";
        public const string Update = "User.Update";
        public const string Activate = "User.Activate";
        public const string Deactivate = "User.Deactivate";
        public const string AssignRole = "User.AssignRole";

        public static readonly string[] All =
        [
            View,
            Create,
            Update,
            Activate,
            Deactivate,
            AssignRole
        ];
    }

    public static class Roles
    {
        public const string View = "Role.View";
        public const string Create = "Role.Create";
        public const string Update = "Role.Update";

        public static readonly string[] All =
        [
            View,
            Create,
            Update
        ];
    }

    public static class Permissions
    {
        public const string View = "Permission.View";
        public const string Assign = "Permission.Assign";

        public static readonly string[] All =
        [
            View,
            Assign
        ];
    }

    public static class Employees
    {
        public const string View = "Employee.View";
        public const string ViewSelf = "Employee.ViewSelf";
        public const string ViewTeam = "Employee.ViewTeam";
        public const string Create = "Employee.Create";
        public const string Update = "Employee.Update";
        public const string Delete = "Employee.Delete";
        public const string UpdateStatus = "Employee.UpdateStatus";
        public const string UploadDocument = "Employee.UploadDocument";

        public static readonly string[] All =
        [
            View,
            ViewSelf,
            ViewTeam,
            Create,
            Update,
            Delete,
            UpdateStatus,
            UploadDocument
        ];
    }

    public static class Organization
    {
        public const string DepartmentView = "Department.View";
        public const string DepartmentManage = "Department.Manage";
        public const string DesignationView = "Designation.View";
        public const string DesignationManage = "Designation.Manage";
        public const string LocationView = "Location.View";
        public const string LocationManage = "Location.Manage";
        public const string ShiftView = "Shift.View";
        public const string ShiftManage = "Shift.Manage";
        public const string HolidayView = "Holiday.View";
        public const string HolidayManage = "Holiday.Manage";
        public const string SettingView = "Setting.View";
        public const string SettingUpdate = "Setting.Update";

        public static readonly string[] All =
        [
            DepartmentView,
            DepartmentManage,
            DesignationView,
            DesignationManage,
            LocationView,
            LocationManage,
            ShiftView,
            ShiftManage,
            HolidayView,
            HolidayManage,
            SettingView,
            SettingUpdate
        ];
    }

    public static class Attendance
    {
        public const string View = "Attendance.View";
        public const string ViewSelf = "Attendance.ViewSelf";
        public const string ViewTeam = "Attendance.ViewTeam";
        public const string CheckIn = "Attendance.CheckIn";
        public const string CheckOut = "Attendance.CheckOut";
        public const string ManualUpdate = "Attendance.ManualUpdate";
        public const string CorrectionRequest = "Attendance.CorrectionRequest";
        public const string ApproveCorrection = "Attendance.ApproveCorrection";

        public static readonly string[] All =
        [
            View,
            ViewSelf,
            ViewTeam,
            CheckIn,
            CheckOut,
            ManualUpdate,
            CorrectionRequest,
            ApproveCorrection
        ];
    }

    public static class Leave
    {
        public const string View = "Leave.View";
        public const string ViewSelf = "Leave.ViewSelf";
        public const string ViewTeam = "Leave.ViewTeam";
        public const string Apply = "Leave.Apply";
        public const string Approve = "Leave.Approve";
        public const string Reject = "Leave.Reject";
        public const string Cancel = "Leave.Cancel";
        public const string LeaveTypeManage = "LeaveType.Manage";
        public const string LeaveBalanceView = "LeaveBalance.View";

        public static readonly string[] All =
        [
            View,
            ViewSelf,
            ViewTeam,
            Apply,
            Approve,
            Reject,
            Cancel,
            LeaveTypeManage,
            LeaveBalanceView
        ];
    }

    public static class Payroll
    {
        public const string View = "Payroll.View";
        public const string Generate = "Payroll.Generate";
        public const string Finalize = "Payroll.Finalize";
        public const string Pay = "Payroll.Pay";
        public const string ViewPayslip = "Payroll.ViewPayslip";
        public const string DownloadPayslip = "Payroll.DownloadPayslip";
        public const string SalaryComponentManage = "SalaryComponent.Manage";
        public const string SalaryStructureAssign = "SalaryStructure.Assign";

        public static readonly string[] All =
        [
            View,
            Generate,
            Finalize,
            Pay,
            ViewPayslip,
            DownloadPayslip,
            SalaryComponentManage,
            SalaryStructureAssign
        ];
    }

    public static class Reports
    {
        public const string Employee = "Report.Employee";
        public const string Attendance = "Report.Attendance";
        public const string Leave = "Report.Leave";
        public const string Payroll = "Report.Payroll";
        public const string Export = "Report.Export";

        public static readonly string[] All =
        [
            Employee,
            Attendance,
            Leave,
            Payroll,
            Export
        ];
    }

    public static class Notifications
    {
        public const string View = "Notification.View";
        public const string Manage = "Notification.Manage";

        public static readonly string[] All =
        [
            View,
            Manage
        ];
    }

    public static class AuditLogs
    {
        public const string View = "AuditLog.View";

        public static readonly string[] All =
        [
            View
        ];
    }

    public static readonly string[] All =
    [
        ..Users.All,
        ..Roles.All,
        ..Permissions.All,
        ..Employees.All,
        ..Organization.All,
        ..Attendance.All,
        ..Leave.All,
        ..Payroll.All,
        ..Reports.All,
        ..Notifications.All,
        ..AuditLogs.All
    ];
}
