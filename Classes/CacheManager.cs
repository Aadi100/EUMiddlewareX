using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace smart_attendance_middleware.Classes
{
    /// <summary>
    /// Manages in-memory cache with incremental updates to avoid expensive full database queries.
    /// Updates AppSession collections in real-time without calling LoadInitialUiAsync repeatedly.
    /// </summary>
    public class CacheManager
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly object _lockObject = new object();
        private DateTime _lastMembersSync = DateTime.MinValue;
        private DateTime _lastStaffSync = DateTime.MinValue;
        private DateTime _lastAttendanceLogsSync = DateTime.MinValue;
        private DateTime _lastStaffAttendanceLogsSync = DateTime.MinValue;
        private DateTime _lastGateLogsSync = DateTime.MinValue;
        private DateTime _lastUsersSync = DateTime.MinValue;
        private DateTime _lastPendingJobsSync = DateTime.MinValue;

        private Timer _refreshTimer;
        private readonly int _refreshIntervalSeconds;

        public event EventHandler<CacheUpdateEventArgs> CacheUpdated;

        public CacheManager(DatabaseHelper dbHelper, int refreshIntervalSeconds = 3600)
        {
            _dbHelper = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _refreshIntervalSeconds = refreshIntervalSeconds;
        }

        /// <summary>
        /// Performs initial full load of all data into AppSession
        /// </summary>
        public async Task InitialLoadAsync()
        {
            try
            {
                LogHelper.Log("[CacheManager] Starting initial cache load...");

                await Task.Run(() =>
                {
                    lock (_lockObject)
                    {
                        var now = DateTime.Now;

                        AppSession.AllMembers = _dbHelper.GetMembers() ?? new List<Member>();
                        _lastMembersSync = now;

                        AppSession.AllStaff = _dbHelper.GetStaffs() ?? new List<Staff>();
                        _lastStaffSync = now;

                        AppSession.AllAttendanceLogs = _dbHelper.GetAllAttendanceLogs() ?? new List<dynamic>();
                        if (AppSession.AllAttendanceLogs.Count > 10000)
                            AppSession.AllAttendanceLogs = AppSession.AllAttendanceLogs.Take(10000).ToList();
                        _lastAttendanceLogsSync = now;

                        AppSession.AllStaffAttendanceLogs = _dbHelper.GetAllStaffAttendanceLogs() ?? new List<dynamic>();
                        if (AppSession.AllStaffAttendanceLogs.Count > 10000)
                            AppSession.AllStaffAttendanceLogs = AppSession.AllStaffAttendanceLogs.Take(10000).ToList();
                        _lastStaffAttendanceLogsSync = now;

                        AppSession.AllGateLogs = _dbHelper.GetAllGateAccesseds() ?? new List<GateAccessed>();
                        if (AppSession.AllGateLogs.Count > 10000)
                            AppSession.AllGateLogs = AppSession.AllGateLogs.Take(10000).ToList();
                        _lastGateLogsSync = now;

                        AppSession.AllUsers = _dbHelper.LoadUsers() ?? new List<LocalUser>();
                        _lastUsersSync = now;

                        AppSession.PendingJobs = _dbHelper.GetAllPendingJobs() ?? new List<DeviceFunctions.PendingJob>();
                        _lastPendingJobsSync = now;
                    }
                });

                LogHelper.Log($"[CacheManager] Initial load complete: " +
                    $"{AppSession.AllMembers.Count} members, " +
                    $"{AppSession.AllStaff.Count} staff, " +
                    $"{AppSession.AllAttendanceLogs.Count} attendance logs");

                OnCacheUpdated(new CacheUpdateEventArgs { UpdateType = CacheUpdateType.FullRefresh });
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[CacheManager] Error during initial load: {ex.Message}");
                throw;
            }
        }

        /// <summary>
        /// Starts automatic periodic incremental updates
        /// </summary>
        public void StartAutoRefresh()
        {
            if (_refreshTimer != null)
            {
                LogHelper.Log("[CacheManager] Auto-refresh already started");
                return;
            }

            _refreshTimer = new Timer(async _ => await RefreshIncrementalAsync(),
                null,
                TimeSpan.FromSeconds(_refreshIntervalSeconds),
                TimeSpan.FromSeconds(_refreshIntervalSeconds));

            LogHelper.Log($"[CacheManager] Auto-refresh started (every {_refreshIntervalSeconds}s)");
        }

        /// <summary>
        /// Stops automatic refresh
        /// </summary>
        public void StopAutoRefresh()
        {
            _refreshTimer?.Dispose();
            _refreshTimer = null;
            LogHelper.Log("[CacheManager] Auto-refresh stopped");
        }

        /// <summary>
        /// Performs incremental refresh - only fetches changed records since last sync
        /// </summary>
        public async Task RefreshIncrementalAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    lock (_lockObject)
                    {
                        var updateTypes = new List<CacheUpdateType>();

                        // Update Members (incremental)
                        var newMembers = _dbHelper.GetMembersUpdatedAfter(_lastMembersSync);
                        if (newMembers != null && newMembers.Count > 0)
                        {
                            UpdateMemberCache(newMembers);
                            updateTypes.Add(CacheUpdateType.Members);
                            LogHelper.Log($"[CacheManager] Updated {newMembers.Count} members");
                        }
                        _lastMembersSync = DateTime.Now;

                        // Update Staff (incremental)
                        var newStaff = _dbHelper.GetStaffUpdatedAfter(_lastStaffSync);
                        if (newStaff != null && newStaff.Count > 0)
                        {
                            UpdateStaffCache(newStaff);
                            updateTypes.Add(CacheUpdateType.Staff);
                            LogHelper.Log($"[CacheManager] Updated {newStaff.Count} staff");
                        }
                        _lastStaffSync = DateTime.Now;

                        // Update Attendance Logs (incremental - last 24 hours rolling window)
                        var newAttendanceLogs = _dbHelper.GetAttendanceLogsAfter(_lastAttendanceLogsSync);
                        if (newAttendanceLogs != null && newAttendanceLogs.Count > 0)
                        {
                            UpdateAttendanceLogsCache(newAttendanceLogs);
                            updateTypes.Add(CacheUpdateType.AttendanceLogs);
                            LogHelper.Log($"[CacheManager] Updated {newAttendanceLogs.Count} attendance logs");
                        }
                        _lastAttendanceLogsSync = DateTime.Now;

                        // Update Staff Attendance Logs
                        var newStaffLogs = _dbHelper.GetStaffAttendanceLogsAfter(_lastStaffAttendanceLogsSync);
                        if (newStaffLogs != null && newStaffLogs.Count > 0)
                        {
                            UpdateStaffAttendanceLogsCache(newStaffLogs);
                            updateTypes.Add(CacheUpdateType.StaffAttendanceLogs);
                            LogHelper.Log($"[CacheManager] Updated {newStaffLogs.Count} staff logs");
                        }
                        _lastStaffAttendanceLogsSync = DateTime.Now;

                        // Update Gate Logs (incremental)
                        var newGateLogs = _dbHelper.GetGateLogsAfter(_lastGateLogsSync);
                        if (newGateLogs != null && newGateLogs.Count > 0)
                        {
                            UpdateGateLogsCache(newGateLogs);
                            updateTypes.Add(CacheUpdateType.GateLogs);
                            LogHelper.Log($"[CacheManager] Updated {newGateLogs.Count} gate logs");
                        }
                        _lastGateLogsSync = DateTime.Now;

                        // Update Users
                        var newUsers = _dbHelper.GetUsersUpdatedAfter(_lastUsersSync);
                        if (newUsers != null && newUsers.Count > 0)
                        {
                            UpdateUsersCache(newUsers);
                            updateTypes.Add(CacheUpdateType.Users);
                            LogHelper.Log($"[CacheManager] Updated {newUsers.Count} users");
                        }
                        _lastUsersSync = DateTime.Now;

                        // Update Pending Jobs
                        var newPendingJobs = _dbHelper.GetPendingJobsUpdatedAfter(_lastPendingJobsSync);
                        if (newPendingJobs != null && newPendingJobs.Count > 0)
                        {
                            UpdatePendingJobsCache(newPendingJobs);
                            updateTypes.Add(CacheUpdateType.PendingJobs);
                            LogHelper.Log($"[CacheManager] Updated {newPendingJobs.Count} pending jobs");
                        }
                        _lastPendingJobsSync = DateTime.Now;

                        if (updateTypes.Count > 0)
                        {
                            OnCacheUpdated(new CacheUpdateEventArgs { UpdateTypes = updateTypes });
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[CacheManager] Error during incremental refresh: {ex.Message}");
            }
        }

        /// <summary>
        /// Immediately updates a specific member in cache (call after insert/update/delete)
        /// </summary>
        public void UpdateMemberImmediate(Member member, CacheOperation operation)
        {
            if (member == null) return;

            lock (_lockObject)
            {
                switch (operation)
                {
                    case CacheOperation.Add:
                    case CacheOperation.Update:
                        var existing = AppSession.AllMembers.FirstOrDefault(m => m.MemberID == member.MemberID);
                        if (existing != null)
                        {
                            AppSession.AllMembers.Remove(existing);
                        }
                        AppSession.AllMembers.Add(member);
                        break;

                    case CacheOperation.Delete:
                        AppSession.AllMembers.RemoveAll(m => m.MemberID == member.MemberID);
                        break;
                }

                LogHelper.Log($"[CacheManager] Immediate member update: {operation} - ID {member.MemberID}");
                OnCacheUpdated(new CacheUpdateEventArgs { UpdateType = CacheUpdateType.Members });
            }
        }

        /// <summary>
        /// Immediately updates a specific staff in cache
        /// </summary>
        public void UpdateStaffImmediate(Staff staff, CacheOperation operation)
        {
            if (staff == null) return;

            lock (_lockObject)
            {
                switch (operation)
                {
                    case CacheOperation.Add:
                    case CacheOperation.Update:
                        var existing = AppSession.AllStaff.FirstOrDefault(s => s.StaffID == staff.StaffID);
                        if (existing != null)
                        {
                            AppSession.AllStaff.Remove(existing);
                        }
                        AppSession.AllStaff.Add(staff);
                        break;

                    case CacheOperation.Delete:
                        AppSession.AllStaff.RemoveAll(s => s.StaffID == staff.StaffID);
                        break;
                }

                LogHelper.Log($"[CacheManager] Immediate staff update: {operation} - ID {staff.StaffID}");
                OnCacheUpdated(new CacheUpdateEventArgs { UpdateType = CacheUpdateType.Staff });
            }
        }

        /// <summary>
        /// Adds new attendance log to cache immediately
        /// </summary>
        public void AddAttendanceLogImmediate(dynamic log, bool isStaff = false)
        {
            if (log == null) return;

            lock (_lockObject)
            {
                if (isStaff)
                {
                    AppSession.AllStaffAttendanceLogs.Insert(0, log);
                    // Keep only last 10000 logs in memory
                    if (AppSession.AllStaffAttendanceLogs.Count > 10000)
                    {
                        AppSession.AllStaffAttendanceLogs.RemoveAt(AppSession.AllStaffAttendanceLogs.Count - 1);
                    }
                    OnCacheUpdated(new CacheUpdateEventArgs { UpdateType = CacheUpdateType.StaffAttendanceLogs });
                }
                else
                {
                    AppSession.AllAttendanceLogs.Insert(0, log);
                    if (AppSession.AllAttendanceLogs.Count > 10000)
                    {
                        AppSession.AllAttendanceLogs.RemoveAt(AppSession.AllAttendanceLogs.Count - 1);
                    }
                    OnCacheUpdated(new CacheUpdateEventArgs { UpdateType = CacheUpdateType.AttendanceLogs });
                }

                LogHelper.Log($"[CacheManager] Immediate attendance log added: {(isStaff ? "Staff" : "Member")}");
            }
        }

        /// <summary>
        /// Adds new gate log to cache immediately
        /// </summary>
        public void AddGateLogImmediate(GateAccessed gateLog)
        {
            if (gateLog == null) return;

            lock (_lockObject)
            {
                AppSession.AllGateLogs.Insert(0, gateLog);

                // Keep only last 10000 logs
                if (AppSession.AllGateLogs.Count > 10000)
                {
                    AppSession.AllGateLogs.RemoveAt(AppSession.AllGateLogs.Count - 1);
                }

                LogHelper.Log($"[CacheManager] Immediate gate log added");
                OnCacheUpdated(new CacheUpdateEventArgs { UpdateType = CacheUpdateType.GateLogs });
            }
        }

        /// <summary>
        /// Forces full refresh of all data (use sparingly)
        /// </summary>
        public async Task ForceFullRefreshAsync()
        {
            LogHelper.Log("[CacheManager] Force full refresh requested...");
            _lastMembersSync = DateTime.MinValue;
            _lastStaffSync = DateTime.MinValue;
            _lastAttendanceLogsSync = DateTime.MinValue;
            _lastStaffAttendanceLogsSync = DateTime.MinValue;
            _lastGateLogsSync = DateTime.MinValue;
            _lastUsersSync = DateTime.MinValue;
            _lastPendingJobsSync = DateTime.MinValue;

            await InitialLoadAsync();
        }

        #region Private Helper Methods

        private void UpdateMemberCache(List<Member> newMembers)
        {
            foreach (var member in newMembers)
            {
                var existing = AppSession.AllMembers.FirstOrDefault(m => m.MemberID == member.MemberID);
                if (existing != null)
                {
                    AppSession.AllMembers.Remove(existing);
                }
                AppSession.AllMembers.Add(member);
            }
        }

        private void UpdateStaffCache(List<Staff> newStaff)
        {
            foreach (var staff in newStaff)
            {
                var existing = AppSession.AllStaff.FirstOrDefault(s => s.StaffID == staff.StaffID);
                if (existing != null)
                {
                    AppSession.AllStaff.Remove(existing);
                }
                AppSession.AllStaff.Add(staff);
            }
        }

        private void UpdateAttendanceLogsCache(List<dynamic> newLogs)
        {
            // For attendance logs, prepend new logs and maintain a rolling window
            foreach (var log in newLogs.OrderByDescending(l => l.CheckInTime))
            {
                AppSession.AllAttendanceLogs.Insert(0, log);
            }

            // Keep only recent logs (last 10000)
            if (AppSession.AllAttendanceLogs.Count > 10000)
            {
                AppSession.AllAttendanceLogs = AppSession.AllAttendanceLogs.Take(10000).ToList();
            }
        }

        private void UpdateStaffAttendanceLogsCache(List<dynamic> newLogs)
        {
            foreach (var log in newLogs.OrderByDescending(l => l.CheckInTime))
            {
                AppSession.AllStaffAttendanceLogs.Insert(0, log);
            }

            if (AppSession.AllStaffAttendanceLogs.Count > 10000)
            {
                AppSession.AllStaffAttendanceLogs = AppSession.AllStaffAttendanceLogs.Take(10000).ToList();
            }
        }

        private void UpdateGateLogsCache(List<GateAccessed> newLogs)
        {
            foreach (var log in newLogs.OrderByDescending(l => l.AccessedTime))
            {
                AppSession.AllGateLogs.Insert(0, log);
            }

            if (AppSession.AllGateLogs.Count > 10000)
            {
                AppSession.AllGateLogs = AppSession.AllGateLogs.Take(10000).ToList();
            }
        }

        private void UpdateUsersCache(List<LocalUser> newUsers)
        {
            foreach (var user in newUsers)
            {
                var existing = AppSession.AllUsers.FirstOrDefault(u => u.LocalUserID == user.LocalUserID);
                if (existing != null)
                {
                    AppSession.AllUsers.Remove(existing);
                }
                AppSession.AllUsers.Add(user);
            }
        }

        private void UpdatePendingJobsCache(List<DeviceFunctions.PendingJob> newJobs)
        {
            // For pending jobs, replace completely as they change frequently
            AppSession.PendingJobs = newJobs;
        }

        private void OnCacheUpdated(CacheUpdateEventArgs e)
        {
            CacheUpdated?.Invoke(this, e);
        }

        #endregion
    }

    #region Supporting Classes

    public enum CacheOperation
    {
        Add,
        Update,
        Delete
    }

    public enum CacheUpdateType
    {
        FullRefresh,
        Members,
        Staff,
        AttendanceLogs,
        StaffAttendanceLogs,
        GateLogs,
        Users,
        PendingJobs
    }

    public class CacheUpdateEventArgs : EventArgs
    {
        public CacheUpdateType UpdateType { get; set; }
        public List<CacheUpdateType> UpdateTypes { get; set; } = new List<CacheUpdateType>();
    }

    #endregion
}
