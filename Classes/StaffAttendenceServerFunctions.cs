using smart_attendance_middleware.Classes;
using System;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public class StaffAttendenceServerFunctions 
    {
        public DatabaseHelper _dbHelper;
        private Processor _processor; 
        private HttpHandler _httpHandler;

        public StaffAttendenceServerFunctions()
        {
            _dbHelper = new DatabaseHelper();
            _processor = new Processor(_dbHelper, null, this); 
            _httpHandler = new HttpHandler(_dbHelper, _processor);
        }

        public sealed class StaffAttendanceRecord
        {
            public int DevID { get; set; }
            public string IP { get; set; }
            public int Enroll { get; set; }
            public DateTime Time { get; set; } // Device (source) timestamp
        }

        private sealed class StaffAttendanceEventResult
        {
            public bool Success { get; set; }
            public string StaffId { get; set; } 
            public int EnrollmentId { get; set; }
            public string Action { get; set; }
            public DateTime EventTime { get; set; } // Echo of device timestamp
        }



        // Process method for staff devices 
        public virtual async void Process(StaffAttendanceRecord record)
        {
            if (record == null) return;

            // Use ONLY record.Time for all persistence & UI
            DateTime deviceTime = record.Time;

            LogHelper.Log(string.Format("[StaffAttendanceProcess] MachineNo={0}, IP={1}, Enroll={2}, Time={3:yyyy-MM-dd HH:mm:ss}",
                record.DevID, record.IP, record.Enroll, deviceTime));

            if (_dbHelper == null)
                _dbHelper = new DatabaseHelper();

            // Resolve action and staffId once and reuse them
            string resolvedAction = null;
            string resolvedStaffId = null;

            try
            {
                resolvedAction = _dbHelper.GetStaffDeviceAttendanceAction(record.DevID);
                resolvedStaffId = _dbHelper.GetStaffIdByEnrollment(record.Enroll);

                if (string.IsNullOrEmpty(resolvedAction) || string.IsNullOrEmpty(resolvedStaffId))
                {
                    LogHelper.Log("[StaffAttendanceProcess] Missing action or staff id; skipping debounce and handling.");
                }
                else
                {
                    // If device reports mixed mode (e.g. "check-in/check-out"), resolve to an effective action
                    // based on current staff attendance state in DB.
                    if (resolvedAction.IndexOf("/", StringComparison.OrdinalIgnoreCase) >= 0
                        || resolvedAction.Equals("check-in/check-out", StringComparison.OrdinalIgnoreCase)
                        || resolvedAction.Equals("check_in/check_out", StringComparison.OrdinalIgnoreCase)
                        || resolvedAction.Equals("both", StringComparison.OrdinalIgnoreCase))
                    {
                        var effective = _dbHelper.GetEffectiveStaffActionForDevice(record.DevID, record.Enroll, deviceTime);
                        if (!string.IsNullOrEmpty(effective))
                        {
                            LogHelper.Log($"[StaffAttendanceProcess] Device supports both; resolved effective action='{effective}' for Enroll={record.Enroll}");
                            resolvedAction = effective;
                        }
                        else
                        {
                            LogHelper.Log($"[StaffAttendanceProcess] Could not resolve effective action for mixed-mode device. Will proceed to handler and let DB helpers decide.");
                        }
                    }

                    LogHelper.Log($"[StaffAttendanceProcess] Staff device action='{resolvedAction}', StaffID='{resolvedStaffId}', Enroll={record.Enroll}");
                }
            }
            catch (Exception exDebounce)
            {
                LogHelper.Log($"[StaffAttendanceProcess][PreHandle][ERROR] {exDebounce.Message}");
            }

            // --- Debounce logic (mirrors AttendenceServerFunctions.Process behavior) ---
            try
            {
                // Read debounce seconds from settings; if invalid, prompt the user and stop
                var s = _dbHelper.GetSetting("AttendanceDebounceSeconds");
                if (!int.TryParse(s, out var debounceSeconds) || debounceSeconds < 0)
                {
                    MessageBox.Show(
                        "Debounce setting is missing or invalid. Please set it in Device Manager and try again.",
                        "Debounce Setting Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    LogHelper.Log("[StaffAttendanceProcess][Debounce] Missing/invalid debounce setting. Aborting event processing.");
                    return; // do not process further until a valid value is saved
                }

                if (string.IsNullOrEmpty(resolvedAction) || string.IsNullOrEmpty(resolvedStaffId))
                {
                    LogHelper.Log("[StaffAttendanceProcess] Debounce check skipped (missing action or staffId)");
                }
                else
                {
                    // Normalize action string
                    string action = resolvedAction;

                    if (action.Equals("check-out", StringComparison.OrdinalIgnoreCase))
                    {
                        // Try to find a paired/open check-in for this staff at this checkout time.
                        DateTime? pairedCheckIn = null;
                        try
                        {
                            pairedCheckIn = _dbHelper.GetPairedCheckInTimeForStaff(resolvedStaffId, deviceTime);
                        }
                        catch (Exception exPair)
                        {
                            LogHelper.Log($"[StaffAttendanceProcess][Debounce] Failed to get paired check-in for staff {resolvedStaffId}: {exPair.Message}");
                        }

                        if (pairedCheckIn.HasValue)
                        {
                            var diff = deviceTime - pairedCheckIn.Value;
                            if (diff.TotalSeconds < debounceSeconds)
                            {
                                LogHelper.Log($"[StaffAttendanceProcess][Debounce] STAFF CHECK-OUT ignored (Δ={diff.TotalSeconds:F0}s < {debounceSeconds}s) Staff={resolvedStaffId} Enroll={record.Enroll} Time={deviceTime:yyyy-MM-dd HH:mm:ss}");
                                return; // exit early (debounced)
                            }
                        }
                        else
                        {
                            LogHelper.Log("[StaffAttendanceProcess][Debounce] No paired/open session for staff checkout; passing to handler.");
                        }
                    }
                    else if (action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                    {
                        // Determine if staff currently has an open session.
                        bool openSessionExists = false;
                        DateTime? lastCheckOut = null;

                        try
                        {
                            // Attempt to detect open session and last checkout by scanning staff attendance logs.
                            var logs = _dbHelper.GetAllStaffAttendanceLogs();
                            if (logs != null)
                            {
                                foreach (var lr in logs)
                                {
                                    try
                                    {
                                        // Safe dynamic access - property names commonly: StaffID, CheckInTime, CheckOutTime
                                        string sid = null;
                                        try { sid = Convert.ToString(lr.StaffID); } catch { try { sid = Convert.ToString(lr.StaffId); } catch { sid = null; } }

                                        if (string.IsNullOrEmpty(sid) || !sid.Equals(resolvedStaffId, StringComparison.OrdinalIgnoreCase))
                                            continue;

                                        // open session if CheckOutTime is null/empty
                                        DateTime? co = null;
                                        try
                                        {
                                            // attempt direct cast
                                            co = lr.CheckOutTime as DateTime?;
                                        }
                                        catch
                                        {
                                            // fallback to parsing string
                                            try
                                            {
                                                var coStr = Convert.ToString(lr.CheckOutTime);
                                                if (DateTime.TryParse(coStr, out DateTime tmpCo))
                                                    co = tmpCo;
                                                else
                                                    co = null;
                                            }
                                            catch
                                            {
                                                co = null;
                                            }
                                        }

                                        if (!co.HasValue)
                                            openSessionExists = true;
                                        else
                                        {
                                            if (!lastCheckOut.HasValue || co.Value > lastCheckOut.Value)
                                                lastCheckOut = co.Value;
                                        }
                                    }
                                    catch
                                    {
                                        // ignore malformed rows
                                    }
                                }
                            }
                        }
                        catch (Exception exLogs)
                        {
                            LogHelper.Log($"[StaffAttendanceProcess][Debounce] Failed to scan staff logs for debounce: {exLogs.Message}");
                        }

                        if (!openSessionExists)
                        {
                            // Completed session: apply hard-coded short debounce (10s) against last check-out
                            const int completedSessionCheckInDebounceSeconds = 10;

                            if (lastCheckOut.HasValue)
                            {
                                var diff = deviceTime - lastCheckOut.Value;
                                if (diff.TotalSeconds < completedSessionCheckInDebounceSeconds)
                                {
                                    LogHelper.Log(
                                        $"[StaffAttendanceProcess][Debounce] STAFF CHECK-IN ignored after completed session " +
                                        $"(Δ={diff.TotalSeconds:F0}s < {completedSessionCheckInDebounceSeconds}s) " +
                                        $"Staff={resolvedStaffId} Enroll={record.Enroll} Time={deviceTime:yyyy-MM-dd HH:mm:ss}");
                                    return;
                                }
                            }
                            else
                            {
                                LogHelper.Log(
                                    $"[StaffAttendanceProcess][Debounce] STAFF CHECK-IN completed-session debounce check skipped (no last checkout). " +
                                    $"Staff={resolvedStaffId} Enroll={record.Enroll} Time={deviceTime:yyyy-MM-dd HH:mm:ss}");
                            }
                        }
                        else
                        {
                            // There is an open session; apply configurable debounce (AttendanceDebounceSeconds) against last check-out
                            if (lastCheckOut.HasValue)
                            {
                                var diff = deviceTime - lastCheckOut.Value;
                                if (diff.TotalSeconds < debounceSeconds)
                                {
                                    LogHelper.Log(
                                        $"[StaffAttendanceProcess][Debounce] STAFF CHECK-IN ignored (Δ={diff.TotalSeconds:F0}s < {debounceSeconds}s) " +
                                        $"Staff={resolvedStaffId} Enroll={record.Enroll} Time={deviceTime:yyyy-MM-dd HH:mm:ss}");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exDebounce)
            {
                LogHelper.Log($"[StaffAttendanceProcess][Debounce][ERROR] {exDebounce.Message}");
                // Fail-open: proceed to normal handling
            }

            StaffAttendanceEventResult result = await HandleStaffAttendanceEvent(record.DevID, record.Enroll, deviceTime, resolvedAction, resolvedStaffId);

            if (result != null && result.Success)
            {
                ShowFacePopupForStaff(result.StaffId, result.EnrollmentId, result.Action, result.EventTime);
                await LogStaffAttendanceToServer(result);
            }
        }

        // Basic handler that inserts/updates staff attendance log records.
        // Adapted to handle string StaffID (GUID) by using staff-specific DB helpers.
        // Accepts pre-resolved action and staffId to avoid redundant DB lookups.
        private async Task<StaffAttendanceEventResult> HandleStaffAttendanceEvent(int machineNo, int enrollmentId, DateTime deviceTime, string preResolvedAction = null, string preResolvedStaffId = null)
        {
            try
            {
                // Use passed-in action if provided; 
                string action = preResolvedAction;
                if (action == null)
                {
                    LogHelper.Log($"[StaffAttendanceProcess] Unknown MachineNo {machineNo}. Ignored.");
                    return null;
                }

                // Use passed-in staffId if provided; otherwise resolve from DB
                string staffId = preResolvedStaffId ?? _dbHelper.GetStaffIdByEnrollment(enrollmentId);
                if (string.IsNullOrEmpty(staffId))
                {
                    LogHelper.Log($"[StaffAttendanceProcess] EnrollmentID {enrollmentId} not linked to any staff. Ignored.");
                    return null;
                }

                string deviceTimeStr = deviceTime.ToString("yyyy-MM-dd HH:mm:ss");

                // Use action-aware staff helpers:
                // - For "check-in" use InsertStaffCheckInByStaffId(staffId, time)
                // - For "check-out" use InsertStaffCheckOutByStaffId(staffId, time)
                try
                {
                    if (action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                    {
                        int newLogId = _dbHelper.InsertStaffCheckInByStaffId(staffId, deviceTimeStr);
                        if (newLogId <= 0)
                        {
                            LogHelper.Log($"[StaffAttendanceProcess] Staff check-in for Enroll={enrollmentId} ignored/suppressed (possible duplicate/debounce).");
                            return null;
                        }

                        LogHelper.Log($"[StaffAttendanceProcess] Staff CHECK-IN handled Enroll={enrollmentId} LogID={newLogId} Time={deviceTimeStr} Action={action} Staff={staffId}");
                    }
                    else if (action.Equals("check-out", StringComparison.OrdinalIgnoreCase))
                    {
                        bool ok = _dbHelper.InsertStaffCheckOutByStaffId(staffId, deviceTimeStr);
                        if (!ok)
                        {
                            LogHelper.Log($"[StaffAttendanceProcess] Staff check-out for Enroll={enrollmentId} not applied (no open session or validation failed).");
                            return null;
                        }

                        LogHelper.Log($"[StaffAttendanceProcess] Staff CHECK-OUT applied Enroll={enrollmentId} Time={deviceTimeStr} Action={action} Staff={staffId}");
                    }
                }
                catch (Exception exDbOp)
                {
                    LogHelper.Log($"[StaffAttendanceProcess] Error writing staff attendance: {exDbOp.Message}");
                    return null;
                }

                // Return string StaffId as-is (GUID)
                return new StaffAttendanceEventResult
                {
                    Success = true,
                    StaffId = staffId,
                    EnrollmentId = enrollmentId,
                    Action = action,
                    EventTime = deviceTime
                };
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[StaffAttendanceProcess][ERROR] {ex.Message}");
                return null;
            }
            finally
            {
                await Task.CompletedTask;
            }
        }

        private async Task LogStaffAttendanceToServer(StaffAttendanceEventResult result)
        {
            if (result == null) return;

            try
            {
                // Ensure HttpHandler is initialized if missing
                if (_httpHandler == null)
                {
                    if (_dbHelper == null) _dbHelper = new DatabaseHelper();
                    _httpHandler = new HttpHandler(_dbHelper);
                    LogHelper.Log("[LogStaffAttendanceToServer] HttpHandler was missing; initialized now for staff.");
                }

                int enrollmentId = result.EnrollmentId;
                if (enrollmentId <= 0)
                {
                    LogHelper.Log($"[LogStaffAttendanceToServer] Cannot log to server: Invalid enrollment ID for Staff={result.StaffId}");
                    return;
                }

                // Read GMT offset from settings (e.g., "+05", "-03", "+05:30")
                var gmtSetting = _dbHelper.GetSetting("GMT");

                // Resolve AttendanceId for staff and (for check-out) the paired check-in time
                string resolvedId = null;
                DateTime? pairedCheckIn = null;

                await DatabaseQueueManager.EnterDbWriteLockAsync("ResolveAttendanceIdForStaff", DbTaskPriority.High);
                try
                {
                    if (result.Action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                    {
                        resolvedId = _dbHelper.GetAttendanceIdForStaff(result.StaffId, result.EventTime, null);
                    }
                    else if (result.Action.Equals("check-out", StringComparison.OrdinalIgnoreCase))
                    {
                        resolvedId = _dbHelper.GetAttendanceIdForStaff(result.StaffId, null, result.EventTime);

                        // attempt to retrieve paired check-in time so we can include it with the checkout payload
                        try
                        {
                            pairedCheckIn = _dbHelper.GetPairedCheckInTimeForStaff(result.StaffId, result.EventTime);
                        }
                        catch (Exception exPair)
                        {
                            LogHelper.Log($"[LogStaffAttendanceToServer] Failed to get paired check-in for Staff={result.StaffId}: {exPair.Message}");
                        }
                    }
                    else
                    {
                        LogHelper.Log($"[LogStaffAttendanceToServer] Unsupported action '{result.Action}' for Staff={result.StaffId}");
                        return;
                    }
                }
                finally
                {
                    DatabaseQueueManager.ReleaseDbWriteLock("ResolveAttendanceIdForStaff");
                }

                if (string.IsNullOrEmpty(resolvedId))
                {
                    LogHelper.Log($"[LogStaffAttendanceToServer] Cannot log to server: No attendance ID resolved for Staff={result.StaffId} at {result.EventTime:yyyy-MM-dd HH:mm:ss}");
                    return;
                }

                // Build UTC (+00) strings from local device times
                string checkInStr = null;
                string checkOutStr = null;

                if (result.Action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                {
                    checkInStr = CommonUtils.ConvertToUtc0(result.EventTime, gmtSetting);

                    await _httpHandler.PostAttendanceRecordAsync(
                        enrollmentId,
                        checkinTime: checkInStr,
                        checkoutTime: null,
                        id: resolvedId
                    );

                    LogHelper.Log($"[LogStaffAttendanceToServer] CHECK-IN sent for Enroll={enrollmentId} Staff={result.StaffId} at {result.EventTime:yyyy-MM-dd HH:mm:ss} (normalized to UTC +00) id={resolvedId}");
                }
                else if (result.Action.Equals("check-out", StringComparison.OrdinalIgnoreCase))
                {
                    checkOutStr = CommonUtils.ConvertToUtc0(result.EventTime, gmtSetting);

                    if (pairedCheckIn.HasValue)
                        checkInStr = CommonUtils.ConvertToUtc0(pairedCheckIn.Value, gmtSetting);

                    await _httpHandler.PostAttendanceRecordAsync(
                        enrollmentId,
                        checkinTime: checkInStr,
                        checkoutTime: checkOutStr,
                        id: resolvedId
                    );

                    LogHelper.Log($"[LogStaffAttendanceToServer] CHECK-OUT sent for Enroll={enrollmentId} Staff={result.StaffId} (normalized to UTC +00) id={resolvedId} paired_checkin={(checkInStr ?? "null")}");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[LogStaffAttendanceToServer] Error: {ex.Message}");
            }
        }

        // Simple popup re-use for staff: show enrollment-based popup using existing FacePopupForm.
        private void ShowFacePopupForStaff(string staffId, int enrollmentId, string action, DateTime time)
        {
            try
            {
                Form main = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;

                Action showAction = delegate
                {
                    try
                    {
                        if (action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                        {
                            SystemSounds.Exclamation.Play();
                        }
                        else
                        {
                            SystemSounds.Hand.Play();
                        }

                        // Reuse FacePopupForm via Dashboard helper
                        Dashboard.GlobalShowFacePopup(enrollmentId, enrollmentId, action, time);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log("[StaffFacePopup][ERROR] " + ex.Message);
                    }
                };

                Action startDelayOnUi = delegate
                {
                    try
                    {
                        var timer = new System.Windows.Forms.Timer { Interval = 2000 }; // 2 seconds
                        timer.Tick += (s, e) =>
                        {
                            var t = (System.Windows.Forms.Timer)s;
                            t.Stop();
                            t.Dispose();
                            showAction();
                        };
                        timer.Start();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log("[StaffFacePopup][DelayTimer][ERROR] " + ex.Message);
                        showAction();
                    }
                };

                if (main != null && main.InvokeRequired)
                {
                    main.BeginInvoke(startDelayOnUi);
                }
                else if (main != null)
                {
                    startDelayOnUi();
                }
                else
                {
                    Task.Run(async () =>
                    {
                        try
                        {
                            await Task.Delay(2000).ConfigureAwait(false);
                            var ui = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
                            if (ui != null && ui.IsHandleCreated && !ui.IsDisposed)
                                ui.BeginInvoke((Action)(() => showAction()));
                            else
                                showAction();
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log("[StaffFacePopup][DelayTask][ERROR] " + ex.Message);
                        }
                    });
                }
            }
            catch (Exception exOuter)
            {
                LogHelper.Log("[StaffFacePopup][ERROR-Outer] " + exOuter.Message);
            }
        }

        // Helper to get staff face image path (face files are named by StaffID in other parts of the codebase)
        private string GetStaffImagePath(string staffId)
        {
            try
            {
                if (string.IsNullOrEmpty(staffId)) return null;
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string faceDir = Path.Combine(baseDir, "FACE");

                string[] candidates =
                {
                    Path.Combine(faceDir, staffId + ".jpg"),
                    Path.Combine(faceDir, staffId + ".png")
                };

                foreach (var path in candidates)
                    if (File.Exists(path))
                        return path;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[GetStaffImagePath] Error finding image for staff {staffId}: {ex.Message}");
            }

            return null;
        }
    }
}