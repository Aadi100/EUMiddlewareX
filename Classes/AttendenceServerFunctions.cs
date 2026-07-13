using smart_attendance_middleware.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Media;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public class AttendenceServerFunctions
    {
        public DatabaseHelper _dbHelper;
        private Processor _processor; 
        private HttpHandler _httpHandler;

        public AttendenceServerFunctions(DatabaseHelper dbHelper)
        {
            _dbHelper = dbHelper ?? new DatabaseHelper();
            _httpHandler = new HttpHandler(_dbHelper);
        }

        public AttendenceServerFunctions()
        {
            _dbHelper = new DatabaseHelper();
            _processor = new Processor(_dbHelper, null, this);
            _httpHandler = new HttpHandler(_dbHelper, _processor);
        }

        public sealed class AttendanceRecord
        {
            public int DevID { get; set; }
            public string IP { get; set; }
            public int Enroll { get; set; }
            public DateTime Time { get; set; } // Device (source) timestamp
            public string Name { get; set; }
            public string Image { get; set; }
        }

        private sealed class AttendanceEventResult
        {
            public bool Success { get; set; }
            public int MemberId { get; set; }
            public int EnrollmentId { get; set; }
            public string Action { get; set; }
            public DateTime EventTime { get; set; } // Echo of device timestamp
            public string AttendanceId { get; set; }
        }

        // Inside the Process method, add a WebSocketManager check and attempt to get it from the application state

        public virtual async void Process(AttendanceRecord record)
        {
            if (record == null) return;

            // Use ONLY record.Time for all persistence & UI (do not replace with DateTime.Now)
            DateTime deviceTime = record.Time;

            LogHelper.Log(string.Format("[MemberAttendanceProcess] MachineNo={0}, IP={1}, Enroll={2}, Time={3:yyyy-MM-dd HH:mm:ss}",
                record.DevID, record.IP, record.Enroll, deviceTime));

            if (_dbHelper == null)
                _dbHelper = new DatabaseHelper();


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
                    LogHelper.Log("[MemberAttendanceProcess][Debounce] Missing/invalid debounce setting. Aborting event processing.");
                    return; // do not process further until a valid value is saved
                }

                if (string.IsNullOrEmpty(action) || !memberId.HasValue)
                {
                    LogHelper.Log("[MemberAttendanceProcess] Debounce check skipped (missing action or memberId)");
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
                                    LogHelper.Log($"[MemberAttendanceProcess][Debounce] CHECK-OUT ignored (Δ={diff.TotalSeconds:F0}s < {debounceSeconds}s) Member={memberId} Time={deviceTime:yyyy-MM-dd HH:mm:ss}");
                                    return; // exit early
                                }
                            }
                            else
                            {
                                LogHelper.Log("[MemberAttendanceProcess][Debounce] Open session found but no check-in time; proceeding.");
                            }
                        }
                        else
                        {
                            LogHelper.Log("[MemberAttendanceProcess][Debounce] No open session for checkout; passing to handler.");
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
                                        $"[MemberAttendanceProcess][Debounce] CHECK-IN ignored after completed session " +
                                        $"(Δ={diff.TotalSeconds:F0}s < {completedSessionCheckInDebounceSeconds}s) " +
                                        $"Member={memberId} Time={deviceTime:yyyy-MM-dd HH:mm:ss}");
                                    return;
                                }
                            }
                            else
                            {
                                LogHelper.Log(
                                    $"[MemberAttendanceProcess][Debounce] CHECK-IN completed-session debounce check skipped (no last checkout). " +
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
                                        $"[MemberAttendanceProcess][Debounce] CHECK-IN ignored (Δ={diff.TotalSeconds:F0}s < {debounceSeconds}s) " +
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
                LogHelper.Log($"[MemberAttendanceProcess][Debounce][ERROR] {exDebounce.Message}");
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
                await LogMemberAttendanceToServer(result);
            }
        }


        private async Task LogMemberAttendanceToServer(AttendanceEventResult result)
        {
            if (result == null) return;

            try
            {
                // Ensure HttpHandler is initialized if missing
                if (_httpHandler == null)
                {
                    if (_dbHelper == null) _dbHelper = new DatabaseHelper();
                    _httpHandler = new HttpHandler(_dbHelper);
                    LogHelper.Log("[LogMemberAttendanceToServer] HttpHandler was missing; initialized now.");
                }

                int enrollmentId = result.EnrollmentId;
                if (enrollmentId <= 0)
                {
                    LogHelper.Log($"[LogMemberAttendanceToServer] Cannot log to server: Invalid enrollment ID for Member={result.MemberId}");
                    return;
                }

                // Read GMT offset from settings (e.g., "+05", "-03", "+05:30")
                var gmtSetting = _dbHelper.GetSetting("GMT");

                // instead of performing an unreliable database lookup by timestamp.
                string resolvedId = result.AttendanceId;

                if (string.IsNullOrEmpty(resolvedId))
                {
                    LogHelper.Log($"[LogMemberAttendanceToServer] Cannot log to server: No attendance ID received from DB operation for Member={result.MemberId}");
                    return;
                }

                // Build UTC (+00) strings from local device times
                string checkInStr = null;
                string checkOutStr = null;

                if (result.Action.Equals("check-in", StringComparison.OrdinalIgnoreCase))
                {
                    // Convert local EventTime (under configured GMT) to UTC and format with "+00"
                    checkInStr = CommonUtils.ConvertToUtc0(result.EventTime, gmtSetting);

                    await _httpHandler.PostAttendanceRecordAsync(
                        enrollmentId,
                        checkinTime: checkInStr,
                        checkoutTime: null,
                        id: resolvedId
                    );

                    LogHelper.Log($"[LogMemberAttendanceToServer] CHECK-IN sent for Member={result.MemberId} at {result.EventTime:yyyy-MM-dd HH:mm:ss} (normalized to UTC +00)");
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
                        checkInStr = CommonUtils.ConvertToUtc0(checkInTime.Value, gmtSetting);
                    }
                    checkOutStr = CommonUtils.ConvertToUtc0(result.EventTime, gmtSetting);

                    await _httpHandler.PostAttendanceRecordAsync(
                        enrollmentId,
                        checkinTime: checkInStr,
                        checkoutTime: checkOutStr,
                        id: resolvedId
                    );

                    LogHelper.Log($"[LogMemberAttendanceToServer] CHECK-OUT sent for Member={result.MemberId} at {result.EventTime:yyyy-MM-dd HH:mm:ss} (normalized to UTC +00)");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[LogMemberAttendanceToServer] Error: {ex.Message}");
            }
        }
        private async Task<AttendanceEventResult> HandleAttendanceEvent(int machineNo, int enrollmentId, DateTime deviceTime)
        {
            try
            {
                string action = _dbHelper.GetDeviceAttendanceAction(machineNo);
                if (action == null)
                {
                    LogHelper.Log($"[MemberAttendanceProcess] Unknown MachineNo {machineNo}. Ignored.");
                    return null;
                }

                int? memberId = _dbHelper.GetMemberIdByEnrollment(enrollmentId);
                if (!memberId.HasValue)
                {
                    LogHelper.Log($"[MemberAttendanceProcess] EnrollmentID {enrollmentId} not linked to any member. Ignored.");
                    return null;
                }

                string deviceTimeStr = deviceTime.ToString("yyyy-MM-dd HH:mm:ss");
                string attendanceId = null;

                // Duplicate + sync check (exact timestamp match)
                var dup = _dbHelper.CheckAttendanceDuplicateAndSynced(memberId.Value, deviceTime, action);
                if (dup.exists)
                {
                    if (dup.synced)
                    {
                        LogHelper.Log($"[MemberAttendanceProcess] Suppressed re-streamed {action} (already synced) Member={memberId} Time={deviceTimeStr}");
                        return null;
                    }
                    else
                    {
                        LogHelper.Log($"[MemberAttendanceProcess] Duplicate {action} detected for Member={memberId} at {deviceTimeStr}, Ignoring.");
                        return null;
                    }
                }

                // Unified gate (today + previous-day open session logic)
                bool gate = _dbHelper.CanProcessAttendance(memberId.Value, deviceTime, action);

                if (action == "check-in")
                {
                    if (!gate)
                    {
                        LogHelper.Log($"[MemberAttendanceProcess] Duplicate/open CHECK-IN blocked for Member={memberId} at {deviceTimeStr}");
                        return null;
                    }

                    await DatabaseQueueManager.EnterDbWriteLockAsync("InsertAttendanceLog", DbTaskPriority.Medium);
                    try
                    {
                        // Use the updated tuple return from DatabaseHelper
                        var result = _dbHelper.InsertAttendanceLog(memberId.Value, deviceTimeStr);
                        attendanceId = result.AttendanceId;

                        if (result.LogId > 0)
                            LogHelper.Log($"[MemberAttendanceProcess] CHECK-IN stored Member={memberId} LogID={result.LogId} GUID={attendanceId} Time={deviceTimeStr}");
                        else
                            return null; // DB insert failed (e.g. timestamp validation)
                    }
                    finally
                    {
                        DatabaseQueueManager.ReleaseDbWriteLock("InsertAttendanceLog");
                    }
                }
                else if (action == "check-out")
                {
                    if (gate)
                    {
                        await DatabaseQueueManager.EnterDbWriteLockAsync("UpdateAttendanceLogForCheckOut", DbTaskPriority.Medium);
                        try
                        {
                            _dbHelper.UpdateAttendanceLogForCheckOut(memberId.Value, deviceTimeStr);

                            // For Check-out, we must fetch the AttendanceId of the session we just updated.
                            attendanceId = _dbHelper.GetAttendanceIdForMember(memberId.Value, null, deviceTime);

                            LogHelper.Log($"[MemberAttendanceProcess] CHECK-OUT applied Member={memberId} GUID={attendanceId} Time={deviceTimeStr}");
                        }
                        finally
                        {
                            DatabaseQueueManager.ReleaseDbWriteLock("UpdateAttendanceLogForCheckOut");
                        }
                    }
                    else
                    {
                        LogHelper.Log($"[MemberAttendanceProcess] CHECK-OUT ignored for Member={memberId} at {deviceTimeStr} (no open session)");
                        return null;
                    }
                }
                else
                {
                    LogHelper.Log($"[MemberAttendanceProcess] Unsupported action '{action}' for MachineNo={machineNo}. Ignored.");
                    return null;
                }

                // If we couldn't resolve an ID, we shouldn't continue to the server logging step.
                if (string.IsNullOrEmpty(attendanceId))
                {
                    LogHelper.Log($"[MemberAttendanceProcess] Warning: Could not resolve Attendance GUID for Member={memberId}. Server sync may fail.");
                }

                return new AttendanceEventResult
                {
                    Success = true,
                    MemberId = memberId.Value,
                    EnrollmentId = enrollmentId,
                    Action = action,
                    EventTime = deviceTime,
                    AttendanceId = attendanceId 
                };
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[MemberAttendanceProcess][ERROR] {ex.Message}");
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

                        Dashboard.GlobalShowFacePopup(memberId, enrollmentId, action, time);
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

        public void HandleUnknownOrStaffScan(int enroll, string name, string image, string ip, DateTime time)
        {
            // Resolve device location from AppSession by IP
            string ipOnly = ip ?? string.Empty;
            int colon = ipOnly.IndexOf(':');
            if (colon >= 0) ipOnly = ipOnly.Substring(0, colon);

            var allDevices = new List<Device>();
            if (AppSession.DeviceList != null) allDevices.AddRange(AppSession.DeviceList);
            if (AppSession.StaffDeviceList != null) allDevices.AddRange(AppSession.StaffDeviceList);
            var device = allDevices.FirstOrDefault(d => string.Equals(d.DeviceIP, ipOnly, StringComparison.OrdinalIgnoreCase));
            string location = device?.Location ?? "Unknown Location";

            string infoBlock = $"EnrollID : {enroll}\n" +
                               $"Name     : {name ?? "Unknown"}\n" +
                               $"Location : {location}\n" +
                               $"Time     : {time:yyyy-MM-dd HH:mm:ss}";

            Action show;

            if (string.IsNullOrEmpty(image))
            {
                show = () =>
                {
                    MessageBox.Show(
                        infoBlock + "\n\nNo image captured.",
                        "Scan Alert",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning
                    );
                };
            }
            else
            {
                try
                {
                    byte[] imageBytes = Convert.FromBase64String(image);
                    System.Drawing.Image img;
                    using (var ms = new MemoryStream(imageBytes))
                        img = System.Drawing.Image.FromStream(ms);

                    show = () =>
                    {
                        var dlg = new Form
                        {
                            Text = $"Scan | EnrollID: {enroll} | {location}",
                            Size = new System.Drawing.Size(340, 520),
                            StartPosition = FormStartPosition.CenterScreen,
                            FormBorderStyle = FormBorderStyle.FixedDialog,
                            MaximizeBox = false
                        };

                        var panel = new System.Windows.Forms.Panel
                        {
                            Dock = DockStyle.Bottom,
                            Height = 90,
                            Padding = new Padding(8)
                        };

                        var lbl = new Label
                        {
                            Dock = DockStyle.Fill,
                            Text = infoBlock,
                            Font = new System.Drawing.Font("Segoe UI", 9f),
                            TextAlign = System.Drawing.ContentAlignment.MiddleLeft
                        };

                        panel.Controls.Add(lbl);

                        var pb = new PictureBox
                        {
                            Dock = DockStyle.Fill,
                            SizeMode = PictureBoxSizeMode.Zoom,
                            Image = img
                        };

                        dlg.Controls.Add(panel);
                        dlg.Controls.Add(pb);
                        dlg.FormClosed += (s, e) => img.Dispose();
                        dlg.Show();
                    };
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[UnknownScan] Failed to decode image: {ex.Message}");
                    show = () =>
                    {
                        MessageBox.Show(
                            infoBlock + "\n\nImage could not be decoded.",
                            "Scan Alert",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error
                        );
                    };
                }
            }

            var owner = WebsocketServer.Instance;
            if (owner != null && !owner.IsDisposed)
                owner.BeginInvoke(show);
            else
                show();
        }

    }
}