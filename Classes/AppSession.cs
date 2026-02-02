using System;
using System.Collections.Generic;
using static smart_attendance_middleware.Dashboard;

namespace smart_attendance_middleware
{
    public static class AppSession
    {
        public static string LoggedInUser { get; set; }
        public static string UserRole { get; set; }
        public static int LoggedInUserID { get; set; }
        public static DateTime LoginTime { get; set; }

        public static List<Device> DeviceList { get; set; } = new List<Device>();

        public static List<Device> StaffDeviceList { get; set; } = new List<Device>();

        // Last check-in details
        public static LastAttendanceDetails LastCheckIn { get; set; }

        // Last check-out details  
        public static LastAttendanceDetails LastCheckOut { get; set; }

        // Sync status
        public static SyncStatusResult SyncStatus { get; set; }

        public static List<smart_attendance_middleware.Classes.DeviceFunctions.PendingJob> PendingJobs { get; set; }
            = new List<smart_attendance_middleware.Classes.DeviceFunctions.PendingJob>();


        public static void ClearSession()
        {
            LoggedInUser = null;
            UserRole = null;
            LoggedInUserID = -1;
            LoginTime = DateTime.MinValue;
            DeviceList.Clear();
            LastCheckIn = null;
            LastCheckOut = null;
            PendingJobs.Clear();
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