using System.Threading.Tasks;
using smart_attendance_middleware;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;

public static class ServiceManager  
{
    public static LogSyncService LogSyncService { get; private set; }
    public static AttendanceLogSyncService AttendanceLogSyncService { get; private set; }
    public static StaffAttendanceLogSyncService StaffAttendanceLogSyncService { get; private set; }
    public static PeriodicMaintenanceService PeriodicMaintenanceService { get; private set; }

    public static Task StartLogSyncServiceAsync(DeviceFunctions deviceFunctions)
    {
        if (LogSyncService == null)
        {
            LogSyncService = new LogSyncService(deviceFunctions);
            LogSyncService.Start();
        }

        return LogSyncService.Started;
    }

    public static void StopLogSyncService()
    {
        LogSyncService?.Stop();
        LogSyncService = null;
    }

    public static Task StartAttendanceLogSyncServiceAsync()
    {
        if (AttendanceLogSyncService == null)
        {
            AttendanceLogSyncService = new AttendanceLogSyncService();
            AttendanceLogSyncService.Start();
        }

        return AttendanceLogSyncService.Started;
    }

    public static void StopAttendanceLogSyncService()
    {
        if (AttendanceLogSyncService != null)
        {
            AttendanceLogSyncService.Stop();
            AttendanceLogSyncService.Dispose();
            AttendanceLogSyncService = null;
        }
    }

    public static Task StartStaffAttendanceLogSyncServiceAsync()
    {
        if (StaffAttendanceLogSyncService == null)
        {
            StaffAttendanceLogSyncService = new StaffAttendanceLogSyncService();
            StaffAttendanceLogSyncService.Start();
        }

        return StaffAttendanceLogSyncService.Started;
    }

    public static void StopStaffAttendanceLogSyncService()
    {
        if (StaffAttendanceLogSyncService != null)
        {
            StaffAttendanceLogSyncService.Stop();
            StaffAttendanceLogSyncService.Dispose();
            StaffAttendanceLogSyncService = null;
        }
    }

    public static Task StartPeriodicMaintenanceServiceAsync(Dashboard dashboard)
    {
        if (PeriodicMaintenanceService == null)
        {
            PeriodicMaintenanceService = new PeriodicMaintenanceService(dashboard);
            PeriodicMaintenanceService.Start();
        }

        return PeriodicMaintenanceService.Started;
    }

    public static void StopPeriodicMaintenanceService()
    {
        if (PeriodicMaintenanceService != null)
        {
            PeriodicMaintenanceService.Stop();
            PeriodicMaintenanceService.Dispose();
            PeriodicMaintenanceService = null;
            LogHelper.Log("🛑 PeriodicMaintenanceService stopped");
        }
    }

    public static void StopAllServices()
    {
        StopLogSyncService();
        StopAttendanceLogSyncService();
        StopStaffAttendanceLogSyncService();
        StopPeriodicMaintenanceService();
        LogHelper.Log("🛑 All services stopped");
    }
}