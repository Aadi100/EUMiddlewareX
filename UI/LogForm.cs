using System;
using System.Windows.Forms;
using smart_attendance_middleware.Classes;

namespace smart_attendance_middleware.UI
{
    public partial class LogForm : Form
    {
        // Lines retained in this form's textbox in normal operation. Change this value to
        // allow more on-screen history; ignored entirely while AppSession.IsDebugMode is
        // true, in which case history is unlimited (same convention as WebsocketServer's
        // AppendCapped/MaxUiLogLines).
        public static int MaxLogLines = 1;

        public LogForm()
        {
            InitializeComponent();
            LogHelper.LogUpdated += AppendLogToTextBox;
            this.FormClosing += (s, e) => {
                LogHelper.LogUpdated -= AppendLogToTextBox;
            };
        }

        private void AppendLogToTextBox(string logMessage)
        {
            if (txtLog.InvokeRequired)
            {
                txtLog.Invoke(new Action(() => AppendLogToTextBox(logMessage)));
            }
            else
            {
                if (AppSession.IsDebugMode)
                {
                    txtLog.AppendText(logMessage + Environment.NewLine);
                }
                else if (MaxLogLines <= 1)
                {
                    txtLog.Text = logMessage;
                }
                else
                {
                    txtLog.AppendText(logMessage + Environment.NewLine);
                    TrimLogTextBox();
                }
                txtLog.SelectionStart = txtLog.TextLength;
                txtLog.ScrollToCaret();
            }
        }

        private void TrimLogTextBox()
        {
            if (txtLog.Lines.Length > MaxLogLines)
            {
                string[] newLines = new string[MaxLogLines];
                Array.Copy(txtLog.Lines, txtLog.Lines.Length - MaxLogLines, newLines, 0, MaxLogLines);
                txtLog.Lines = newLines;
            }
        }
    }
}
