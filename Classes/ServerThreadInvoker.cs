using AxFPCLOCK_SVRLib;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware.Helpers
{
    public static class ServerThreadInvoker
    {
        // Core generic runner
        public static Task<T> Run<T>(string ip, Func<AxFPCLOCK_Svr, T> action, bool requirePing = true)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));

            var tcs = new TaskCompletionSource<T>();

            var thread = new Thread(() =>
            {
                try
                {
                    using (var hostForm = new Form
                    {
                        ShowInTaskbar = false,
                        StartPosition = FormStartPosition.Manual,
                        Location = new System.Drawing.Point(-2000, -2000), // keep invisible
                        Size = new System.Drawing.Size(0, 0)
                    })
                    {
                        var ocx = new AxFPCLOCK_Svr();
                        hostForm.Controls.Add(ocx);

                        hostForm.Load += (s, e) =>
                        {
                            try
                            {
                                ocx.CreateControl(); // ensure OCX handle
                                var result = action(ocx);
                                tcs.TrySetResult(result);
                            }
                            catch (Exception ex)
                            {
                                tcs.TrySetException(ex);
                            }
                            finally
                            {
                                hostForm.Close();
                            }
                        };

                        Application.Run(hostForm);
                    }
                }
                catch (Exception ex)
                {
                    tcs.TrySetException(ex);
                }
            });

            thread.IsBackground = true;
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();

            return tcs.Task;
        }

        // Convenience void-style wrapper
        public static Task Run(string ip, Action<AxFPCLOCK_Svr> action, bool requirePing = true)
        {
            if (action == null) throw new ArgumentNullException(nameof(action));
            return Run<object>(ip, ocx =>
            {
                action(ocx);
                return null;
            }, requirePing);
        }
    }
}