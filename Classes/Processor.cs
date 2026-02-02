using smart_attendance_middleware.UI;
using System.Drawing;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace smart_attendance_middleware
{
    public class Processor
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly WebSocketManager _wsManager;
        private readonly Timer _timer;
        private int _attempts = 0;
        private readonly int[] _intervals = { 2000, 4000, 8000, 16000, 60000 };
        private ToolStripStatusLabel _statusLabel;
        private DeviceFunctions _deviceFunctions;
        private AttendenceServerFunctions attendenceServerFunctions;

        public Processor(DatabaseHelper dbHelper, WebSocketManager wsManager,DeviceFunctions deviceFunctions)
        {
            _dbHelper = dbHelper;
            _wsManager = wsManager;
            _deviceFunctions = deviceFunctions;
            _timer = new Timer { Interval = _intervals[0] };
            _timer.Tick += CheckInternetStatus;
        }

        public Processor(DatabaseHelper dbHelper, WebSocketManager wsManager, AttendenceServerFunctions attendenceServerFunctions)
        {
            _dbHelper = dbHelper;
            _wsManager = wsManager;
            this.attendenceServerFunctions = attendenceServerFunctions;
        }


        /// <summary>
        /// Central entry point for handling all real-time events from the WebSocketManager.
        /// Routes each event type to the appropriate internal method.
        /// </summary>
        public void HandleLiveUserUpdate(string eventType, JObject data)
        {
            switch (eventType)
            {
                case "notification.member_created":
                    SyncUserFromCloud(data);
                    _ = Task.Run(async () =>
                    {
                        await HandleMemberFaceImage(data);
                        HandleMemberCreation();
                    });
                    _ = update_session();
                    ShowMemberNotification(data, "Member Created", "A new member has been created.");

                    break;

                case "notification.member_updated":
                    _ = Task.Run(async () =>
                    {
                        await _deviceFunctions.StatusSync(data);
                        SyncUserFromCloud(data);
                        await HandleMemberFaceImage(data);
                        await HandleUpdateMember(data);

                    });
                    _ = update_session();
                    ShowMemberNotification(data, "Member Updated", "Member details have been updated.");



                    break;
                case "notification.member_deleted":
                    HandleDeletedMember(data);
                    _ = update_session();
                    ShowMemberNotification(data, "Member Deleted", "A member has been removed.");



                    break;

                case "gym.attendance":
                    HandleAttendance(data);
                    ShowMemberNotification(data, "Attendance Updated", "A member's attendance has been updated.");
                    break;


                case "notification.status":
                    _ = Task.Run(async () =>
                    {
                        await _deviceFunctions.StatusSync(data);
                        HandleStatusChange(data);
                    });
                    ShowMemberNotification(data, "Member Status Changed", "A member's status has been changed.");

                    break;

                case "member.all":
                    _ = Task.Run(async () =>
                    {
                        await _deviceFunctions.StatusSync(data);
                        if (data["version"] != null)
                        {
                            var version = data["version"]?.ToString();
                            _dbHelper.SaveSetting("VersionTimestamp", version);
                            LogHelper.Log($"💾 Saved VersionTimestamp: {version}");
                        }
                        HandleAllMembers(data);
                    });
                    break;

                case "member.get":
                    HandleSingleMember(data);
                    break;

                case "client-command:response":
                    HandleGenericResponse(data);
                    break;

                case "member.attendance.response":
                    HandleAttendanceResponse(data);
                    break;

                case "member.status.set":
                    HandleMemberStatusResponse(data);
                    break;

                default:
                    LogHelper.Log($"⚠️ Unhandled event type: {eventType}");
                    break;
            }
        }

        /// <summary>
        /// Inserts or updates a member from cloud data. Deletes if marked as 'Deleted'. Assigned machine id.
        /// </summary>

        public async void SyncUserFromCloud(JObject member)
        {
            try
            {
                int memberId = (int)member["id"];
                string status = (string)member["status"];
                int? enrollmentId = member["enrollment_id"]?.Type != JTokenType.Null ? (int?)member["enrollment_id"] : null;

                if (status == "Deleted" || status == "Expired")
                {
                    // Direct DB delete without global write lock
                    _dbHelper.DeleteMemberById(memberId);
                    LogHelper.Log($"🗑 Deleted member {memberId} from local DB.");

                    // Non-DB work outside (face image deletion)
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            bool imageDeleted = await _wsManager.DeleteMemberFaceImageAsync(memberId);
                            if (imageDeleted)
                                LogHelper.Log($"✅ Successfully deleted face image for member {memberId}");
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"❌ Error deleting face image for member {memberId}: {ex.Message}");
                        }
                    });

                    LogHelper.Log($"🗑️ Deleted user {memberId} from local DB.");
                    return;
                }

                // Direct upsert without global write lock
                _dbHelper.UpsertMember(member);
                LogHelper.Log($"✅ Synced user {memberId} with enrollment_id {enrollmentId}.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ SyncUserFromCloud error: {ex.Message}");
            }
        }


        /// <summary>
        /// Handles member face image management before member creation using WebSocketManager functionality.
        /// Checks FACE directory for existing image, deletes if found, and downloads new image if available.
        /// </summary>
        /// <param name="memberData">Member data containing face image information</param>
        private async Task HandleMemberFaceImage(JObject memberData)
        {
            try
            {
                int memberId = memberData["id"]?.Value<int>() ?? 0;
                string newFaceImageUrl = memberData["picture"]?.Value<string>();

                if (memberId == 0)
                {
                    LogHelper.Log("⚠️ Invalid member ID for face image handling");
                    return;
                }

                LogHelper.Log($"🖼️ Starting face image management for member {memberId}");

                // Delete existing face image using WebSocketManager
                bool existingImageDeleted = await _wsManager.DeleteMemberFaceImageAsync(memberId);

                if (existingImageDeleted)
                {
                    LogHelper.Log($"✅ Deleted existing face image for member {memberId}");
                }
                else
                {
                    LogHelper.Log($"ℹ️ No existing face image found for member {memberId}");
                }

                // Download new image if new face link is available using WebSocketManager
                if (!string.IsNullOrEmpty(newFaceImageUrl))
                {
                    try
                    {
                        // Create a dynamic object to mimic the members data structure that SaveMemberPicturesAsync expects
                        var memberPictureData = new
                        {
                            members = new[]
                            {
                                new
                                {
                                    id = memberId,
                                    picture = newFaceImageUrl
                                }
                            }
                        };

                        LogHelper.Log($"📥 Downloading new face image for member {memberId} from: {newFaceImageUrl}");

                        // Use WebSocketManager's existing SaveMemberPicturesAsync method via reflection
                        var saveMethod = typeof(WebSocketManager).GetMethod("SaveMemberPicturesAsync",
                            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                        if (saveMethod != null)
                        {
                            await (Task)saveMethod.Invoke(_wsManager, new object[] { memberPictureData });
                            LogHelper.Log($"✅ Successfully downloaded and saved new face image for member {memberId}");
                        }
                        else
                        {
                            LogHelper.Log($"❌ Could not access SaveMemberPicturesAsync method for member {memberId}");
                        }
                    }
                    catch (Exception downloadEx)
                    {
                        LogHelper.Log($"❌ Failed to download new face image for member {memberId}: {downloadEx.Message}");
                    }
                }
                else
                {
                    LogHelper.Log($"ℹ️ No new face image URL provided for member {memberId}");
                }

                LogHelper.Log($"🏁 Completed face image management for member {memberId}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error handling face image management: {ex.Message}");
                LogHelper.Log($"🔍 Stack Trace: {ex.StackTrace}");
            }
        }



        /// <summary>
        /// Deletes a member from the local database.
        /// </summary>
        public async void DeleteUserFromDb(int memberId)
        {
            try
            {
                using (await DatabaseQueueManager.EnterDbWriteLockAsync("DeleteUserFromDb", DbTaskPriority.High))
                {
                    _dbHelper.DeleteMemberById(memberId);
                    LogHelper.Log($"🗑 Deleted member {memberId} from local DB.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ DeleteUserFromDb error: {ex.Message}");
            }
        }

        private async void HandleAttendance(JObject data)
        {
            try
            {
                LogHelper.Log($"📍 Attendance update received: {data.ToString(Formatting.None)}");

                var isManual = data["is_manual"]?.Value<bool>() ?? false;
                if (!isManual)
                {
                    LogHelper.Log("ℹ️ Skipping attendance update (is_manual is false or not present).");
                    return;
                }

                int memberId = data["member_id"]?.Value<int>() ?? 0;
                string attendanceId = data["attendance_id"]?.Value<string>();
                string checkOutStr = data["check_out"]?.Value<string>();

                if (memberId == 0 || string.IsNullOrWhiteSpace(attendanceId))
                {
                    LogHelper.Log("⚠️ Invalid manual attendance payload (missing member_id or attendance_id). Skipping.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(checkOutStr))
                {
                    LogHelper.Log($"ℹ️ Manual attendance has no check-out for member {memberId}, AttendanceId {attendanceId}. Nothing to update.");
                    return;
                }

                DateTime checkOutTime;
                if (!DateTime.TryParse(checkOutStr, out checkOutTime))
                {
                    LogHelper.Log($"⚠️ Invalid check-out format: '{checkOutStr}'. Skipping update for member {memberId}, AttendanceId {attendanceId}.");
                    return;
                }

                using (await DatabaseQueueManager.EnterDbWriteLockAsync("HandleAttendance_Write", DbTaskPriority.High))
                {
                    _dbHelper.UpdateAttendanceLogForCheckOut(memberId, checkOutStr);

                    bool syncUpdated = _dbHelper.UpdateAttendanceLogSyncStatus(
                        memberId,
                        attendanceId,
                        checkInTime: null,
                        checkOutTime: checkOutTime,
                        checkIn: null,
                        checkOut: true
                    );

                    if (syncUpdated)
                        LogHelper.Log($"✅ Manual checkout synced for member {memberId}, AttendanceId {attendanceId}, time {checkOutTime:yyyy-MM-dd HH:mm:ss}");
                    else
                        LogHelper.Log($"ℹ️ No sync flag change needed (row not found or already synced): member {memberId}, AttendanceId {attendanceId}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error handling manual attendance: {ex.Message}");
            }
        }

        public async void HandleAttendanceResponse(JObject data)
        {
            try
            {
                LogHelper.Log($"📍 Attendance response received: {data.ToString()}");
                string attendanceId = data["id"]?.ToString();
                int memberId = data["member_id"].Value<int>();

                string checkInStr = data["check_in"]?.ToString();
                string checkOutStr = data["check_out"]?.ToString();

                bool hasCheckIn = !string.IsNullOrWhiteSpace(checkInStr);
                bool hasCheckOut = !string.IsNullOrWhiteSpace(checkOutStr);

                bool anyUpdateDone = false;
                DateTime when;

                using (await DatabaseQueueManager.EnterDbWriteLockAsync("HandleAttendanceResponse_Write", DbTaskPriority.High))
                {
                    if (hasCheckIn && DateTime.TryParse(checkInStr, out when))
                    {
                        LogHelper.Log($"Processed attendance log for member {memberId} with check-in at {when:yyyy-MM-dd HH:mm:ss} and AttendanceId {attendanceId}");
                        bool checkInSyncUpdated = _dbHelper.UpdateAttendanceLogSyncStatus(
                            memberId,
                            attendanceId,
                            checkInTime: when,
                            checkOutTime: null,
                            checkIn: true,
                            checkOut: null
                        );
                        LogHelper.Log(checkInSyncUpdated
                            ? $"✅ Updated sync status for check-in: Member {memberId}, AttendanceId {attendanceId}"
                            : $"ℹ️ No change for check-in sync (already up-to-date or row not found): Member {memberId}, AttendanceId {attendanceId}");
                        anyUpdateDone |= checkInSyncUpdated;
                    }

                    if (hasCheckOut && DateTime.TryParse(checkOutStr, out when))
                    {
                        LogHelper.Log($"Updated attendance log for member {memberId} with check-out at {when:yyyy-MM-dd HH:mm:ss} using AttendanceId {attendanceId}");
                        bool checkOutSyncUpdated = _dbHelper.UpdateAttendanceLogSyncStatus(
                            memberId,
                            attendanceId,
                            checkInTime: null,
                            checkOutTime: when,
                            checkIn: null,
                            checkOut: true
                        );
                        LogHelper.Log(checkOutSyncUpdated
                            ? $"✅ Updated sync status for check-out: Member {memberId}, AttendanceId {attendanceId}"
                            : $"ℹ️ No change for check-out sync (already up-to-date or row not found): Member {memberId}, AttendanceId {attendanceId}");
                        anyUpdateDone |= checkOutSyncUpdated;
                    }
                }

                if (anyUpdateDone)
                    LogHelper.Log($"✅ Attendance sync completed for member {memberId}, AttendanceId {attendanceId}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ HandleAttendanceResponse error: {ex.Message}");
            }
        }




        /// <summary>
        /// Handles real-time member status updates (active/inactive).
        /// </summary>
        private async void HandleStatusChange(JObject data)
        {
            try
            {
                int memberId = data["id"]?.Value<int>() ?? 0;
                string newStatus = data["status"]?.Value<string>()?.ToLower() ?? string.Empty;

                if (memberId == 0)
                {
                    LogHelper.Log("⚠️ Invalid member ID in status update");
                    return;
                }
                if (string.IsNullOrEmpty(newStatus) || (newStatus != "active" && newStatus != "inactive"))
                {
                    LogHelper.Log($"⚠️ Invalid status value '{newStatus}' for member {memberId}");
                    return;
                }

                bool shouldEnable = newStatus == "active";

                // DB write only for status update
                using (await DatabaseQueueManager.EnterDbWriteLockAsync("HandleStatusChange_UpdateStatus", DbTaskPriority.High))
                {
                    bool statusUpdated = _dbHelper.UpdateMemberStatus(memberId, shouldEnable ? "Active" : "Inactive");
                    if (!statusUpdated)
                    {
                        LogHelper.Log($"⚠️ Failed to update status for member {memberId}");
                        return;
                    }
                }

                // DB reads and network I/O outside lock
                var devices = _dbHelper.GetDetails(memberId);
                if (devices.Count == 0)
                {
                    LogHelper.Log($"ℹ️ No devices found for member {memberId}");
                    return;
                }

                foreach (var device in devices)
                {
                    try
                    {
                        var allDevices = _dbHelper.LoadDevices();
                        var currentDevice = allDevices.FirstOrDefault(d => d.MachineNo == device.machineNo);
                        if (currentDevice == null)
                        {
                            LogHelper.Log($"⚠️ Device with machine number {device.machineNo} not found");
                            continue;
                        }

                        var (deviceExists, syncStatus, memberStatus) = _dbHelper.GetMemberDeviceSyncStatus(memberId, currentDevice.DeviceId);
                        if (!deviceExists || !syncStatus)
                        {
                            LogHelper.Log($"ℹ️ Skipping device {device.machineNo} (exists={deviceExists}, sync={syncStatus})");
                            continue;
                        }

                        bool success = await DeviceService.SetUserEnabledStatus(
                            device.ip,
                            device.port,
                            device.machineNo,
                            device.password,
                            device.enrollmentNumber,
                            device.enrollmentNumber,
                            device.backupNumber,
                            shouldEnable);

                        if (success)
                            LogHelper.Log($"✅ {(shouldEnable ? "Enabled" : "Disabled")} member {memberId} on device {device.machineNo}");
                        else
                            LogHelper.Log($"⚠️ Failed to {(shouldEnable ? "enable" : "disable")} member {memberId} on device {device.machineNo}");
                    }
                    catch (Exception exDev)
                    {
                        LogHelper.Log($"❌ Error processing device {device.machineNo} for member {memberId}: {exDev.Message}");
                    }
                }

                LogHelper.Log($"🏁 Completed status update for member {memberId}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ HandleStatusChange error: {ex.Message}");
            }
        }


        private async void HandleDeletedMember(JObject data)
        {
            try
            {
                int memberId = data["id"]?.Value<int>() ?? 0;
                int? enrollmentId = data["enrollment_id"]?.Type != JTokenType.Null ? (int?)data["enrollment_id"] : null;

                if (memberId == 0)
                {
                    LogHelper.Log("⚠️ Invalid member ID in deletion request");
                    return;
                }
                if (enrollmentId == null || enrollmentId == 0)
                {
                    LogHelper.Log($"⚠️ Invalid enrollment ID for member {memberId}");
                    return;
                }

                LogHelper.Log($"🗑️ Beginning deletion process for member {memberId} (Enrollment: {enrollmentId})");

                var deviceDetails = _dbHelper.GetAvailableDeviceDetails(memberId);
                if (deviceDetails.Count > 0)
                {
                    var failedDevices = new List<(string ip, int port, int machineNo, string password, int enrollmentNumber, string reason)>();
                    var skippedDevices = new List<(string ip, int port, int machineNo, string password, int enrollmentNumber, string reason)>();

                    foreach (var device in deviceDetails)
                    {
                        try
                        {
                            var allDevices = _dbHelper.LoadDevices();
                            var currentDevice = allDevices.FirstOrDefault(d => d.MachineNo == device.machineNo);
                            if (currentDevice == null)
                            {
                                skippedDevices.Add((device.ip, device.port, device.machineNo, device.password, device.enrollmentNumber, "Device not found"));
                                continue;
                            }

                            var (deviceExists, syncStatus, memberStatus) = _dbHelper.GetMemberDeviceSyncStatus(memberId, currentDevice.DeviceId);
                            if (!deviceExists || !syncStatus)
                            {
                                skippedDevices.Add((device.ip, device.port, device.machineNo, device.password, device.enrollmentNumber, "Sync status false / not in status"));
                                continue;
                            }

                            bool success = await DeviceService.DeleteEnrollmentData(
                                device.ip,
                                device.port,
                                device.machineNo,
                                device.password,
                                device.enrollmentNumber,
                                device.enrollmentNumber,
                                12);

                            if (!success)
                                failedDevices.Add((device.ip, device.port, device.machineNo, device.password, device.enrollmentNumber, "Deletion failed"));
                        }
                        catch (Exception exDev)
                        {
                            failedDevices.Add((device.ip, device.port, device.machineNo, device.password, device.enrollmentNumber, $"Exception: {exDev.Message}"));
                        }
                    }

                    if (failedDevices.Count + skippedDevices.Count > 0)
                    {
                        await Task.Delay(10000);
                        var retryDevices = failedDevices.Concat(skippedDevices).ToList();

                        foreach (var retryDevice in retryDevices)
                        {
                            try
                            {
                                bool retrySuccess = await DeviceService.DeleteEnrollmentData(
                                    retryDevice.ip, retryDevice.port, retryDevice.machineNo, retryDevice.password,
                                    retryDevice.enrollmentNumber, retryDevice.enrollmentNumber, 12);

                                if (!retrySuccess)
                                    LogHelper.Log($"💥 Final retry failed for device {retryDevice.machineNo}");
                            }
                            catch (Exception exRetry)
                            {
                                LogHelper.Log($"💥 Retry failed for device {retryDevice.machineNo}: {exRetry.Message}");
                            }
                        }
                    }
                }

                // DB cleanup under lock
                using (await DatabaseQueueManager.EnterDbWriteLockAsync("HandleDeletedMember_Cleanup", DbTaskPriority.High))
                {
                    _dbHelper.DeleteMemberById(memberId);
                    bool logsDeleted = _dbHelper.DeleteMemberLogs(memberId);
                    LogHelper.Log(logsDeleted
                        ? $"✅ Successfully deleted logs for member {memberId}"
                        : $"ℹ️ No logs found to delete for member {memberId}");
                }

                // Face image deletion outside lock
                _ = Task.Run(async () =>
                {
                    try
                    {
                        bool imageDeleted = await _wsManager.DeleteMemberFaceImageAsync(memberId);
                        if (imageDeleted)
                            LogHelper.Log($"✅ Successfully deleted face image for member {memberId}");
                    }
                    catch (Exception exImg)
                    {
                        LogHelper.Log($"❌ Error deleting face image for member {memberId}: {exImg.Message}");
                    }
                });

                LogHelper.Log($"🏁 Completed deletion process for member {memberId}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"⛔ Critical error handling member deletion: {ex.Message}");
            }
        }



        private async Task HandleUpdateMember(JObject data)
        {
            try
            {
                // 1. Extract member data
                int memberId = data["member_id"]?.Value<int>() ?? 0;

                LogHelper.Log($"🔍 Processing update for member {memberId}");

                // Process all entries
                await _deviceFunctions.ProcessUserWorkflowAsync();

                LogHelper.Log($"🔍 Process Completed for member {memberId}");


            }
            catch (Exception ex)
            {
                LogHelper.Log($"⛔ Critical error updating member: {ex.Message}");
                LogHelper.Log($"🔍 Stack Trace: {ex.StackTrace}");
            }
        }


        private async void HandleMemberCreation()
        {
            try
            {
                LogHelper.Log("Starting member creation");
                await _deviceFunctions.ProcessUserWorkflowAsync();
                LogHelper.Log("Member creation completed");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error during member creation: {ex.Message}");

            }
        }

        /// <summary>
        /// Handles bulk member data returned from the cloud.
        /// </summary>
        
        private async void HandleAllMembers(JObject data)
        {
            try
            {
                LogHelper.Log($"📦 Received all members data (paginated): {data.ToString()}");

                var membersArray = data["members"] as JArray;
                if (membersArray == null)
                {
                    LogHelper.Log("❌ 'members' array not found in response.");
                    return;
                }

                int upsertedOrSynced = 0;
                int deletedOrExpired = 0;

                foreach (var memberObj in membersArray)
                {
                    try
                    {
                        var member = (JObject)memberObj;
                        var status = member["status"]?.Value<string>() ?? string.Empty;

                        // Reuse existing logic for delete/expire vs upsert
                        SyncUserFromCloud(member);

                        if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase) ||
                            status.Equals("Expired", StringComparison.OrdinalIgnoreCase))
                        {
                            deletedOrExpired++;
                        }
                        else
                        {
                            upsertedOrSynced++;
                        }
                    }
                    catch (Exception exItem)
                    {
                        LogHelper.Log($"⚠️ Error processing member in bulk: {exItem.Message}");
                    }
                }

                LogHelper.Log($"✅ {upsertedOrSynced} members synced/upserted, 🗑 {deletedOrExpired} deleted/expired.");

                var version = data["version"]?.Value<long>();
                if (version != null)
                {
                    try
                    {
                        _dbHelper.SaveSetting("VersionTimestamp", version.ToString());
                        LogHelper.Log($"💾 Saved VersionTimestamp: {version}");
                    }
                    catch (Exception exVer)
                    {
                        LogHelper.Log($"⚠️ Failed to save VersionTimestamp: {exVer.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ HandleAllMembers error: {ex.Message}");
            }
        }
        /// <summary>
        /// Handles single member fetch from cloud.
        /// </summary>
        private void HandleSingleMember(JObject data)
        {
            LogHelper.Log($"👤 Single member fetched: {data.ToString()}");
            // TODO: Optional use-case for debugging or search
        }

        /// <summary>
        /// Handles command response for client-triggered actions.
        /// </summary>
        private void HandleGenericResponse(JObject data)
        {
            LogHelper.Log($"📨 Command response received: {data.ToString()}");
        }



        //Abdul Hadi Code

        //Update Status in Label in form
        public void MonitorInternetStatus(ToolStripStatusLabel label)
        {
            _statusLabel = label;
            _timer.Start();
        }

        //Loop to checK internet status
        private void CheckInternetStatus(object sender, EventArgs e)
        {
            bool connected = IsInternetAvailable();

            if (_statusLabel.GetCurrentParent() != null)
            {
                _statusLabel.GetCurrentParent().Invoke((MethodInvoker)(() =>
                {
                    _statusLabel.Text = $"Internet: {(connected ? "Connected" : "Disconnected")}";
                }));
            }

            _timer.Interval = connected ? 60000 : _intervals[Math.Min(_attempts++, _intervals.Length - 1)];
        }

        //Check if internet is available
        private bool IsInternetAvailable()
        {
            try { return new Ping().Send("8.8.8.8", 3000).Status == IPStatus.Success; }
            catch { return false; }
        }



        public async Task update_session()
        {
            // Reads only; no global DB write lock required.
            try
            {
                List<Device> devices = _dbHelper.LoadDevices();
                if (devices != null && devices.Count > 0)
                {
                    AppSession.DeviceList = devices;
                    var sb = new StringBuilder("Loaded devices:\n");
                    foreach (var device in devices)
                    {
                        sb.AppendLine($"Device {device.DeviceId}:");
                        sb.AppendLine($"  IP         : {device.DeviceIP}");
                        sb.AppendLine($"  Port       : {device.Port}");
                        sb.AppendLine($"  Password   : {device.Password}");
                        sb.AppendLine($"  Machine No : {device.MachineNo}");
                        sb.AppendLine($"  Connection Type : {device.ConnectionType}");
                        sb.AppendLine();
                    }
                    LogHelper.Log(sb.ToString());
                }
                else
                {
                    LogHelper.Log("No devices found in database.");
                }

                List<Device> staffDevices = _dbHelper.LoadStaffDevices();
                if (staffDevices != null && staffDevices.Count > 0)
                {
                    AppSession.StaffDeviceList = staffDevices;
                    var sbs = new StringBuilder("Loaded staff devices:\n");
                    foreach (var sd in staffDevices)
                    {
                        sbs.AppendLine($"Staff Device {sd.StaffDeviceId}:");
                        sbs.AppendLine($"  IP         : {sd.DeviceIP}");
                        sbs.AppendLine($"  Port       : {sd.Port}");
                        sbs.AppendLine($"  Password   : {sd.Password}");
                        sbs.AppendLine($"  Machine No : {sd.MachineNo}");
                        sbs.AppendLine();
                    }
                    LogHelper.Log(sbs.ToString());
                }
                else
                {
                    LogHelper.Log("No staff devices found in database.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Error loading devices: {ex.Message}");
            }
        }

        //Abdul Hadi Code
        ///his method handles the response for setting member status.
        private void HandleMemberStatusResponse(JObject data)
        {
            try
            {
                LogHelper.Log($"📊 Member status set response: {data.ToString(Formatting.None)}");

                // Check if the response contains status information
                var statusInfo = data["message"]?.ToString();
                if (!string.IsNullOrEmpty(statusInfo))
                {
                    LogHelper.Log($"✅ Status update result: {statusInfo}");
                }

            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Error handling member status response: {ex.Message}");
            }
        }

        private void ShowMemberNotification(JObject data, string title, string defaultMessage)
        {
            try
            {
                int memberId = data["id"]?.Value<int>() ??
                               data["member_id"]?.Value<int>() ?? 0;

                string name = data["name"]?.Value<string>() ??
                              data["full_name"]?.Value<string>() ??
                              $"Member #{memberId}";

                string message = $"{name}\n{defaultMessage}";
                // Optional: choose icon based on title
                Icon icon = null;
                if (title.IndexOf("Created", StringComparison.OrdinalIgnoreCase) >= 0)
                    icon = SystemIcons.Information;
                else if (title.IndexOf("Updated", StringComparison.OrdinalIgnoreCase) >= 0)
                    icon = SystemIcons.Application;
                else if (title.IndexOf("Deleted", StringComparison.OrdinalIgnoreCase) >= 0)
                    icon = SystemIcons.Warning;

                ToastNotifier.Show(title, message, icon, 4500);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"⚠️ Failed to show notification: {ex.Message}");
            }
        }
    }
    
}
