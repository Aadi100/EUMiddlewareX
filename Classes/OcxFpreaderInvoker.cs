using AxFPREADERLib;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware.Helpers
{
    /// <summary>
    /// Executes AxFPREADER (USB card reader) operations in a safe, dedicated STA thread.
    /// Use the optional initializer to set OcxState from resources if required.
    /// </summary>
    public static class OcxFpreaderInvoker
    {
        /// <summary>
        /// Runs an AxFpreader operation that returns a value in a STA-threaded context.
        /// </summary>
        /// <typeparam name="T">Return type.</typeparam>
        /// <param name="action">Operation that uses AxFpreader.</param>
        /// <param name="initializer">
        /// Optional initializer invoked before CreateControl (e.g. set OcxState from resources).
        /// Example:
        ///   var res = new ComponentResourceManager(typeof(DeviceManager));
        ///   reader.OcxState = (AxHost.State)res.GetObject("axFpreader1.OcxState");
        /// </param>
        public static Task<T> Run<T>(Func<AxFpreader, T> action, Action<AxFpreader> initializer = null)
        {
            var tcs = new TaskCompletionSource<T>();

            Thread staThread = new Thread(() =>
            {
                try
                {
                    using (var dummyForm = new Form
                    {
                        Width = 0,
                        Height = 0,
                        ShowInTaskbar = false,
                        WindowState = FormWindowState.Minimized
                    })
                    {
                        var reader = new AxFpreader();
                        dummyForm.Controls.Add(reader);

                        dummyForm.Load += (s, e) =>
                        {
                            try
                            {
                                initializer?.Invoke(reader);   // set OcxState if needed
                                reader.CreateControl();        // create OCX window

                                var result = action(reader);
                                tcs.SetResult(result);
                            }
                            catch (Exception ex)
                            {
                                tcs.SetException(ex);
                            }
                            finally
                            {
                                try { reader.DisConnectUsb(); } catch { /* ignore */ }
                                dummyForm.Close();
                            }
                        };

                        Application.Run(dummyForm);
                    }
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
        /// Runs an AxFpreader action without a return value in a STA-threaded context.
        /// </summary>
        /// <param name="action">Action that uses AxFpreader.</param>
        /// <param name="initializer">Optional initializer to set OcxState, etc.</param>
        public static Task Run(Action<AxFpreader> action, Action<AxFpreader> initializer = null)
        {
            return Run<bool>(reader =>
            {
                action(reader);
                return true;
            }, initializer);
        }
    }
}