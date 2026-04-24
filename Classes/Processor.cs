using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using smart_attendance_middleware.Classes;
using smart_attendance_middleware.Services;
using smart_attendance_middleware.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;


namespace smart_attendance_middleware
{
    public class Processor
    {
        private readonly DatabaseHelper _dbHelper;
        private readonly WebSocketManager _wsManager;
        private DeviceFunctions _deviceFunctions;
        private AttendenceServerFunctions attendenceServerFunctions;
        private StaffAttendenceServerFunctions staffAttendenceServerFunctions;
        public Processor(DatabaseHelper dbHelper, WebSocketManager wsManager,DeviceFunctions deviceFunctions)
        {
            _dbHelper = dbHelper;
            _wsManager = wsManager;
            _deviceFunctions = deviceFunctions;
        }

        public Processor(DatabaseHelper dbHelper, WebSocketManager wsManager, AttendenceServerFunctions attendenceServerFunctions)
        {
            _dbHelper = dbHelper;
            _wsManager = wsManager;
            this.attendenceServerFunctions = attendenceServerFunctions;
        }

        public Processor(DatabaseHelper dbHelper, WebSocketManager wsManager, StaffAttendenceServerFunctions staffAttendenceServerFunctions)
        {
                _dbHelper = dbHelper;
                _wsManager = wsManager;
                this.staffAttendenceServerFunctions = staffAttendenceServerFunctions;
        }


        /// <summary>
        /// Central entry point for handling all real-time events from the WebSocketManager.
        /// Routes each event type to the appropriate internal method.
        /// </summary>
        public void HandleLiveUserUpdate(string eventType, JObject data)
        {
            switch (eventType)
            {

                case "attendance_created":
                    LogHelper.Log("Received attendance_created event. Processing attendance update...");
                    HandleAttendance(data);
                    ShowUsersNotification(data, "Attendance Created", "A new attendance record has been created.");
                    break;

                case "attendance_updated":
                    LogHelper.Log("Received attendance_updated event. Processing attendance update...");
                    HandleAttendance(data);
                    ShowUsersNotification(data, "Attendance Updated", "An attendance record has been updated.");
                    break;

                case "user_created":
                    LogHelper.Log("Received user_created event. Processing new user...");
                    var userData = data["user"] as JObject;
                    if (userData != null)
                    {
                        SyncUserFromCloud(userData);
                        _ = Task.Run(async () =>
                        {
                            await HandleUsersFaceImage(userData);
                            HandleUsersCreation();
                        });
                        ShowUsersNotification(userData, "User Created", "A new user has been created.");
                    }
                    break;

                case "user_updated":
                    LogHelper.Log("Received user_updated event. Processing user update...");
                    var userDataUpd = data["user"] as JObject;
                    if (userDataUpd != null)
                    {
                        _ = Task.Run(async () =>
                        {
                            await _deviceFunctions.StatusSync(userDataUpd);
                            SyncUserFromCloud(userDataUpd);
                            await HandleUsersFaceImage(userDataUpd);
                            await HandleUpdateUsers(userDataUpd);
                        });
                        ShowUsersNotification(userDataUpd, "User Updated", "User details have been updated.");
                    }
                    break;

                //case "notification.member_deleted":
                //    HandleDeletedMember(data);
                //    ShowUsersNotification(data, "User Deleted", "A user has been removed.");
                //    break;

                case "member_status":
                    LogHelper.Log("Received member_status event. Processing status synchronization...");
                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            // 1. StatusSync handles complex hardware logic (Enable/Disable/Delete)
                            // and has a built-in PendingJob queue for offline devices.
                            await _deviceFunctions.StatusSync(data);

                            // 2. Perform the final local database status toggle
                            // This ensures the local 'Status' column matches the Cloud 'status' string.
                            int memberId = data["id"]?.Value<int>() ?? 0;
                            string newStatus = data["status"]?.Value<string>() ?? string.Empty;

                            if (memberId > 0 && !string.IsNullOrEmpty(newStatus))
                            {
                                await DatabaseQueueManager.EnterDbWriteLockAsync("Processor_MemberStatus_Update", DbTaskPriority.High);
                                try
                                {
                                    _dbHelper.UpdateMemberStatus(memberId, newStatus);
                                }
                                finally
                                {
                                    DatabaseQueueManager.ReleaseDbWriteLock("Processor_MemberStatus_Update");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"❌ Error processing member_status event: {ex.Message}");
                        }
                    });

                    ShowUsersNotification(data, "Member Status Changed", "A member's status has been synchronized.");
                    break;
                default:
                    LogHelper.Log($"⚠️ Unhandled event type: {eventType}");
                    break;
            }
        }


        /// <summary>
        /// Central entry point for handling all HTTP events.
        /// Routes each event type to the appropriate internal method.
        /// </summary>
        public async Task HandleHttpEvent(string eventType, JObject data)
        {
            switch (eventType)
            {
                case "attendance.response":
                    ShowUsersNotification(data, "Attendance Response", "Attendance response received from server.");
                    HandleHttpAttendanceResponseSingle(data);
                    break;

                case "attendance.response.bulk":
                    ShowUsersNotification(data, "Bulk Attendance Response", "Bulk attendance response received from server.");
                    HandleHttpAttendanceResponseBulk(data);
                    break;

                case "member.all":
                    var membersArray = data["members"] as JArray;
                    if (membersArray == null)
                    {
                        LogHelper.Log("❌ 'members' array not found in HTTP response for 'member.all'.");
                        return;
                    }

                    // StatusSync and HandleAllMembers expect a JObject with a "members" property.
                    // The incoming 'data' object already has the correct structure.
                    await _deviceFunctions.StatusSync(data);

                    // Save synced_at from the root
                    if (data["synced_at"] != null)
                    {
                        var lastSyncIso = data["synced_at"]?.ToString();
                        var lastSync = CommonUtils.FormatSyncedAtUtcCompact(lastSyncIso) ?? lastSyncIso;
                        _dbHelper.SaveSetting("last_sync", lastSync);
                        LogHelper.Log($"💾 Saved last_sync: {lastSync}");
                    }

                    await HandleHttpAllMembers(data);
                    break;

                default:
                    LogHelper.Log($"⚠️ Unhandled HTTP event type: {eventType}");
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
                // Route staff to a dedicated handler BEFORE any int parsing
                bool isStaff = member["is_staff"]?.Type != JTokenType.Null && member["is_staff"]?.Value<bool>() == true;
                if (isStaff)
                {
                    // Staff IDs are GUIDs; avoid int casts
                    LogHelper.Log($"ℹ️ Routing staff member to SyncStaffFromCloud.");
                    SyncStaffFromCloud(member);
                    return;
                }

                // Safely parse memberId (must be integer for members)
                if (member["id"] == null || member["id"].Type != JTokenType.Integer)
                {
                    LogHelper.Log("⚠️ Member 'id' is missing or not an integer. Skipping non-staff record.");
                    return;
                }
                int memberId = member["id"].Value<int>();

                string status = member["status"]?.Value<string>() ?? string.Empty;

                int? enrollmentId = null;
                if (member["enrollment_id"] != null && member["enrollment_id"].Type == JTokenType.Integer)
                    enrollmentId = member["enrollment_id"].Value<int>();

                // If face == false, ensure picture is null so no image processing occurs
                bool hasFace = member["face"]?.Type != JTokenType.Null && member["face"]?.Value<bool>() == true;
                if (!hasFace && member["picture"] != null)
                {
                    member["picture"] = JValue.CreateNull();
                }

                if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase) ||
                    status.Equals("Expired", StringComparison.OrdinalIgnoreCase))
                {
                    _dbHelper.DeleteMemberById(memberId);
                    LogHelper.Log($"🗑 Deleted member {memberId} from local DB.");

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

                _dbHelper.UpsertMember(member);
                LogHelper.Log($"✅ Synced user {memberId} with enrollment_id {enrollmentId}.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ SyncUserFromCloud error: {ex.Message}");
            }
        }

        // Dedicated staff handler: upsert into Staff table and normalize picture by face flag
        private async void SyncStaffFromCloud(JObject staff)
        {
            try
            {
                string staffId = staff["id"]?.ToString();
                string status = staff["status"]?.Value<string>() ?? string.Empty;
                int? enrollmentId = staff["enrollment_id"]?.Type == JTokenType.Integer ? staff["enrollment_id"].Value<int>() : (int?)null;

                bool hasFace = staff["face"]?.Type != JTokenType.Null && staff["face"]?.Value<bool>() == true;
                if (!hasFace && staff["picture"] != null)
                {
                    staff["picture"] = JValue.CreateNull();
                }

                LogHelper.Log($"👤 Handling staff member {staffId} (status={status})");

                // Check if this is a new staff member
                bool isNewStaff = _dbHelper.GetStaff(staffId) == null;

                // Update enrollment IDs list only for add or remove operations
                if (enrollmentId.HasValue)
                {
                    string key = "staff_enrollment_ids";
                    string currentJson = _dbHelper.GetSetting(key) ?? "[]";
                    List<string> enrollmentIds;
                    try
                    {
                        enrollmentIds = JsonConvert.DeserializeObject<List<string>>(currentJson) ?? new List<string>();
                    }
                    catch
                    {
                        enrollmentIds = new List<string>();
                    }
                    string enrollmentIdStr = enrollmentId.Value.ToString();
                    bool listChanged = false;

                    if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase) ||
                        status.Equals("Expired", StringComparison.OrdinalIgnoreCase))
                    {
                        if (enrollmentIds.Remove(enrollmentIdStr))
                        {
                            listChanged = true;
                            LogHelper.Log($"Removed enrollment ID {enrollmentIdStr} from staff enrollment IDs list.");
                        }
                    }
                    else if (isNewStaff)
                    {
                        if (!enrollmentIds.Contains(enrollmentIdStr))
                        {
                            enrollmentIds.Add(enrollmentIdStr);
                            listChanged = true;
                            LogHelper.Log($"Added enrollment ID {enrollmentIdStr} to staff enrollment IDs list.");
                        }
                    }

                    if (listChanged)
                    {
                        _dbHelper.SaveJsonArraySetting(key, enrollmentIds);
                    }
                }

                if (status.Equals("Deleted", StringComparison.OrdinalIgnoreCase) ||
                    status.Equals("Expired", StringComparison.OrdinalIgnoreCase))
                {
                    // Face deletion API expects int memberId; skip for staff GUIDs
                    _ = Task.Run(() =>
                    {
                        LogHelper.Log($"ℹ️ Skipping face image delete for staff {staffId} (non-integer ID).");
                        return Task.CompletedTask;
                    });

                    // Delete staff by string ID in DB helper (ensure DeleteStaffById accepts string)
                    _dbHelper.DeleteStaffById(staffId);

                    LogHelper.Log($"🗑️ Staff {staffId} marked as {status}.");
                    return;
                }

                // Persist staff to Staff table and compute device sync placeholders
                _dbHelper.UpsertStaff(staff);
                LogHelper.Log($"✅ Synced staff {staffId}.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ SyncStaffFromCloud error: {ex.Message}");
            }
        }

        /// <summary>
        /// Handles member face image management before member creation using WebSocketManager functionality.
        /// Checks FACE directory for existing image, deletes if found, and downloads new image if available.
        /// </summary>
        /// <param name="memberData">Member data containing face image information</param>
        private async Task HandleUsersFaceImage(JObject memberData)
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
        /// Handles attendance update events from WebSocket, only processing manual updates.
        /// Supports staff (GUID id + enrollment_id) and regular members (int id).
        /// </summary>
        private async void HandleAttendance(JObject data)
        {
            try
            {
                var attendance = data["attendance"] as JObject;
                if (attendance == null) return;

                bool isManual = attendance["is_manual"]?.Value<bool>() ?? false;
                if (!isManual)
                {
                    LogHelper.Log("ℹ️ Skipping non-manual attendance update.");
                    return;
                }

                string attendanceId = attendance["id"]?.ToString();
                var memberObj = attendance["member"] as JObject;
                if (memberObj == null)
                {
                    LogHelper.Log("⚠️ Attendance payload missing member object.");
                    return;
                }

                // Determine if this is a staff record
                bool isStaff = memberObj["is_staff"]?.Type != JTokenType.Null && memberObj["is_staff"]?.Value<bool>() == true;

                string checkInStr = attendance["check_in"]?.ToString();
                string checkOutStr = attendance["check_out"]?.ToString();

                bool hasCheckIn = !string.IsNullOrWhiteSpace(checkInStr);
                bool hasCheckOut = !string.IsNullOrWhiteSpace(checkOutStr);

                bool anyUpdateDone = false;
                DateTime when;

                await DatabaseQueueManager.EnterDbWriteLockAsync("HandleAttendanceUpdated_Write", DbTaskPriority.High);
                try
                {
                    if (isStaff)
                    {
                        // Staff: use enrollment_id (int). Staff primary id is GUID string and should NOT be parsed as int.
                        int enrollmentId = memberObj["enrollment_id"]?.Type == JTokenType.Integer
                            ? memberObj["enrollment_id"].Value<int>()
                            : 0;

                        if (enrollmentId == 0 || string.IsNullOrWhiteSpace(attendanceId))
                        {
                            LogHelper.Log("⚠️ Invalid STAFF attendance payload (missing enrollment_id or id).");
                        }
                        else
                        {
                            if (hasCheckIn && DateTime.TryParse(checkInStr, out when))
                            {
                                LogHelper.Log($"Processed manual STAFF attendance update for enrollment {enrollmentId} with check-in at {when:yyyy-MM-dd HH:mm:ss} and AttendanceId {attendanceId}");
                                bool updated = _dbHelper.UpdateStaffAttendanceLogSyncStatus(
                                    enrollmentId,
                                    attendanceId,
                                    checkInTime: when,
                                    checkOutTime: null,
                                    checkIn: true,
                                    checkOut: null
                                );
                                LogHelper.Log(updated
                                    ? $"✅ Updated STAFF sync status for check-in: Enrollment {enrollmentId}, AttendanceId {attendanceId}"
                                    : $"ℹ️ No change for STAFF check-in sync (already up-to-date or row not found): Enrollment {enrollmentId}, AttendanceId {attendanceId}");
                                anyUpdateDone |= updated;
                            }

                            if (hasCheckOut && DateTime.TryParse(checkOutStr, out when))
                            {
                                LogHelper.Log($"Updated manual STAFF attendance update for enrollment {enrollmentId} with check-out at {when:yyyy-MM-dd HH:mm:ss} using AttendanceId {attendanceId}");
                                bool updated = _dbHelper.UpdateStaffAttendanceLogSyncStatus(
                                    enrollmentId,
                                    attendanceId,
                                    checkInTime: null,
                                    checkOutTime: when,
                                    checkIn: null,
                                    checkOut: true
                                );
                                LogHelper.Log(updated
                                    ? $"✅ Updated STAFF sync status for check-out: Enrollment {enrollmentId}, AttendanceId {attendanceId}"
                                    : $"ℹ️ No change for STAFF check-out sync (already up-to-date or row not found): Enrollment {enrollmentId}, AttendanceId {attendanceId}");
                                anyUpdateDone |= updated;
                            }
                        }
                    }
                    else
                    {
                        // Member: id must be integer
                        int memberId = memberObj["id"]?.Type == JTokenType.Integer
                            ? memberObj["id"].Value<int>()
                            : 0;

                        if (memberId == 0 || string.IsNullOrWhiteSpace(attendanceId))
                        {
                            LogHelper.Log("⚠️ Invalid member attendance record (missing member_id or id). Skipping.");
                        }
                        else
                        {
                            if (hasCheckIn && DateTime.TryParse(checkInStr, out when))
                            {
                                LogHelper.Log($"Processed manual attendance update for member {memberId} with check-in at {when:yyyy-MM-dd HH:mm:ss} and AttendanceId {attendanceId}");
                                bool updated = _dbHelper.UpdateAttendanceLogSyncStatus(
                                    memberId,
                                    attendanceId,
                                    checkInTime: when,
                                    checkOutTime: null,
                                    checkIn: true,
                                    checkOut: null
                                );
                                LogHelper.Log(updated
                                    ? $"✅ Updated sync status for check-in: Member {memberId}, AttendanceId {attendanceId}"
                                    : $"ℹ️ No change for check-in sync (already up-to-date or row not found): Member {memberId}, AttendanceId {attendanceId}");
                                anyUpdateDone |= updated;
                            }

                            if (hasCheckOut && DateTime.TryParse(checkOutStr, out when))
                            {
                                LogHelper.Log($"Updated manual attendance update for member {memberId} with check-out at {when:yyyy-MM-dd HH:mm:ss} using AttendanceId {attendanceId}");
                                bool updated = _dbHelper.UpdateAttendanceLogSyncStatus(
                                    memberId,
                                    attendanceId,
                                    checkInTime: null,
                                    checkOutTime: when,
                                    checkIn: null,
                                    checkOut: true
                                );
                                LogHelper.Log(updated
                                    ? $"✅ Updated sync status for check-out: Member {memberId}, AttendanceId {attendanceId}"
                                    : $"ℹ️ No change for check-out sync (already up-to-date or row not found): Member {memberId}, AttendanceId {attendanceId}");
                                anyUpdateDone |= updated;
                            }
                        }
                    }
                }
                finally
                {
                    DatabaseQueueManager.ReleaseDbWriteLock("HandleAttendanceUpdated_Write");
                }

                if (anyUpdateDone)
                {
                    if (isStaff)
                        LogHelper.Log($"✅ Manual STAFF attendance update sync completed for enrollment {memberObj["enrollment_id"]?.ToString() ?? "unknown"}, AttendanceId {attendanceId}");
                    else
                        LogHelper.Log($"✅ Manual attendance update sync completed for member {memberObj["id"]?.ToString() ?? "unknown"}, AttendanceId {attendanceId}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ HandleAttendanceUpdated error: {ex.Message}");
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
                await DatabaseQueueManager.EnterDbWriteLockAsync("HandleDeletedMember_Cleanup", DbTaskPriority.High);
                try
                {
                    _dbHelper.DeleteMemberById(memberId);
                    bool logsDeleted = _dbHelper.DeleteMemberLogs(memberId);
                    LogHelper.Log(logsDeleted
                        ? $"✅ Successfully deleted logs for member {memberId}"
                        : $"ℹ️ No logs found to delete for member {memberId}");
                }
                finally
                {
                    DatabaseQueueManager.ReleaseDbWriteLock("HandleDeletedMember_Cleanup");
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



        private async Task HandleUpdateUsers(JObject data)
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


        private async void HandleUsersCreation()
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



        ///<summary>
        /// Shows a notification for a member-related event.
        ///</summary>
        private void ShowUsersNotification(JObject data, string title, string defaultMessage)
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







        ////////////////////////////////HTTP Methods//////////////////////////////////////


        private async Task HandleHttpAllMembers(JObject data)
        {
            try
            {
                LogHelper.Log($"📦 Received all members data : {data.ToString()}");

                var membersArray = data["members"] as JArray;
                if (membersArray == null)
                {
                    LogHelper.Log("❌ 'members' array not found in response.");
                    return;
                }

                // Build a transformed payload:
                // - Skip staff
                // - Preserve only picture when face == true; otherwise set picture to null
                // This ensures SaveMemberPicturesAsync only downloads images for face-enabled members.
                var transformedMembers = new JArray();
                foreach (var memberToken in membersArray)
                {
                    var member = memberToken as JObject;
                    if (member == null) continue;

                    bool hasFace = member["face"]?.Type != JTokenType.Null && member["face"]?.Value<bool>() == true;

                    // Clone member to avoid mutating original input
                    var memberClone = (JObject)member.DeepClone();

                    // If face is false, nullify picture so downloader won’t process it
                    if (!hasFace)
                    {
                        if (memberClone["picture"] != null)
                            memberClone["picture"] = JValue.CreateNull();
                    }

                    transformedMembers.Add(memberClone);
                }

                // Create a new envelope for SaveMemberPicturesAsync
                var picturesEnvelope = new JObject
                {
                    ["members"] = transformedMembers
                };

                // Use reflection to call the private SaveMemberPicturesAsync method
                try
                {
                    var saveMethod = typeof(WebSocketManager).GetMethod(
                        "SaveMemberPicturesAsync",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

                    if (saveMethod != null)
                    {
                        await (Task)saveMethod.Invoke(_wsManager, new object[] { picturesEnvelope });
                        LogHelper.Log("🖼️ Queued member picture download process for face-enabled, non-staff members.");
                    }
                    else
                    {
                        LogHelper.Log("❌ Could not find SaveMemberPicturesAsync method to download images.");
                    }
                }
                catch (Exception exImg)
                {
                    LogHelper.Log($"❌ Error invoking SaveMemberPicturesAsync: {exImg.Message}");
                }

                int upsertedOrSynced = 0;
                int deletedOrExpired = 0;

                // Process sync/upsert/delete for non-staff members only.
                foreach (var memberObj in transformedMembers)
                {
                    try
                    {
                        var member = (JObject)memberObj;
                        var status = member["status"]?.Value<string>() ?? string.Empty;

                        // Reuse existing logic (SyncUserFromCloud itself skips staff, but we filtered already)
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

                LogHelper.Log($"✅ {upsertedOrSynced} members synced/upserted (non-staff), 🗑 {deletedOrExpired} deleted/expired.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ HandleAllMembers error: {ex.Message}");
            }
        }

        public async void HandleHttpAttendanceResponseSingle(JObject data)
        {
            try
            {
                LogHelper.Log($"📍 Attendance single response received: {data.ToString(Newtonsoft.Json.Formatting.None)}");

                // Check if the data contains a 'records' array and extract the first record
                JObject record = data;
                if (data["records"] is JArray records && records.Count > 0)
                {
                    record = (JObject)records[0];
                }

                string attendanceId = record["id"]?.ToString();
                bool isStaff = record["is_staff"]?.Type != JTokenType.Null && record["is_staff"]?.Value<bool>() == true;

                if (isStaff)
                {
                    // For staff, prefer explicit enrollment_id, fall back to member_id if present
                    int enrollmentId = record["enrollment_id"]?.Value<int>() ?? record["member_id"]?.Value<int>() ?? 0;
                    if (enrollmentId == 0 || string.IsNullOrWhiteSpace(attendanceId))
                    {
                        LogHelper.Log("⚠️ Invalid single STAFF attendance payload (missing enrollment_id or id).");
                        return;
                    }

                    string checkInIso = record["check_in"]?.ToString() ?? record["checkin_time"]?.ToString();
                    string checkOutIso = record["check_out"]?.ToString() ?? record["checkout_time"]?.ToString();

                    DateTime checkInLocal; string checkInFmt;
                    DateTime checkOutLocal; string checkOutFmt;

                    bool hasCheckIn = CommonUtils.TryParseIsoToLocal(checkInIso, out checkInLocal, out checkInFmt);
                    bool hasCheckOut = CommonUtils.TryParseIsoToLocal(checkOutIso, out checkOutLocal, out checkOutFmt);

                    bool anyUpdateDone = false;

                    await DatabaseQueueManager.EnterDbWriteLockAsync("HandleAttendanceResponseSingle_Write_Staff", DbTaskPriority.High);
                    try
                    {
                        if (hasCheckIn)
                        {
                            LogHelper.Log($"Processed STAFF attendance log for enrollment {enrollmentId} with check-in at {checkInFmt} and AttendanceId {attendanceId}");
                            bool updated = _dbHelper.UpdateStaffAttendanceLogSyncStatus(
                                enrollmentId,
                                attendanceId,
                                checkInTime: checkInLocal,
                                checkOutTime: null,
                                checkIn: true,
                                checkOut: null
                            );
                            LogHelper.Log(updated
                                ? $"✅ Updated STAFF sync status for check-in: Enrollment {enrollmentId}, AttendanceId {attendanceId}"
                                : $"ℹ️ No change for STAFF check-in sync (already up-to-date or row not found): Enrollment {enrollmentId}, AttendanceId {attendanceId}");
                            anyUpdateDone |= updated;
                        }

                        if (hasCheckOut)
                        {
                            LogHelper.Log($"Updated STAFF attendance log for enrollment {enrollmentId} with check-out at {checkOutFmt} using AttendanceId {attendanceId}");
                            bool updated = _dbHelper.UpdateStaffAttendanceLogSyncStatus(
                                enrollmentId,
                                attendanceId,
                                checkInTime: null,
                                checkOutTime: checkOutLocal,
                                checkIn: null,
                                checkOut: true
                            );
                            LogHelper.Log(updated
                                ? $"✅ Updated STAFF sync status for check-out: Enrollment {enrollmentId}, AttendanceId {attendanceId}"
                                : $"ℹ️ No change for STAFF check-out sync (already up-to-date or row not found): Enrollment {enrollmentId}, AttendanceId {attendanceId}");
                            anyUpdateDone |= updated;
                        }
                    }
                    finally
                    {
                        DatabaseQueueManager.ReleaseDbWriteLock("HandleAttendanceResponseSingle_Write_Staff");
                    }

                    if (anyUpdateDone)
                        LogHelper.Log($"✅ STAFF Attendance sync completed for enrollment {enrollmentId}, AttendanceId {attendanceId}");

                    return;
                }

                // Member (non-staff) branch
                int memberId = record["member_id"]?.Value<int>() ?? 0;
                if (memberId == 0 || string.IsNullOrWhiteSpace(attendanceId))
                {
                    LogHelper.Log("⚠️ Invalid single attendance payload (missing member_id or id).");
                    return;
                }

                string checkInIsoM = record["check_in"]?.ToString() ?? record["checkin_time"]?.ToString();
                string checkOutIsoM = record["check_out"]?.ToString() ?? record["checkout_time"]?.ToString();

                DateTime checkInLocalM; string checkInFmtM;
                DateTime checkOutLocalM; string checkOutFmtM;

                bool hasCheckInM = CommonUtils.TryParseIsoToLocal(checkInIsoM, out checkInLocalM, out checkInFmtM);
                bool hasCheckOutM = CommonUtils.TryParseIsoToLocal(checkOutIsoM, out checkOutLocalM, out checkOutFmtM);

                bool anyUpdateDoneM = false;

                await DatabaseQueueManager.EnterDbWriteLockAsync("HandleAttendanceResponseSingle_Write", DbTaskPriority.High);
                try
                {
                    if (hasCheckInM)
                    {
                        LogHelper.Log($"Processed attendance log for member {memberId} with check-in at {checkInFmtM} and AttendanceId {attendanceId}");
                        bool updated = _dbHelper.UpdateAttendanceLogSyncStatus(
                            memberId,
                            attendanceId,
                            checkInTime: checkInLocalM,
                            checkOutTime: null,
                            checkIn: true,
                            checkOut: null
                        );
                        LogHelper.Log(updated
                            ? $"✅ Updated sync status for check-in: Member {memberId}, AttendanceId {attendanceId}"
                            : $"ℹ️ No change for check-in sync (already up-to-date or row not found): Member {memberId}, AttendanceId {attendanceId}");
                        anyUpdateDoneM |= updated;
                    }

                    if (hasCheckOutM)
                    {
                        LogHelper.Log($"Updated attendance log for member {memberId} with check-out at {checkOutFmtM} using AttendanceId {attendanceId}");
                        bool updated = _dbHelper.UpdateAttendanceLogSyncStatus(
                            memberId,
                            attendanceId,
                            checkInTime: null,
                            checkOutTime: checkOutLocalM,
                            checkIn: null,
                            checkOut: true
                        );
                        LogHelper.Log(updated
                            ? $"✅ Updated sync status for check-out: Member {memberId}, AttendanceId {attendanceId}"
                            : $"ℹ️ No change for check-out sync (already up-to-date or row not found): Member {memberId}, AttendanceId {attendanceId}");
                        anyUpdateDoneM |= updated;
                    }
                }
                finally
                {
                    DatabaseQueueManager.ReleaseDbWriteLock("HandleAttendanceResponseSingle_Write");
                }

                if (anyUpdateDoneM)
                    LogHelper.Log($"✅ Attendance sync completed for member {memberId}, AttendanceId {attendanceId}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ HandleAttendanceResponseSingle error: {ex.Message}");
            }
        }


        /// <summary>
        /// Handles bulk attendance response from the cloud.
        /// <summary>

        public async void HandleHttpAttendanceResponseBulk(JObject data)
        {
            try
            {
                LogHelper.Log($"📍 Attendance bulk response received: {data.ToString(Newtonsoft.Json.Formatting.None)}");

                var records = data["records"] as JArray;
                if (records == null || records.Count == 0)
                {
                    LogHelper.Log("⚠️ Bulk attendance response has no 'records'.");
                    return;
                }

                int totalUpdated = 0;

                // We process the records and call the bulk DB update methods which handle their own transactions and logging
                foreach (var recToken in records)
                {
                    var rec = recToken as JObject;
                    if (rec == null) continue;

                    string attendanceId = rec["id"]?.ToString();
                    bool isStaff = rec["is_staff"]?.Type != JTokenType.Null && rec["is_staff"]?.Value<bool>() == true;

                    // support both naming conventions used across single/bulk payloads
                    string checkInIso = rec["checkin_time"]?.ToString() ?? rec["check_in"]?.ToString();
                    string checkOutIso = rec["checkout_time"]?.ToString() ?? rec["check_out"]?.ToString();

                    DateTime checkInLocal; string checkInFmt;
                    DateTime checkOutLocal; string checkOutFmt;

                    bool hasCheckIn = CommonUtils.TryParseIsoToLocal(checkInIso, out checkInLocal, out checkInFmt);
                    bool hasCheckOut = CommonUtils.TryParseIsoToLocal(checkOutIso, out checkOutLocal, out checkOutFmt);

                    if (!hasCheckIn)
                    {
                        LogHelper.Log($"⚠️ Skipping record in bulk response: Missing or invalid check-in time for ID {attendanceId}");
                        continue;
                    }

                    if (isStaff)
                    {
                        int enrollmentId = rec["enrollment_id"]?.Value<int>() ?? rec["member_id"]?.Value<int>() ?? 0;
                        if (enrollmentId == 0 || string.IsNullOrWhiteSpace(attendanceId))
                        {
                            LogHelper.Log("⚠️ Invalid STAFF record in bulk (missing enrollment_id or id). Skipping.");
                            continue;
                        }

                        // Use the bulk helper which looks up the record by enrollment + check-in time
                        bool success = _dbHelper.UpdateStaffAttendanceLogBulk(
                            enrollmentId,
                            checkInLocal,
                            hasCheckOut ? (DateTime?)checkOutLocal : null,
                            attendanceId,
                            setCheckInSynced: true,
                            setCheckOutSynced: hasCheckOut
                        );

                        if (success) totalUpdated++;
                    }
                    else
                    {
                        int memberId = rec["member_id"]?.Value<int>() ?? 0;
                        if (memberId == 0 || string.IsNullOrWhiteSpace(attendanceId))
                        {
                            LogHelper.Log("⚠️ Invalid member record in bulk (missing member_id or id). Skipping.");
                            continue;
                        }

                        // Use the bulk helper which looks up the record by memberId + check-in time
                        bool success = _dbHelper.UpdateAttendanceLogBulkMembers(
                            memberId,
                            checkInLocal,
                            hasCheckOut ? (DateTime?)checkOutLocal : null,
                            attendanceId,
                            setCheckInSynced: true,
                            setCheckOutSynced: hasCheckOut
                        );

                        if (success) totalUpdated++;
                    }
                }

                if (totalUpdated > 0)
                    LogHelper.Log($"✅ Bulk attendance status update completed. Successfully updated {totalUpdated}/{records.Count} records.");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ HandleHttpAttendanceResponseBulk error: {ex.Message}");
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////


        public void update_session()
        {
            try
            {
                List<Device> devices = _dbHelper.LoadDevices();
                AppSession.DeviceList = devices;


                List<Device> staffDevices = _dbHelper.LoadStaffDevices();
                AppSession.StaffDeviceList = staffDevices;

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading devices: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }

}
