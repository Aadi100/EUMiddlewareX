using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static CuoreUI.Drawing.EasingFunctions;

namespace smart_attendance_middleware.Classes
{
    internal sealed class AttendanceSimulator
    {
        private readonly DatabaseHelper _db;
        private readonly WebSocketManager _ws;
        private readonly Random _rng = new Random();
        private readonly int _durationMinutes;

        public AttendanceSimulator(DatabaseHelper dbHelper, WebSocketManager wsManager, int durationMinutes = 1)
        {
            _db = dbHelper ?? throw new ArgumentNullException(nameof(dbHelper));
            _ws = wsManager ?? throw new ArgumentNullException(nameof(wsManager));
            _durationMinutes = durationMinutes > 0 ? durationMinutes : 1;
        }

        public async Task RunAsync(CancellationToken token)
        {
            // Hard-limit runtime (respects external cancellation too)
            using (var cts = CancellationTokenSource.CreateLinkedTokenSource(token))
            {
                cts.CancelAfter(TimeSpan.FromMinutes(_durationMinutes));
                var hardStop = cts.Token;
                var until = DateTime.UtcNow.AddMinutes(_durationMinutes);
                SimulationEvents.RaiseSimulationStarted(until);

                // Same pipeline that AttendanceServerForm uses internally
                var fn = new AttendenceServerFunctions();
                fn._dbHelper = _db;

                // Devices (attendance devices only)
                var activeDevices = (_db.LoadDevices() ?? new List<Device>())
                    .Where(d => d != null && d.IsActive)
                    .ToList();

                if (activeDevices.Count == 0)
                {
                    LogHelper.Log("[SIM] No active devices found. Aborting.");
                    SimulationEvents.RaiseSimulationStopped("No active devices");
                    return;
                }

                // Split by action (AttendenceServerFunctions uses MachineNo => action)
                var checkInDevices = activeDevices.Where(d => IsAction(d.MachineNo, "check-in")).ToList();
                var checkOutDevices = activeDevices.Where(d => IsAction(d.MachineNo, "check-out")).ToList();

                if (checkInDevices.Count == 0 && checkOutDevices.Count == 0)
                {
                    LogHelper.Log("[SIM] No devices mapped to 'check-in' or 'check-out'. Aborting.");
                    SimulationEvents.RaiseSimulationStopped("No in/out device mapping");
                    return;
                }

                // Get fully synced members
                var members = _db.GetMembers() ?? new List<Member>();
                var activeDeviceIds = new HashSet<int>(activeDevices.Select(d => d.DeviceId));

                var fullySyncedMembers = members
                    .Where(m => m != null)
                    .Where(m => m.MemberID > 0)
                    .Where(m => m.EnrollmentID.HasValue) // attendance pipeline needs enrollmentId -> memberId
                    .Where(m => IsFullySyncedAcrossDevices(m, activeDeviceIds))
                    .ToList();

                LogHelper.Log($"[SIM] ActiveDevices={activeDevices.Count}, InDevices={checkInDevices.Count}, OutDevices={checkOutDevices.Count}, FullySyncedMembers={fullySyncedMembers.Count}");

                if (fullySyncedMembers.Count == 0)
                {
                    LogHelper.Log("[SIM] No fully synced members (with EnrollmentID) found. Aborting.");
                    SimulationEvents.RaiseSimulationStopped("No fully synced members");
                    return;
                }

                try
                {
                    // Main loop (bounded by 1 hour via hardStop token)
                    while (!hardStop.IsCancellationRequested)
                    {
                        var member = fullySyncedMembers[_rng.Next(fullySyncedMembers.Count)];
                        var memberName = _db.GetMemberName(member.MemberID) ?? $"Member #{member.MemberID}";

                        // Choose scenario
                        int scenario = _rng.Next(1, 7);

                        switch (scenario)
                        {
                            case 1:
                                SimulationEvents.RaiseScenario("Normal check-in/out", member.MemberID, memberName);
                                await Scenario_NormalCheckInOut(fn, member, checkInDevices, checkOutDevices, hardStop);
                                break;

                            case 2:
                                SimulationEvents.RaiseScenario("Duplicate & rapid check-in", member.MemberID, memberName);
                                await Scenario_DuplicateAndRapidCheckIn(fn, member, checkInDevices, hardStop);
                                break;

                            case 3:
                                SimulationEvents.RaiseScenario("Check-out with no open session", member.MemberID, memberName);
                                await Scenario_CheckOutWithoutOpenSession(fn, member, checkOutDevices, hardStop);
                                break;

                            case 4:
                                SimulationEvents.RaiseScenario("Check-in then immediate check-out", member.MemberID, memberName);
                                await Scenario_CheckInThenImmediateCheckOut(fn, member, checkInDevices, checkOutDevices, hardStop);
                                break;

                            case 5:
                                SimulationEvents.RaiseScenario("Duplicate check-out", member.MemberID, memberName);
                                await Scenario_DuplicateCheckOut(fn, member, checkOutDevices, hardStop);
                                break;

                            default:
                                SimulationEvents.RaiseScenario("Random burst", member.MemberID, memberName);
                                await Scenario_RandomBurst(fn, member, checkInDevices, checkOutDevices, hardStop);
                                break;
                        }

                        await Task.Delay(TimeSpan.FromSeconds(_rng.Next(1, 4)), hardStop);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when either the caller cancels or the 1-hour limit is reached
                }
                finally
                {
                    LogHelper.Log("[SIM] Simulation stopped (cancellation or limit reached).");
                    SimulationEvents.RaiseSimulationStopped("Cancelled or duration limit reached");
                }
            }
        }

        private bool IsAction(int machineNo, string expected)
        {
            var action = _db.GetDeviceAttendanceAction(machineNo);
            return string.Equals(action, expected, StringComparison.OrdinalIgnoreCase);
        }

        private static bool IsFullySyncedAcrossDevices(Member member, HashSet<int> activeDeviceIds)
        {
            if (member.SyncStatus == null || member.SyncStatus.Count == 0)
                return false;

            foreach (var did in activeDeviceIds)
            {
                bool ok;
                if (!member.SyncStatus.TryGetValue(did, out ok) || !ok)
                    return false;
            }

            return true;
        }

        private Device Pick(List<Device> devices)
        {
            if (devices == null || devices.Count == 0) return null;
            return devices[_rng.Next(devices.Count)];
        }

        private async Task EmitAsync(AttendenceServerFunctions fn, Device device, Member member, int enroll, DateTime deviceTime, CancellationToken token)
        {
            token.ThrowIfCancellationRequested();

            var rec = new AttendenceServerFunctions.AttendanceRecord
            {
                DevID = device.MachineNo,
                IP = device.DeviceIP + ":" + device.Port,
                Enroll = enroll,
                Time = deviceTime
            };

            var action = _db.GetDeviceAttendanceAction(device.MachineNo) ?? "?";
            var memberName = _db.GetMemberName(member.MemberID) ?? $"Member #{member.MemberID}";

            LogHelper.Log($"[SIM] Emit -> DevID={rec.DevID}, Enroll={rec.Enroll}, Time={rec.Time:yyyy-MM-dd HH:mm:ss}");
            SimulationEvents.RaiseEmitted(new SimulationEvents.EmitEventArgs
            {
                Time = deviceTime,
                Action = action,
                MachineNo = device.MachineNo,
                DeviceIP = device.DeviceIP,
                Port = device.Port,
                EnrollmentId = enroll,
                MemberId = member.MemberID,
                MemberName = memberName
            });

            fn.Process(rec);

            // Give async-void pipeline time to execute and commit to DB
            await Task.Delay(1000, token).ConfigureAwait(false);

            // Try to surface pairing info (best-effort)
            try
            {
                string attendanceId = null;
                DateTime? pairedCheckIn = null;
                bool paired = false;
                string note = null;

                if (string.Equals(action, "check-in", StringComparison.OrdinalIgnoreCase))
                {
                    attendanceId = _db.GetAttendanceIdForMember(member.MemberID, checkIn: deviceTime);
                    note = attendanceId != null ? "Check-in created" : "Check-in not recorded (filtered/duplicate?)";
                    SimulationEvents.RaisePaired(new SimulationEvents.PairingEventArgs
                    {
                        AttendanceId = attendanceId,
                        MemberId = member.MemberID,
                        MemberName = memberName,
                        CheckInTime = deviceTime,
                        CheckOutTime = null,
                        Paired = false,
                        Note = note
                    });
                }
                else if (string.Equals(action, "check-out", StringComparison.OrdinalIgnoreCase))
                {
                    attendanceId = _db.GetAttendanceIdForMember(member.MemberID, checkOut: deviceTime);
                    if (attendanceId != null)
                    {
                        pairedCheckIn = _db.GetPairedCheckInTime(member.MemberID, deviceTime);
                        paired = pairedCheckIn.HasValue;
                        note = paired ? "Check-out paired" : "Check-out recorded but no pair found";
                    }
                    else
                    {
                        note = "Check-out rejected/ignored (no open session?)";
                    }

                    SimulationEvents.RaisePaired(new SimulationEvents.PairingEventArgs
                    {
                        AttendanceId = attendanceId,
                        MemberId = member.MemberID,
                        MemberName = memberName,
                        CheckInTime = pairedCheckIn,
                        CheckOutTime = deviceTime,
                        Paired = paired,
                        Note = note
                    });
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[SIM] Pairing probe failed: {ex.Message}");
            }

            // Give async-void pipeline time to execute
            await Task.Delay(200, token);
        }

        private async Task Scenario_NormalCheckInOut(
            AttendenceServerFunctions fn,
            Member m,
            List<Device> inDevs,
            List<Device> outDevs,
            CancellationToken token)
        {
            var inDev = Pick(inDevs);
            var outDev = Pick(outDevs);

            if (inDev == null && outDev == null) return;

            DateTime t0 = DateTime.Now.AddSeconds(-_rng.Next(1, 10));

            if (inDev != null)
                await EmitAsync(fn, inDev, m, m.EnrollmentID.Value, t0, token);

            if (outDev != null)
                await EmitAsync(fn, outDev, m, m.EnrollmentID.Value, t0.AddSeconds(_rng.Next(20, 120)), token);
        }

        private async Task Scenario_DuplicateAndRapidCheckIn(
            AttendenceServerFunctions fn,
            Member m,
            List<Device> inDevs,
            CancellationToken token)
        {
            var dev = Pick(inDevs);
            if (dev == null) return;

            DateTime t = DateTime.Now;

            // Exact duplicate timestamp (duplicate/sync guard)
            await EmitAsync(fn, dev, m, m.EnrollmentID.Value, t, token);
            await EmitAsync(fn, dev, m, m.EnrollmentID.Value, t, token);

            // Rapid follow-up (debounce-like behavior)
            await EmitAsync(fn, dev, m, m.EnrollmentID.Value, t.AddSeconds(1), token);
        }

        private async Task Scenario_CheckOutWithoutOpenSession(
            AttendenceServerFunctions fn,
            Member m,
            List<Device> outDevs,
            CancellationToken token)
        {
            var dev = Pick(outDevs);
            if (dev == null) return;

            // Checkout first (should often be rejected due to no open session)
            await EmitAsync(fn, dev, m, m.EnrollmentID.Value, DateTime.Now, token);
        }

        private async Task Scenario_CheckInThenImmediateCheckOut(
            AttendenceServerFunctions fn,
            Member m,
            List<Device> inDevs,
            List<Device> outDevs,
            CancellationToken token)
        {
            var inDev = Pick(inDevs);
            var outDev = Pick(outDevs);

            if (inDev == null || outDev == null) return;

            DateTime t = DateTime.Now;

            // Check-in
            await EmitAsync(fn, inDev, m, m.EnrollmentID.Value, t, token);

            // Immediate checkout (should trigger debounce protections)
            await EmitAsync(fn, outDev, m, m.EnrollmentID.Value, t.AddSeconds(2), token);

            // Later checkout (should typically pass)
            await EmitAsync(fn, outDev, m, m.EnrollmentID.Value, t.AddSeconds(40), token);
        }

        private async Task Scenario_DuplicateCheckOut(
            AttendenceServerFunctions fn,
            Member m,
            List<Device> outDevs,
            CancellationToken token)
        {
            var dev = Pick(outDevs);
            if (dev == null) return;

            DateTime t = DateTime.Now;

            await EmitAsync(fn, dev, m, m.EnrollmentID.Value, t, token);
            await EmitAsync(fn, dev, m, m.EnrollmentID.Value, t, token);
            await EmitAsync(fn, dev, m, m.EnrollmentID.Value, t.AddSeconds(1), token);
        }

        private async Task Scenario_RandomBurst(
            AttendenceServerFunctions fn,
            Member m,
            List<Device> inDevs,
            List<Device> outDevs,
            CancellationToken token)
        {
            int count = _rng.Next(3, 7);
            DateTime baseTime = DateTime.Now.AddSeconds(-_rng.Next(1, 5));

            for (int i = 0; i < count; i++)
            {
                token.ThrowIfCancellationRequested();

                // Randomly pick in/out device; fallback to whichever exists
                Device dev;
                if (_rng.NextDouble() < 0.5)
                    dev = Pick(inDevs) ?? Pick(outDevs);
                else
                    dev = Pick(outDevs) ?? Pick(inDevs);

                if (dev == null) return;

                await EmitAsync(fn, dev, m, m.EnrollmentID.Value, baseTime.AddSeconds(_rng.Next(0, 4)), token);
                await Task.Delay(_rng.Next(150, 600), token);
            }
        }
    }
}                           