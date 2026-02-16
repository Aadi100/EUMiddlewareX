using smart_attendance_middleware.Classes;
using System;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    // Name exactly as requested
    public class AttendenceServerFunctions
    {

        public DatabaseHelper _dbHelper;
        public WebSocketManager _wsManager;
        private Processor _processor;
        private HttpHandler _httpHandler;
        public AttendenceServerFunctions(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? new DatabaseHelper();
            _httpHandler = new HttpHandler(_dbHelper);
        }

        public AttendenceServerFunctions(WebSocketManager wsManager)
        {
            _dbHelper = new DatabaseHelper();
            _wsManager = wsManager;

            if (_wsManager != null)
            {
                _processor = new Processor(_dbHelper, _wsManager, this);
                // Ensure HttpHandler has a Processor to process responses
                _httpHandler = new HttpHandler(_dbHelper, _processor);
            }
            else
            {
                _httpHandler = new HttpHandler(_dbHelper);
            }
        }

        public AttendenceServerFunctions()
        {
            _dbHelper = new DatabaseHelper();
            // HttpHandler without Processor for now; will be re-created when WS is set
            _httpHandler = new HttpHandler(_dbHelper);
        }

        // Method to set the WebSocketManager if needed after construction
        public void SetWebSocketManager(WebSocketManager wsManager)
        {
            if (wsManager == null)
            {
                LogHelper.Log("[AttendenceServerFunctions] Ignoring attempt to set null WebSocketManager");
                return;
            }

            _wsManager = wsManager;
            LogHelper.Log("[AttendenceServerFunctions] WebSocketManager successfully set");

            // Create processor only if not already created
            if (_processor == null && _wsManager != null)
            {
                _processor = new Processor(_dbHelper, _wsManager, this);
                LogHelper.Log("[AttendenceServerFunctions] Processor created with WebSocketManager");

                // Recreate HttpHandler with Processor so it can process attendance/member responses
                _httpHandler = new HttpHandler(_dbHelper, _processor);
            }
        }

        public sealed class AttendanceRecord
        {
            public int DevID { get; set; }
            public string IP { get; set; }
            public int Enroll { get; set; }
            public DateTime Time { get; set; } // Device (source) timestamp
        }

        private sealed class AttendanceEventResult
        {
            public bool Success { get; set; }
            public int MemberId { get; set; }
            public int EnrollmentId { get; set; }
            public string Action { get; set; }
            public DateTime EventTime { get; set; } // Echo of device timestamp
        }

        // Inside the Process method, add a WebSocketManager check and attempt to get it from the application state
       
        public virtual async void Process(AttendanceRecord record)
        {
            if (record == null) return;

            // Use ONLY record.Time for all persistence & UI (do not replace with DateTime.Now)
            DateTime deviceTime = record.Time;

            LogHelper.Log(string.Format("[AttendanceProcess] MachineNo={0}, IP={1}, Enroll={2}, Time={3:yyyy-MM-dd HH:mm:ss}",
                record.DevID, record.IP, record.Enroll, deviceTime));

            if (_dbHelper == null)
                _dbHelper = new DatabaseHelper();

            //Check if WebSocketManager is missing and try to get it from Dashboard if available
            if (_wsManager == null)
            {
                // Log that we're attempting to get WebSocketManager
                LogHelper.Log("[AttendanceProcess] WebSocketManager not set, attempting to retrieve from application state...");

                // Try to find the Dashboard form which should have the WebSocketManager
                var dashboard = Application.OpenForms.OfType<Dashboard>().FirstOrDefault();
                if (dashboard != null && dashboard._wsManager != null)
                {
                    LogHelper.Log("[AttendanceProcess] Found WebSocketManager in Dashboard, setting it now");
                    SetWebSocketManager(dashboard._wsManager);
                }
                else
                {
                    LogHelper.Log("[AttendanceProcess] Could not find WebSocketManager in application state");
                }
            }

            // --- debounce guard BEFORE processing DB mutation ---
            try
            {
                // Resolve action and memberId for debounce checks
                string action = _dbHelper.GetDeviceAttendanceAction(record.DevID);
                int? memberId = _dbHelper.GetMemberIdByEnrollment(record.Enroll);

                // Read debounce seconds from settings; if invalid, prompt the user and stop
                var s = _dbHelper.GetSetting("AttendanceDebounceSeconds");
                if (!int.TryParse(s, out var debounceSeconds) || debounceSeconds < 0)
                {
                    MessageBox.Show(
                        "Debounce setting is missing or invalid. Please set it in Device Manager and try again.",
                        "Debounce Setting Required",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    LogHelper.Log("[AttendanceProcess][Debounce] Missing/invalid debounce setting. Aborting event processing.");
                    return; // do not process further until a valid value is saved
                }

                if (string.IsNullOrEmpty(action) || !memberId.HasValue)
                {
                    LogHelper.Log("[AttendanceProcess] Debounce check skipped (missing action or memberId)");
                }
                else
                {
                    if (action.Equals("check-out", StringComparison.OrdinalIgnoreCase))
                    {
                        // Require an open session; compare with its check-in time
                        int openLogId = _dbHelper.GetOpenAttendanceLogId(memberId.Value);
                        if (openLogId > 0)
                        {
                            DateTime? openCheckInTime = _dbHelper.GetOpenAttendanceLogCheckInTime(memberId.Value);
                            if (openCheckInTime.HasValue)
                            {
                                var diff = deviceTime - openCheckInTime.Value;
                                if (diff.TotalSeconds < debounceSeconds)
                                {
                                    LogHelper.Log($"[AttendanceProcess][Debounce] CHECK-OUT ignored (Δ={diff.TotalSeconds:F0}s < {debounceSeconds}s) Member={memberId} Time={deviceTime:yyyy-MM-dd HH:mm:ss}");
                                    return; // exit early
                                }
                            }
                            else
                            {
                                LogHelper.Log("[AttendanceProcess][Debounce] Open session found but no check-in time; proceeding.");
                            }
                        }
                        else
                        {
                            LogHelper.Log("[AttendanceProcess][Debounce] No open session for checkout; passing to handler.");
                        }
                    }
                    
                    else if (action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                    {
                        int openLogId = _dbHelper.GetOpenAttendanceLogId(memberId.Value);

                        // If session is completed (no open log), apply a HARD-CODED 10s debounce
                        // against the last check-out time.
                        if (openLogId <= 0)
                        {
                            const int completedSessionCheckInDebounceSeconds = 10;

                            DateTime? lastCheckOut = _dbHelper.GetLastCheckOutTimeForMember(memberId.Value);
                            if (lastCheckOut.HasValue)
                            {
                                var diff = deviceTime - lastCheckOut.Value;
                                if (diff.TotalSeconds < completedSessionCheckInDebounceSeconds)
                                {
                                    LogHelper.Log(
                                        $"[AttendanceProcess][Debounce] CHECK-IN ignored after completed session " +
                                        $"(Δ={diff.TotalSeconds:F0}s < {completedSessionCheckInDebounceSeconds}s) " +
                                        $"Member={memberId} Time={deviceTime:yyyy-MM-dd HH:mm:ss}");
                                    return;
                                }
                            }
                            else
                            {
                                LogHelper.Log(
                                    $"[AttendanceProcess][Debounce] CHECK-IN completed-session debounce check skipped (no last checkout). " +
                                    $"Member={memberId} Time={deviceTime:yyyy-MM-dd HH:mm:ss}");
                            }
                        }
                        else
                        {
                            // There is an open session; keep existing configurable debounce behavior (debounceSeconds)
                            DateTime? lastCheckOut = _dbHelper.GetLastCheckOutTimeForMember(memberId.Value);
                            if (lastCheckOut.HasValue)
                            {
                                var diff = deviceTime - lastCheckOut.Value;
                                if (diff.TotalSeconds < debounceSeconds)
                                {
                                    LogHelper.Log(
                                        $"[AttendanceProcess][Debounce] CHECK-IN ignored (Δ={diff.TotalSeconds:F0}s < {debounceSeconds}s) " +
                                        $"Member={memberId} Time={deviceTime:yyyy-MM-dd HH:mm:ss}");
                                    return;
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception exDebounce)
            {
                LogHelper.Log($"[AttendanceProcess][Debounce][ERROR] {exDebounce.Message}");
                // Fail-open: proceed to normal handling
            }

            AttendanceEventResult result = await HandleAttendanceEvent(record.DevID, record.Enroll, deviceTime);

            if (result != null && result.Success)
            {
                ShowFacePopup(result.MemberId, result.EnrollmentId, result.Action, result.EventTime);

                // --- Update AppSession here, after showing the popup ---
                string memberName = _dbHelper.GetMemberName(result.MemberId);
                var attendanceDetails = new LastAttendanceDetails
                {
                    MemberId = result.MemberId,
                    EnrollmentId = result.EnrollmentId,
                    MemberName = memberName ?? "Unknown Member",
                    Action = result.Action,
                    Time = result.EventTime,
                    ImagePath = GetMemberImagePath(result.MemberId)
                };

                if (result.Action == "check-in")
                {
                    AppSession.LastCheckIn = attendanceDetails;
                    LogHelper.Log($"[AppSession] Updated last check-in: {memberName} at {result.EventTime:HH:mm:ss}");
                }
                else if (result.Action == "check-out")
                {
                    AppSession.LastCheckOut = attendanceDetails;
                    LogHelper.Log($"[AppSession] Updated last check-out: {memberName} at {result.EventTime:HH:mm:ss}");
                }
                // -------------------------------------------------------

                // Log attendance to the server using the WebSocketManager
                await LogAttendanceToServer(result);
            }
        }


        private async Task LogAttendanceToServer(AttendanceEventResult result)
        {
            if (result == null) return;

            try
            {
                if (_httpHandler == null)
                {
                    LogHelper.Log("[LogAttendanceToServer] HttpHandler not initialized.");
                    return;
                }

                int enrollmentId = result.EnrollmentId;
                if (enrollmentId <= 0)
                {
                    LogHelper.Log($"[LogAttendanceToServer] Cannot log to server: Invalid enrollment ID for Member={result.MemberId}");
                    return;
                }

                // Read GMT offset from settings (e.g., "+05", "-03", "+05:30")
                var gmtSetting = _dbHelper.GetSetting("GMT");

                // Resolve attendance ID by action-specific timestamp under DB lock
                string resolvedId = null;
                await DatabaseQueueManager.EnterDbWriteLockAsync("ResolveAttendanceIdForMember", DbTaskPriority.High);
                try
                {
                    if (result.Action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                    {
                        resolvedId = _dbHelper.GetAttendanceIdForMember(result.MemberId, result.EventTime, null);
                    }
                    else if (result.Action.Equals("check-out", StringComparison.OrdinalIgnoreCase))
                    {
                        resolvedId = _dbHelper.GetAttendanceIdForMember(result.MemberId, null, result.EventTime);
                    }
                    else
                    {
                        LogHelper.Log($"[LogAttendanceToServer] Unsupported action '{result.Action}'");
                        return;
                    }
                }
                finally
                {
                    DatabaseQueueManager.ReleaseDbWriteLock("ResolveAttendanceIdForMember");
                }

                if (string.IsNullOrEmpty(resolvedId))
                {
                    LogHelper.Log($"[LogAttendanceToServer] Cannot log to server: No attendance ID for Member={result.MemberId} at {result.EventTime:yyyy-MM-dd HH:mm:ss}");
                    return;
                }

                // Build UTC (+00) strings from local device times
                string checkInStr = null;
                string checkOutStr = null;

                if (result.Action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                {
                    // Convert local EventTime (under configured GMT) to UTC and format with "+00"
                    checkInStr = CommonUtils.FormatWithGmt(result.EventTime, gmtSetting);

                    await _httpHandler.PostAttendanceRecordAsync(
                        enrollmentId,
                        checkinTime: checkInStr,
                        checkoutTime: null,
                        id: resolvedId
                    );

                    LogHelper.Log($"[LogAttendanceToServer] CHECK-IN sent for Member={result.MemberId} at {result.EventTime:yyyy-MM-dd HH:mm:ss} (normalized to UTC +00)");
                }
                else if (result.Action.Equals("check-out", StringComparison.OrdinalIgnoreCase))
                {
                    // Get paired check-in time under DB lock
                    DateTime? checkInTime = null;
                    await DatabaseQueueManager.EnterDbWriteLockAsync("GetPairedCheckInTime", DbTaskPriority.High);
                    try
                    {
                        checkInTime = _dbHelper.GetPairedCheckInTime(result.MemberId, result.EventTime);
                    }
                    finally
                    {
                        DatabaseQueueManager.ReleaseDbWriteLock("GetPairedCheckInTime");
                    }

                    if (checkInTime.HasValue)
                    {
                        checkInStr = CommonUtils.FormatWithGmt(checkInTime.Value, gmtSetting);
                    }
                    checkOutStr = CommonUtils.FormatWithGmt(result.EventTime, gmtSetting);

                    await _httpHandler.PostAttendanceRecordAsync(
                        enrollmentId,
                        checkinTime: checkInStr,
                        checkoutTime: checkOutStr,
                        id: resolvedId
                    );

                    LogHelper.Log($"[LogAttendanceToServer] CHECK-OUT sent for Member={result.MemberId} (normalized to UTC +00)");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[LogAttendanceToServer] Error: {ex.Message}");
            }
        }

        private async Task<AttendanceEventResult> HandleAttendanceEvent(int machineNo, int enrollmentId, DateTime deviceTime)
        {
            try
            {
                string action = _dbHelper.GetDeviceAttendanceAction(machineNo);
                if (action == null)
                {
                    LogHelper.Log($"[AttendanceProcess] Unknown MachineNo {machineNo}. Ignored.");
                    return null;
                }

                int? memberId = _dbHelper.GetMemberIdByEnrollment(enrollmentId);
                if (!memberId.HasValue)
                {
                    LogHelper.Log($"[AttendanceProcess] EnrollmentID {enrollmentId} not linked to any member. Ignored.");
                    return null;
                }

                string deviceTimeStr = deviceTime.ToString("yyyy-MM-dd HH:mm:ss");

                // Duplicate + sync check (exact timestamp match)
                // Requires DatabaseHelper.CheckAttendanceDuplicateAndSynced(memberId, deviceTime, action)
                var dup = _dbHelper.CheckAttendanceDuplicateAndSynced(memberId.Value, deviceTime, action);
                if (dup.exists)
                {
                    if (dup.synced)
                    {
                        // Already processed and acknowledged by cloud – suppress entirely.
                        LogHelper.Log($"[AttendanceProcess] Suppressed re-streamed {action} (already synced) Member={memberId} Time={deviceTimeStr}");
                        return null;
                    }
                    else
                    {
                        // This avoids double entries but allows cloud sync to complete.
                        LogHelper.Log($"[AttendanceProcess] Duplicate {action} detected for Member={memberId} at {deviceTimeStr}, Ignoring.");
                        return null; // Changed to null to avoid double sending; handled in Process method
                    }
                }

                // Unified gate (today + previous-day open session logic) only for non-duplicates
                bool gate = _dbHelper.CanProcessAttendance(memberId.Value, deviceTime, action);

                if (action == "check-in")
                {
                    // gate == true means no open session -> allow insert
                    if (!gate)
                    {
                        LogHelper.Log($"[AttendanceProcess] Duplicate/open CHECK-IN blocked for Member={memberId} at {deviceTimeStr}");
                        return null;
                    }

                    await DatabaseQueueManager.EnterDbWriteLockAsync("InsertAttendanceLog", DbTaskPriority.Medium);
                    try
                    {
                        int logId = _dbHelper.InsertAttendanceLog(memberId.Value, deviceTimeStr);
                        LogHelper.Log($"[AttendanceProcess] CHECK-IN stored Member={memberId} LogID={logId} Time={deviceTimeStr}");
                    }
                    finally
                    {
                        DatabaseQueueManager.ReleaseDbWriteLock("InsertAttendanceLog");
                    }
                }
                else if (action == "check-out")
                {
                    // gate == true means there IS an open session -> allow checkout update
                    if (gate)
                    {
                        await DatabaseQueueManager.EnterDbWriteLockAsync("UpdateAttendanceLogForCheckOut", DbTaskPriority.Medium);
                        try
                        {
                            _dbHelper.UpdateAttendanceLogForCheckOut(memberId.Value, deviceTimeStr);
                            LogHelper.Log($"[AttendanceProcess] CHECK-OUT applied Member={memberId} Time={deviceTimeStr}");
                        }
                        finally
                        {
                            DatabaseQueueManager.ReleaseDbWriteLock("UpdateAttendanceLogForCheckOut");
                        }
                    }
                    else
                    {
                        LogHelper.Log($"[AttendanceProcess] CHECK-OUT ignored for Member={memberId} at {deviceTimeStr} (no open session)");
                        return null;
                    }
                }
                else
                {
                    LogHelper.Log($"[AttendanceProcess] Unsupported action '{action}' for MachineNo={machineNo}. Ignored.");
                    return null;
                }


                return new AttendanceEventResult
                {
                    Success = true,
                    MemberId = memberId.Value,
                    EnrollmentId = enrollmentId,
                    Action = action,
                    EventTime = deviceTime
                };
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[AttendanceProcess][ERROR] {ex.Message}");
                return null;
            }

        }

        public object ProcessAttendanceEvent(int machineNo, int enrollmentId, DateTime deviceTime)
        {
            return HandleAttendanceEvent(machineNo, enrollmentId, deviceTime);
        }

        // Helper method to get member image path
        private string GetMemberImagePath(int memberId)
        {
            try
            {
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string faceDir = Path.Combine(baseDir, "FACE");
                string[] candidates =
                {
            Path.Combine(faceDir, memberId + ".jpg"),
            Path.Combine(faceDir, memberId + ".png")
        };

                foreach (var path in candidates)
                {
                    if (File.Exists(path))
                    {
                        return path;
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[GetMemberImagePath] Error finding image for member {memberId}: {ex.Message}");
            }

            return null; // No image found
        }


        private void ShowFacePopup(int memberId, int enrollmentId, string action, DateTime time)
        {
            try
            {
                Form main = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;

                Action showAction = delegate
                {
                    try
                    {
                        // Play different sounds for check-in vs check-out (after delay, in sync with popup)
                        if (action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                        {
                            SystemSounds.Exclamation.Play();
                        }
                        else
                        {
                            SystemSounds.Hand.Play();
                        }

                        FacePopupForm f = new FacePopupForm(memberId, enrollmentId, action, time);
                        f.Show();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log("[FacePopup][ERROR] " + ex.Message);
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
                        LogHelper.Log("[FacePopup][DelayTimer][ERROR] " + ex.Message);
                        // Fallback without delay
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
                    // Fallback: delay on thread pool, then try to marshal back to any open form
                    System.Threading.Tasks.Task.Run(async () =>
                    {
                        try
                        {
                            await System.Threading.Tasks.Task.Delay(2000).ConfigureAwait(false);
                            var ui = Application.OpenForms.Count > 0 ? Application.OpenForms[0] : null;
                            if (ui != null && ui.IsHandleCreated && !ui.IsDisposed)
                                ui.BeginInvoke((Action)(() => showAction()));
                            else
                                showAction(); // last resort
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log("[FacePopup][DelayTask][ERROR] " + ex.Message);
                        }
                    });
                }
            }
            catch (Exception exOuter)
            {
                LogHelper.Log("[FacePopup][ERROR-Outer] " + exOuter.Message);
            }
        }

    }
}