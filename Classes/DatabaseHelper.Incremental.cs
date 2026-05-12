using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using smart_attendance_middleware.Classes;

namespace smart_attendance_middleware
{
    /// <summary>
    /// Partial class containing incremental query methods for CacheManager.
    /// These methods fetch only records updated/created after a specific timestamp.
    /// </summary>
    public partial class DatabaseHelper
    {
        /// <summary>
        /// Gets members updated or created after a specific timestamp
        /// </summary>
        public List<Member> GetMembersUpdatedAfter(DateTime lastSync)
        {
            var members = new List<Member>();

            string query = @"
                SELECT MemberID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, PackageExpiry, SyncStatus, UpdatedAt
                FROM Members
                WHERE UpdatedAt > @LastSync OR CreatedAt > @LastSync
                ORDER BY UpdatedAt DESC";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LastSync", lastSync);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var syncStatusJson = reader.IsDBNull(8) ? "{}" : reader.GetString(8);
                            members.Add(new Member
                            {
                                MemberID = reader.GetInt32(0),
                                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Status = reader.IsDBNull(2) ? null : reader.GetString(2),
                                RFID = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Picture = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Fingerprint = !reader.IsDBNull(5) && reader.GetBoolean(5),
                                EnrollmentID = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                PackageExpiry = reader.IsDBNull(7) ? null : reader.GetString(7),
                                SyncStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson) ?? new Dictionary<int, bool>()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DB] Error getting updated members: {ex.Message}");
            }

            return members;
        }

        /// <summary>
        /// Gets staff updated or created after a specific timestamp
        /// </summary>
        public List<Staff> GetStaffUpdatedAfter(DateTime lastSync)
        {
            var staff = new List<Staff>();

            string query = @"
                SELECT StaffID, Name, Status, RFID, Picture, Fingerprint, EnrollmentID, SyncStatus
                FROM Staff
                WHERE UpdatedAt > @LastSync OR CreatedAt > @LastSync
                ORDER BY UpdatedAt DESC";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LastSync", lastSync);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var syncStatusJson = reader.IsDBNull(7) ? "{}" : reader.GetString(7);
                            staff.Add(new Staff
                            {
                                StaffID = reader.GetString(0),
                                Name = reader.IsDBNull(1) ? null : reader.GetString(1),
                                Status = reader.IsDBNull(2) ? null : reader.GetString(2),
                                RFID = reader.IsDBNull(3) ? null : reader.GetString(3),
                                Picture = reader.IsDBNull(4) ? null : reader.GetString(4),
                                Fingerprint = !reader.IsDBNull(5) && reader.GetBoolean(5),
                                EnrollmentID = reader.IsDBNull(6) ? (int?)null : reader.GetInt32(6),
                                SyncStatus = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<int, bool>>(syncStatusJson) ?? new Dictionary<int, bool>()
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DB] Error getting updated staff: {ex.Message}");
            }

            return staff;
        }

        /// <summary>
        /// Gets attendance logs created after a specific timestamp (with 24-hour rolling window)
        /// </summary>
        public List<dynamic> GetAttendanceLogsAfter(DateTime lastSync)
        {
            var logs = new List<dynamic>();

            // Only get logs from last 24 hours to keep cache manageable
            var cutoffTime = DateTime.Now.AddHours(-24);
            var effectiveLastSync = lastSync > cutoffTime ? lastSync : cutoffTime;

            string query = @"
                SELECT TOP 1000 
                    LogID,
                    MemberID,
                    AttendanceId,
                    CheckInTime,
                    CheckOutTime,
                    CreatedAt,
                    UpdatedAt
                FROM AttendanceLogs
                WHERE CreatedAt > @LastSync
                ORDER BY CheckInTime DESC";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LastSync", effectiveLastSync);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new
                            {
                                LogID = reader.GetInt32(0),
                                MemberID = reader.GetInt32(1),
                                AttendanceId = reader.IsDBNull(2) ? null : reader.GetString(2),
                                CheckInTime = reader.IsDBNull(3) ? null : reader.GetDateTime(3).ToString("yyyy-MM-dd HH:mm:ss"),
                                CheckOutTime = reader.IsDBNull(4) ? null : reader.GetDateTime(4).ToString("yyyy-MM-dd HH:mm:ss"),
                                CreatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5).ToString("yyyy-MM-dd HH:mm:ss"),
                                UpdatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6).ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DB] Error getting updated attendance logs: {ex.Message}");
            }

            return logs;
        }

        /// <summary>
        /// Gets staff attendance logs created after a specific timestamp (with 24-hour rolling window)
        /// </summary>
        public List<dynamic> GetStaffAttendanceLogsAfter(DateTime lastSync)
        {
            var logs = new List<dynamic>();

            var cutoffTime = DateTime.Now.AddHours(-24);
            var effectiveLastSync = lastSync > cutoffTime ? lastSync : cutoffTime;

            string query = @"
                SELECT TOP 1000 
                    sal.LogID,
                    sal.EnrollmentID,
                    s.StaffID AS AttendeeID,
                    COALESCE(s.Name, '') AS Name,
                    sal.AttendanceId,
                    sal.SyncStatus,
                    sal.CheckInTime,
                    sal.CheckOutTime
                FROM StaffAttendanceLog sal
                LEFT JOIN Staff s ON s.EnrollmentID = sal.EnrollmentID
                WHERE sal.CreatedAt > @LastSync
                ORDER BY COALESCE(sal.CheckInTime, sal.CheckOutTime) DESC";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LastSync", effectiveLastSync);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new
                            {
                                LogID = reader.IsDBNull(0) ? 0 : reader.GetInt32(0),
                                EnrollmentID = reader.IsDBNull(1) ? 0 : reader.GetInt32(1),
                                AttendeeID = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                Name = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                AttendanceId = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                SyncStatus = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                CheckInTime = reader.IsDBNull(6) ? string.Empty : reader.GetDateTime(6).ToString("yyyy-MM-dd HH:mm:ss"),
                                CheckOutTime = reader.IsDBNull(7) ? string.Empty : reader.GetDateTime(7).ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DB] Error getting updated staff logs: {ex.Message}");
            }

            return logs;
        }

        /// <summary>
        /// Gets gate logs created after a specific timestamp (with 24-hour rolling window)
        /// </summary>
        public List<GateAccessed> GetGateLogsAfter(DateTime lastSync)
        {
            var logs = new List<GateAccessed>();

            var cutoffTime = DateTime.Now.AddHours(-24);
            var effectiveLastSync = lastSync > cutoffTime ? lastSync : cutoffTime;

            string query = @"
                SELECT TOP 1000 
                    GateId, DeviceIP, MemberId, IsVisitor, VisitorName, 
                    VisitorPhone, Reason, AccessedTime, Username
                FROM GateAccessed
                WHERE CreatedAt > @LastSync
                ORDER BY AccessedTime DESC";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LastSync", effectiveLastSync);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            logs.Add(new GateAccessed
                            {
                                GateId = reader.GetInt32(0),
                                DeviceIP = reader.GetString(1),
                                MemberId = reader.GetInt32(2),
                                IsVisitor = reader.GetBoolean(3),
                                VisitorName = reader.IsDBNull(4) ? null : reader.GetString(4),
                                VisitorPhone = reader.IsDBNull(5) ? null : reader.GetString(5),
                                Reason = reader.IsDBNull(6) ? null : reader.GetString(6),
                                AccessedTime = reader.IsDBNull(7) ? DateTime.MinValue : reader.GetDateTime(7),
                                Username = reader.IsDBNull(8) ? null : reader.GetString(8)
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DB] Error getting updated gate logs: {ex.Message}");
            }

            return logs;
        }

        /// <summary>
        /// Gets users updated or created after a specific timestamp
        /// </summary>
        public List<LocalUser> GetUsersUpdatedAfter(DateTime lastSync)
        {
            var users = new List<LocalUser>();

            string query = @"
                SELECT LocalUserID, UserName, Role, Password, BackupCode, CreatedAt, UpdatedAt
                FROM LocalUsers
                WHERE UpdatedAt > @LastSync OR CreatedAt > @LastSync
                ORDER BY UpdatedAt DESC";

            try
            {
                using (var conn = new SqlConnection(connectionString))
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@LastSync", lastSync);
                    conn.Open();

                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new LocalUser
                            {
                                LocalUserID = reader.GetInt32(0),
                                UserName = reader.GetString(1),
                                Role = reader.GetString(2),
                                Password = reader.GetString(3),
                                BackupCode = reader.IsDBNull(4) ? null : reader.GetString(4),
                                CreatedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5).ToString("yyyy-MM-dd HH:mm:ss"),
                                UpdatedAt = reader.IsDBNull(6) ? null : reader.GetDateTime(6).ToString("yyyy-MM-dd HH:mm:ss")
                            });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[DB] Error getting updated users: {ex.Message}");
            }

            return users;
        }

        /// <summary>
        /// Gets pending jobs updated after a specific timestamp.
        /// For pending jobs, just return all current pending jobs as they change frequently.
        /// </summary>
        public List<DeviceFunctions.PendingJob> GetPendingJobsUpdatedAfter(DateTime lastSync)
        {
            // For pending jobs, just return all current pending jobs
            // They are lightweight and change frequently
            return GetAllPendingJobs();
        }
    }
}
