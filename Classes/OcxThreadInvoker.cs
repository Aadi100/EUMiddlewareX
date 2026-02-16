using AxFP_CLOCKLib;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using smart_attendance_middleware.Helpers;

namespace smart_attendance_middleware.Helpers
{
    /// <summary>
    /// Executes OCX-based operations in a safe, dedicated STA thread. 
    /// Includes optional ping check before connecting.
    /// </summary>
    public static class OcxThreadInvoker
    {
        /// <summary>
        /// Runs an OCX operation that returns a value using a safe STA-threaded context, 
        /// with optional ping check before execution.
        /// </summary>
        /// <typeparam name="T">Return type of the operation.</typeparam>
        /// <param name="ip">IP address of the device to optionally ping.</param>
        /// <param name="action">The function that performs the OCX operation.</param>
        /// <param name="requirePing">Set to true if you want to ping the device before connecting (default: true).</param>
        /// <returns>Result of the action or throws if failed.</returns>
        public static Task<T> Run<T>(string ip, Func<AxFP_CLOCK, T> action, bool requirePing = true)
        {
            var tcs = new TaskCompletionSource<T>();

            // ✅ Step 1: Optional Ping check before anything else
            if (requirePing && !NetworkHelper.IsDeviceReachable(ip))
            {
                tcs.SetException(new Exception("Device is not reachable. Please check the IP address, ensure the device is powered on, and confirm it’s connected to the same network."
));
                return tcs.Task;
            }

            // ✅ Step 2: Setup STA thread with OCX control
            Thread staThread = new Thread(() =>
            {
                try
                {
                    Thread.Sleep(2000); // Small delay to ensure thread stability

                    var dummyForm = new Form
                    {
                        Width = 0,
                        Height = 0,
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    };

                    var ocx = new AxFP_CLOCK();
                    dummyForm.Controls.Add(ocx);

                    dummyForm.Load += (s, e) =>
                    {
                        try
                        {
                            ocx.CreateControl(); // Required for OCX
                            var result = action(ocx);
                            tcs.SetResult(result);
                        }
                        catch (Exception ex)
                        {
                            tcs.SetException(ex);
                        }
                        finally
                        {
                            dummyForm.Close();
                        }
                    };

                    Application.Run(dummyForm);
                }
                catch (Exception ex)
                {
                    tcs.SetException(ex);
                }
            });

            staThread.SetApartmentState(ApartmentState.STA);
            staThread.IsBackground = true;
            staThread.Start();

            return tcs.Task;
        }

        /// <summary>
        /// Runs an OCX action without a return value using a safe STA-threaded context.
        /// </summary>
        /// <param name="ip">IP address of the device to optionally ping.</param>
        /// <param name="action">The action to execute using OCX.</param>
        /// <param name="requirePing">Set to true to ping before executing the OCX logic.</param>
        public static async Task Run(string ip, Action<AxFP_CLOCK> action, bool requirePing = true)
        {
            await Run<bool>(ip, ocx =>
            {
                action(ocx);
                return true;
            }, requirePing);
        }
    }
}
