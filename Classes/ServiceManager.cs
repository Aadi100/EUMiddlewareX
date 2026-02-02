using smart_attendance_middleware;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;

public static class ServiceManager
{
    public static LogSyncService LogSyncService { get; private set; }
    public static AttendanceLogSyncService AttendanceLogSyncService { get; private set; }
    public static PeriodicMaintenanceService PeriodicMaintenanceService { get; private set; }

    public static void StartLogSyncService(DeviceFunctions deviceFunctions)
    {
        if (LogSyncService == null)
        {
            LogSyncService = new LogSyncService(deviceFunctions);
            LogSyncService.Start();
        }
    }

    public static void StopLogSyncService()
    {
        LogSyncService?.Stop();
        LogSyncService = null;
    }

    public static void StartAttendanceLogSyncService(WebSocketManager wsManager)
    {
        if (AttendanceLogSyncService == null)
        {
            AttendanceLogSyncService = new AttendanceLogSyncService(wsManager);
            AttendanceLogSyncService.Start();
        }
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

    public static void StartPeriodicMaintenanceService(Dashboard dashboard)
    {
        if (PeriodicMaintenanceService == null)
        {
            PeriodicMaintenanceService = new PeriodicMaintenanceService(dashboard);
            PeriodicMaintenanceService.Start();
            LogHelper.Log("✅ PeriodicMaintenanceService started successfully");
        }
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
        StopPeriodicMaintenanceService();
        LogHelper.Log("🛑 All services stopped");
    }
}