using System;
using System.Windows.Forms;
using smart_attendance_middleware.Classes;

namespace smart_attendance_middleware.UI
{
    public partial class LogForm : Form
    {
        private const int MaxLogLines = 10000;

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
                txtLog.AppendText(logMessage + Environment.NewLine);
                TrimLogTextBox();
            }
        }

        private void TrimLogTextBox()
        {
            if (txtLog.Lines.Length > MaxLogLines)
            {
                string[] newLines = new string[MaxLogLines];
                Array.Copy(txtLog.Lines, txtLog.Lines.Length - MaxLogLines, newLines, 0, MaxLogLines);
                txtLog.Lines = newLines;
                txtLog.SelectionStart = txtLog.TextLength;
                txtLog.ScrollToCaret();
            }
        }
    }
}
