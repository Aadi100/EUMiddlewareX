using System;
using System.Collections.Generic;
using static smart_attendance_middleware.Dashboard;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace smart_attendance_middleware
{
    public static class AppSession
    {
        public static string LoggedInUser { get; set; }
        public static string UserRole { get; set; }
        public static int LoggedInUserID { get; set; }
        public static DateTime LoginTime { get; set; }
        public static bool IsDebugMode { get; set; }

        public static List<Device> DeviceList { get; set; } = new List<Device>();
        public static List<Device> StaffDeviceList { get; set; } = new List<Device>();

        public static List<Member> AllMembers { get; set; } = new List<Member>();
        public static List<Staff> AllStaff { get; set; } = new List<Staff>();

        public static List<dynamic> AllAttendanceLogs { get; set; } = new List<dynamic>();
        public static List<dynamic> AllStaffAttendanceLogs { get; set; } = new List<dynamic>();
        public static List<GateAccessed> AllGateLogs { get; set; } = new List<GateAccessed>();
        public static List<LocalUser> AllUsers { get; set; } = new List<LocalUser>();

        // Last check-in details
        public static LastAttendanceDetails LastCheckIn { get; set; }

        // Last check-out details
        public static LastAttendanceDetails LastCheckOut { get; set; }

        // Sync status
        public static SyncStatusResult SyncStatus { get; set; }

        public static List<smart_attendance_middleware.Classes.DeviceFunctions.PendingJob> PendingJobs { get; set; }
            = new List<smart_attendance_middleware.Classes.DeviceFunctions.PendingJob>();

        // Log/StaffLog rows with at least one biometric whose SyncStatus is still false —
        // the same figure shown as "Pending: N" on the Dashboard.
        public static List<LogEntry> PendingBiometricSyncLogs { get; set; } = new List<LogEntry>();
        public static List<StaffLogEntry> StaffPendingBiometricSyncLogs { get; set; } = new List<StaffLogEntry>();

        // Keyed by normalized IP, value is (type: "member"/"staff", deviceId)
        public static Dictionary<string, (string type, int deviceId)> DeviceCategoryMap { get; set; }
            = new Dictionary<string, (string type, int deviceId)>(StringComparer.OrdinalIgnoreCase);

        public static HashSet<string> StaffEnrollmentIds { get; set; }
            = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // Registry for connected devices, keyed by Serial Number (SN)
        public static System.Collections.Concurrent.ConcurrentDictionary<string, System.Net.WebSockets.WebSocket> RegisteredDevices { get; set; }
            = new System.Collections.Concurrent.ConcurrentDictionary<string, System.Net.WebSockets.WebSocket>();

        public static void ClearSession()
        {
            LoggedInUser = null;
            UserRole = null;
            LoggedInUserID = -1;
            LoginTime = DateTime.MinValue;
            DeviceList.Clear();
            StaffDeviceList.Clear();
            AllMembers.Clear();
            AllStaff.Clear();
            AllAttendanceLogs.Clear();
            AllStaffAttendanceLogs.Clear();
            AllGateLogs.Clear();
            AllUsers.Clear();
            LastCheckIn = null;
            LastCheckOut = null;
            PendingJobs.Clear();
            PendingBiometricSyncLogs.Clear();
            StaffPendingBiometricSyncLogs.Clear();
            DeviceCategoryMap.Clear();
            StaffEnrollmentIds.Clear();
            RegisteredDevices.Clear();
        }
    }

    // Class to store attendance details
    public class LastAttendanceDetails
    {
        public int MemberId { get; set; }
        public int EnrollmentId { get; set; }
        public string MemberName { get; set; }
        public string Action { get; set; }  // "check-in" or "check-out"
        public DateTime Time { get; set; }
        public string ImagePath { get; set; }  // Path to member's image
    }
}