using System;
using System.Threading;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    /// <summary>
    /// Hosts WebsocketServer on its own STA thread with a message loop.
    /// </summary>
    public static class WebsocketServerThreadManager
    {
        private static Thread _thread;
        private static ManualResetEventSlim _readyEvent = new ManualResetEventSlim(false);

        /// <summary>Fires once after the WebSocket server is listening and the initial scan completes.</summary>
        public static event Action Ready;

        /// <summary>
        /// Fires whenever a device connects or disconnects.
        /// Args: (connectedCount, totalDbDeviceCount)
        /// </summary>
        public static event Action<int, int> DeviceConnectionChanged;

        public static bool IsRunning => _thread != null && _thread.IsAlive;

        private static bool _showUI = false;

        public static void Start(bool waitForReady = false, bool showUI = false)
        {
            if (IsRunning) return;

            _showUI = showUI;
            _readyEvent.Reset();
            _thread = new Thread(ThreadMain);
            _thread.IsBackground = true;
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Name = "WebsocketServerThread";
            _thread.Start();

            if (waitForReady)
                _readyEvent.Wait(TimeSpan.FromSeconds(20));
            else
            {
                // Fire-and-forget with async timeout logging
                ThreadPool.RegisterWaitForSingleObject(
                    _readyEvent.WaitHandle,
                    (state, timedOut) =>
                    {
                        if (timedOut)
                            LogHelper.Log("[WebsocketServerThread] Startup timeout (async).");
                        else
                            LogHelper.Log("[WebsocketServerThread] Ready (async).");
                    },
                    null,
                    TimeSpan.FromSeconds(20),
                    executeOnlyOnce: true);
            }
        }

        private static void ThreadMain()
        {
            try
            {
                var form = new WebsocketServer();
                if (!_showUI)
                {
                    form.WindowState = System.Windows.Forms.FormWindowState.Minimized;
                    form.ShowInTaskbar = false;
                }
                Application.Run(form);
            }
            catch (Exception ex)
            {
                LogHelper.Log("[WebsocketServerThread] Fatal thread error: " + ex.Message);
            }
            finally
            {
                _readyEvent.Set();
            }
        }

        /// <summary>Called by WebsocketServer once the HTTP listener is up and scan is done.</summary>
        public static void NotifyReady()
        {
            _readyEvent.Set();
            try { Ready?.Invoke(); }
            catch (Exception ex) { LogHelper.Log("[WebsocketServerThread] Ready event error: " + ex.Message); }
        }

        /// <summary>Called by WebsocketServer when a device connects or disconnects.</summary>
        public static void NotifyDeviceConnectionChanged(int connectedCount, int totalDbDevices)
        {
            try { DeviceConnectionChanged?.Invoke(connectedCount, totalDbDevices); }
            catch (Exception ex) { LogHelper.Log("[WebsocketServerThread] DeviceConnectionChanged event error: " + ex.Message); }
        }

        public static void Stop()
        {
            var inst = WebsocketServer.Instance;
            if (inst == null || inst.IsDisposed) return;
            try
            {
                if (inst.InvokeRequired)
                    inst.BeginInvoke(new Action(() => { if (!inst.IsDisposed) inst.Close(); }));
                else
                    inst.Close();
            }
            catch (Exception ex)
            {
                LogHelper.Log("[WebsocketServerThread] Stop error: " + ex.Message);
            }
        }
    }
}
