using System;

namespace smart_attendance_middleware.Classes
{
    // Simple event hub to surface simulator activity to a UI (form)
    internal static class SimulationEvents
    {
        public sealed class SimulationStartedEventArgs : EventArgs
        {
            public DateTime UntilUtc { get; set; }
        }

        public sealed class SimulationStoppedEventArgs : EventArgs
        {
            public string Reason { get; set; }
        }

        public sealed class ScenarioEventArgs : EventArgs
        {
            public string ScenarioName { get; set; }
            public int MemberId { get; set; }
            public string MemberName { get; set; }
        }

        public sealed class EmitEventArgs : EventArgs
        {
            public DateTime Time { get; set; }
            public string Action { get; set; } // "check-in"/"check-out"
            public int MachineNo { get; set; }
            public string DeviceIP { get; set; }
            public int Port { get; set; }
            public int EnrollmentId { get; set; }
            public int MemberId { get; set; }
            public string MemberName { get; set; }
        }

        public sealed class PairingEventArgs : EventArgs
        {
            public string AttendanceId { get; set; }
            public int MemberId { get; set; }
            public string MemberName { get; set; }
            public DateTime? CheckInTime { get; set; }
            public DateTime? CheckOutTime { get; set; }
            public bool Paired { get; set; }
            public string Note { get; set; }
        }

        public static event EventHandler<SimulationStartedEventArgs> SimulationStarted;
        public static event EventHandler<SimulationStoppedEventArgs> SimulationStopped;
        public static event EventHandler<ScenarioEventArgs> Scenario;
        public static event EventHandler<EmitEventArgs> Emitted;
        public static event EventHandler<PairingEventArgs> Paired;

        public static void RaiseSimulationStarted(DateTime untilUtc)
            => SimulationStarted?.Invoke(null, new SimulationStartedEventArgs { UntilUtc = untilUtc });

        public static void RaiseSimulationStopped(string reason)
            => SimulationStopped?.Invoke(null, new SimulationStoppedEventArgs { Reason = reason });

        public static void RaiseScenario(string name, int memberId, string memberName)
            => Scenario?.Invoke(null, new ScenarioEventArgs { ScenarioName = name, MemberId = memberId, MemberName = memberName });

        public static void RaiseEmitted(EmitEventArgs args)
            => Emitted?.Invoke(null, args);

        public static void RaisePaired(PairingEventArgs args)
            => Paired?.Invoke(null, args);
    }
}