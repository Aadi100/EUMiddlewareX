using System;
using System.Threading;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    /// <summary>
    /// Hosts AttendanceServerForm on its own STA thread with a message loop.
    /// </summary>
    public static class AttendanceServerThreadManager
    {
        private static Thread _thread;
        private static ManualResetEvent _readyEvent;
        private static bool _showUIRequested;

        public static event Action Ready;

        public static bool IsRunning
        {
            get { return _thread != null && _thread.IsAlive; }
        }

        /// <summary>
        /// Starts the attendance server form on a dedicated STA thread.
        /// Set waitForReady=false to make the call non-blocking (recommended for UI responsiveness).
        /// </summary>
        public static void Start(bool showUI = false, bool waitForReady = true)
        {
            if (IsRunning)
            {
                if (showUI)
                    ShowUI();
                return;
            }

            _showUIRequested = showUI;
            _readyEvent = new ManualResetEvent(false);

            _thread = new Thread(ThreadMain);
            _thread.IsBackground = true;
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();

            if (waitForReady)
            {
                if (!_readyEvent.WaitOne(TimeSpan.FromSeconds(10)))
                {
                    LogHelper.Log("[AttendanceServerThread] Startup timeout (waited).");
                }
            }
            else
            {
                // Fire-and-forget: register a wait callback just for logging
                ThreadPool.RegisterWaitForSingleObject(
                    _readyEvent,
                    (state, timedOut) =>
                    {
                        if (timedOut)
                        {
                            LogHelper.Log("[AttendanceServerThread] Startup timeout (async).");
                        }
                        else
                        {
                            LogHelper.Log("[AttendanceServerThread] Ready (async).");
                        }
                    },
                    null,
                    TimeSpan.FromSeconds(10),
                    true);
            }
        }

        private static void ThreadMain()
        {
            try
            {
                if (!AttendanceServerForm.InitializeGlobal(startImmediately: true, showUI: _showUIRequested))
                {
                    LogHelper.Log("[AttendanceServerThread] InitializeGlobal failed.");
                    _readyEvent.Set();
                    ReadySafeInvoke();
                    return;
                }

                _readyEvent.Set();
                ReadySafeInvoke();

                Application.Run();
            }
            catch (Exception ex)
            {
                LogHelper.Log("[AttendanceServerThread] Fatal thread error: " + ex.Message);
                try { _readyEvent.Set(); } catch { }
                ReadySafeInvoke();
            }
        }

        private static void ReadySafeInvoke()
        {
            try { Ready?.Invoke(); } catch (Exception ex) { LogHelper.Log("[AttendanceServerThread] Ready event error: " + ex.Message); }
        }

        public static void ShowUI()
        {
            var inst = AttendanceServerForm.Instance;
            if (inst == null || inst.IsDisposed) return;
            try
            {
                if (inst.InvokeRequired)
                    inst.BeginInvoke(new Action(() =>
                    {
                        if (!inst.Visible) inst.Show();
                        inst.BringToFront();
                        inst.Focus();
                    }));
                else
                {
                    if (!inst.Visible) inst.Show();
                    inst.BringToFront();
                    inst.Focus();
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("[AttendanceServerThread] ShowUI error: " + ex.Message);
            }
        }

        public static void Stop()
        {
            var inst = AttendanceServerForm.Instance;
            try
            {
                if (inst != null && !inst.IsDisposed)
                {
                    if (inst.InvokeRequired)
                        inst.BeginInvoke(new Action(() => SafeClose(inst)));
                    else
                        SafeClose(inst);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log("[AttendanceServerThread] Stop error: " + ex.Message);
            }
        }

        private static void SafeClose(Form f)
        {
            try
            {
                if (!f.IsDisposed) f.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Log("[AttendanceServerThread] SafeClose error: " + ex.Message);
            }
        }
    }
}