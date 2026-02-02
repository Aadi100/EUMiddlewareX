using AxFPREADERLib;
using smart_attendance_middleware.Classes;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SQLite;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace smart_attendance_middleware
{
    public partial class Fingerprint : Form
    {
        string strFPImagepath;
        DatabaseHelper dbHelper;

        public string EnrollmentID { get; set; }

        // Cancellation and reentrancy guards
        private CancellationTokenSource _cts;
        private bool _operationInProgress;

        public Fingerprint()
        {
            InitializeComponent();
            Message.Text = "";

            // Initialize database helper
            dbHelper = new DatabaseHelper();
            try
            {
                dbHelper.InitializeDatabase();
            }
            catch (Exception ex)
            {
                Message.Text = $"Database initialization error: {ex.Message}";
                Message.Update();
            }

            strFPImagepath = System.Environment.CurrentDirectory;
            strFPImagepath += "\\test.bmp";

            TB_FP_NUM.Text = "0";

            // Subscribe to safe shutdown
            this.FormClosing += Fingerprint_FormClosing;

            // Adjust UI layout: resize enroll button and place retry to its right
            try
            {
                // Assuming button1 is the enroll fingerprint button
                if (this.button1 != null)
                {
                    this.button1.Size = new Size(140, this.button1.Size.Height);
                    // Place retry button to the right with small margin
                    if (this.btn_retry != null)
                    {
                        this.btn_retry.Size = new Size(120, this.button1.Size.Height);
                        this.btn_retry.Location = new Point(this.button1.Right + 10, this.button1.Top);
                    }
                }
            }
            catch { }
        }

        private void Fingerprint_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                _cts?.Cancel();
                _operationInProgress = false;
                CloseDevice();
            }
            catch
            {
                // swallow any exception during closing
            }
        }

        // Tries to connect to the device repeatedly until timeout, keeping UI responsive
        private bool EnsureDeviceConnectedWithRetry(int timeoutMs, CancellationToken token)
        {
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                if (token.IsCancellationRequested) return false;
                bool connected = false;
                try
                {
                    connected = axFpreader1.ConnectUsb();
                }
                catch (Exception ex)
                {
                    Message.Text = $"ConnectUsb error: {ex.Message}";
                    Message.Update();
                }
                if (connected)
                {
                    return true;
                }
                Application.DoEvents();
                Thread.Sleep(200);
            }
            return false;
        }

        // Non-blocking polling with timeout and cancellation
        private bool DetectDeviceWithTimeout(int timeoutMs, CancellationToken token)
        {
            var start = DateTime.UtcNow;
            while ((DateTime.UtcNow - start).TotalMilliseconds < timeoutMs)
            {
                if (token.IsCancellationRequested) return false;
                bool bRet = false;
                try
                {
                    bRet = axFpreader1.DetectFinger();
                }
                catch (Exception ex)
                {
                    Message.Text = $"DetectFinger error: {ex.Message}";
                    Message.Update();
                }
                if (bRet)
                {
                    return true;
                }
                Application.DoEvents();
                Thread.Sleep(50);
            }
            return false;
        }

        private void showFPPicture()
        {
            try
            {
                using (var fs = new FileStream(strFPImagepath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    pictureBox.Image = Image.FromStream(fs);
                    pictureBox.Show();
                    pictureBox.Refresh();
                }
            }
            catch (Exception ex)
            {
                Message.Text = $"Show FP image error: {ex.Message}";
                Message.Update();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (_operationInProgress)
            {
                Message.Text = "Operation already in progress";
                Message.Update();
                return;
            }

            _operationInProgress = true;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            // Add at the beginning
            aiChatLogger.AddFingerprintMessage("Starting fingerprint enrollment...");

            try
            {
                bool bRet;
                int verIndex = 0;
                string strVersion = "";

                // Retry connecting for up to 10 seconds to handle device unplug/replug during runtime
                if (!EnsureDeviceConnectedWithRetry(10000, token))
                {
                    Message.Text = "Device not connected";
                    Message.Update();
                    return;
                }

                try
                {
                    bRet = axFpreader1.GetVersion(ref verIndex, ref strVersion);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetVersion error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (!bRet)
                {
                    Message.Text = "No device";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                try { axFpreader1.OnoffLED(1); } catch (Exception ex) { Message.Text = $"OnoffLED error: {ex.Message}"; Message.Update(); }
                Message.Text = "press finger 1#";
                Message.Update();

                if (!DetectDeviceWithTimeout(15000, token))
                {
                    Message.Text = "Finger detection timeout";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                try
                {
                    bRet = axFpreader1.GetImage(strFPImagepath);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetImage error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (bRet)
                {
                    showFPPicture();
                }
                else
                {
                    Message.Text = "Get image fail";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                try
                {
                    bRet = axFpreader1.GetTemplet(0);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetTemplet(0) error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (!bRet)
                {
                    Message.Text = "Get Templet Fail";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                // Check for duplicate fingerprints in database
                List<byte[]> allFingerprints = null;
                try
                {
                    allFingerprints = dbHelper.GetAllBinaryTemplates();
                }
                catch (Exception ex)
                {
                    Message.Text = $"DB read error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                int enrollNumber = 0;
                int fingerNumber = 0;
                int privilege = 0;
                try
                {
                    enrollNumber = Int32.Parse(TB_ID.Text.ToString());
                    fingerNumber = Int32.Parse(TB_FP_NUM.Text.ToString());
                }
                catch (Exception ex)
                {
                    Message.Text = $"Invalid input: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                if (allFingerprints != null && allFingerprints.Count > 0)
                {
                    foreach (var template in allFingerprints)
                    {
                        bool isDup = false;
                        try
                        {
                            object obj = new System.Runtime.InteropServices.VariantWrapper(template);
                            isDup = axFpreader1.VerifyFinger(ref obj);
                        }
                        catch (Exception ex)
                        {
                            Message.Text = $"VerifyFinger error: {ex.Message}";
                            Message.Update();
                        }
                        if (isDup)
                        {
                            Message.Text = "FP DOUBLE (duplicate)";
                            Message.Update();
                            CloseDevice();
                            return;
                        }
                        Application.DoEvents();
                    }
                }

                Message.Text = "press finger 2 #";
                Message.Update();

                if (!DetectDeviceWithTimeout(15000, token))
                {
                    Message.Text = "Finger detection timeout";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                try
                {
                    bRet = axFpreader1.GetImage(strFPImagepath);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetImage error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (bRet)
                {
                    showFPPicture();
                }
                else
                {
                    Message.Text = "Get image fail";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                try
                {
                    bRet = axFpreader1.GetTemplet(1);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetTemplet(1) error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (!bRet)
                {
                    Message.Text = "Get Templet Fail";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                if (verIndex == 1)
                {
                    Message.Text = "press finger 3 #";
                    Message.Update();

                    if (!DetectDeviceWithTimeout(15000, token))
                    {
                        Message.Text = "Finger detection timeout";
                        Message.Update();
                        CloseDevice();
                        return;
                    }

                    try
                    {
                        bRet = axFpreader1.GetImage(strFPImagepath);
                    }
                    catch (Exception ex)
                    {
                        Message.Text = $"GetImage error: {ex.Message}";
                        Message.Update();
                        CloseDevice();
                        return;
                    }
                    if (bRet)
                    {
                        showFPPicture();
                    }
                    else
                    {
                        Message.Text = "Get image fail";
                        Message.Update();
                        CloseDevice();
                        return;
                    }

                    try
                    {
                        bRet = axFpreader1.GetTemplet(2);
                    }
                    catch (Exception ex)
                    {
                        Message.Text = $"GetTemplet(2) error: {ex.Message}";
                        Message.Update();
                        CloseDevice();
                        return;
                    }
                    if (!bRet)
                    {
                        Message.Text = "Get Templet Fail";
                        Message.Update();
                        CloseDevice();
                        return;
                    }
                }

                // Get merged template data
                int[] dwData = new int[1420 / 4 * 5 + 4];
                object objData = new System.Runtime.InteropServices.VariantWrapper(dwData);
                try
                {
                    bRet = axFpreader1.GetMergeChar(ref objData);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetMergeChar error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (!bRet)
                {
                    Message.Text = "MergeChar Error";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                // Convert int[] to byte[] safely
                byte[] templateData = null;
                IntPtr ptr = IntPtr.Zero;
                try
                {
                    dwData = (int[])objData;
                    templateData = new byte[1420];
                    ptr = Marshal.AllocHGlobal(templateData.Length);
                    Marshal.Copy(dwData, 0, ptr, 1420 / 4);
                    Marshal.Copy(ptr, templateData, 0, 1420);
                }
                catch (Exception ex)
                {
                    Message.Text = $"Template conversion error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                finally
                {
                    if (ptr != IntPtr.Zero)
                    {
                        try { Marshal.FreeHGlobal(ptr); } catch { }
                    }
                }

                // Save to database
                var fingerprint = new FingerprintData
                {
                    EnrollNumber = enrollNumber,
                    FingerNumber = fingerNumber,
                    Privilege = privilege,
                    TemplateData = templateData
                };

                bool success = false;
                try
                {
                    success = dbHelper.AddOrUpdateFingerprint(fingerprint);
                }
                catch (Exception ex)
                {
                    aiChatLogger.AddErrorMessage($"Failed to enroll fingerprint for user {enrollNumber}: {ex.Message}");
                    Message.Text = "Failed to save FPData to database";
                    Message.Update();
                }

                if (success)
                {
                    aiChatLogger.AddSuccessMessage($"Fingerprint enrolled successfully for user {enrollNumber}!");
                    Message.Text = "Added new FPData to database";
                }
                else if (string.IsNullOrEmpty(Message.Text))
                {
                    Message.Text = "Failed to save FPData to database";
                }
                Message.Update();
            }
            finally
            {
                CloseDevice();
                _operationInProgress = false;
                _cts = null;
            }
        }

        private void EnrollFPWithStr_Click(object sender, EventArgs e)
        {
            if (_operationInProgress)
            {
                Message.Text = "Operation already in progress";
                Message.Update();
                return;
            }

            _operationInProgress = true;
            _cts = new CancellationTokenSource();
            var token = _cts.Token;

            try
            {
                bool bRet;
                int verIndex = 0;
                string strVersion = "";

                // Retry connecting for up to 10 seconds to handle device unplug/replug during runtime
                if (!EnsureDeviceConnectedWithRetry(10000, token))
                {
                    Message.Text = "Device not connected";
                    Message.Update();
                    return;
                }

                try
                {
                    bRet = axFpreader1.GetVersion(ref verIndex, ref strVersion);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetVersion error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (!bRet)
                {
                    Message.Text = "No device";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                try { axFpreader1.OnoffLED(1); } catch (Exception ex) { Message.Text = $"OnoffLED error: {ex.Message}"; Message.Update(); }
                Message.Text = "press finger 1#";
                Message.Update();

                if (!DetectDeviceWithTimeout(15000, token))
                {
                    Message.Text = "Finger detection timeout";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                try
                {
                    bRet = axFpreader1.GetImage(strFPImagepath);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetImage error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (bRet)
                {
                    showFPPicture();
                }
                else
                {
                    Message.Text = "Get image fail";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                try
                {
                    bRet = axFpreader1.GetTemplet(0);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetTemplet(0) error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (!bRet)
                {
                    Message.Text = "Get Templet Fail";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                // Check for duplicate fingerprints in database (string)
                List<string> allFingerprints = null;
                try
                {
                    allFingerprints = dbHelper.GetAllStringTemplates();
                }
                catch (Exception ex)
                {
                    Message.Text = $"DB read error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                int enrollNumber = 0;
                int fingerNumber = 0;
                int privilege = 0;
                try
                {
                    enrollNumber = Int32.Parse(TB_ID.Text.ToString());
                    fingerNumber = Int32.Parse(TB_FP_NUM.Text.ToString());
                }
                catch (Exception ex)
                {
                    Message.Text = $"Invalid input: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                if (allFingerprints != null && allFingerprints.Count > 0)
                {
                    foreach (var template in allFingerprints)
                    {
                        bool isDup = false;
                        try
                        {
                            object strFPData = new System.Runtime.InteropServices.VariantWrapper(template);
                            isDup = axFpreader1.VerifyFingerWithCloud(ref strFPData);
                        }
                        catch (Exception ex)
                        {
                            Message.Text = $"VerifyFingerWithCloud error: {ex.Message}";
                            Message.Update();
                        }
                        if (isDup)
                        {
                            Message.Text = "FP DOUBLE (duplicate)";
                            Message.Update();
                            CloseDevice();
                            return;
                        }
                        Application.DoEvents();
                    }
                }

                Message.Text = "press finger 2 #";
                Message.Update();

                if (!DetectDeviceWithTimeout(15000, token))
                {
                    Message.Text = "Finger detection timeout";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                try
                {
                    bRet = axFpreader1.GetImage(strFPImagepath);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetImage error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (bRet)
                {
                    showFPPicture();
                }
                else
                {
                    Message.Text = "Get image fail";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                try
                {
                    bRet = axFpreader1.GetTemplet(1);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetTemplet(1) error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (!bRet)
                {
                    Message.Text = "Get Templet Fail";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                if (verIndex == 1)
                {
                    Message.Text = "press finger 3 #";
                    Message.Update();

                    if (!DetectDeviceWithTimeout(15000, token))
                    {
                        Message.Text = "Finger detection timeout";
                        Message.Update();
                        CloseDevice();
                        return;
                    }

                    try
                    {
                        bRet = axFpreader1.GetImage(strFPImagepath);
                    }
                    catch (Exception ex)
                    {
                        Message.Text = $"GetImage error: {ex.Message}";
                        Message.Update();
                        CloseDevice();
                        return;
                    }
                    if (bRet)
                    {
                        showFPPicture();
                    }
                    else
                    {
                        Message.Text = "Get image fail";
                        Message.Update();
                        CloseDevice();
                        return;
                    }

                    try
                    {
                        bRet = axFpreader1.GetTemplet(2);
                    }
                    catch (Exception ex)
                    {
                        Message.Text = $"GetTemplet(2) error: {ex.Message}";
                        Message.Update();
                        CloseDevice();
                        return;
                    }
                    if (!bRet)
                    {
                        Message.Text = "Get Templet Fail";
                        Message.Update();
                        CloseDevice();
                        return;
                    }
                }

                // Get merged template data as string
                string strInfo1 = "";
                object strInfo = new System.Runtime.InteropServices.VariantWrapper(strInfo1);
                try
                {
                    bRet = axFpreader1.GetMergeCharWithCloud(ref strInfo);
                }
                catch (Exception ex)
                {
                    Message.Text = $"GetMergeCharWithCloud error: {ex.Message}";
                    Message.Update();
                    CloseDevice();
                    return;
                }
                if (!bRet)
                {
                    Message.Text = "MergeChar Error";
                    Message.Update();
                    CloseDevice();
                    return;
                }

                // Save to database
                var fingerprint = new FingerprintData
                {
                    EnrollNumber = enrollNumber,
                    FingerNumber = fingerNumber,
                    Privilege = privilege,
                    TemplateString = strInfo.ToString()
                };

                bool success = false;
                try
                {
                    success = dbHelper.AddOrUpdateFingerprint(fingerprint);
                }
                catch (Exception ex)
                {
                    Message.Text = $"Failed to save FPDataString to database: {ex.Message}";
                    Message.Update();
                }
                if (success)
                {
                    Message.Text = "Added new FPDataString to database";
                }
                else if (string.IsNullOrEmpty(Message.Text))
                {
                    Message.Text = "Failed to save FPDataString to database";
                }
                Message.Update();
            }
            finally
            {
                CloseDevice();
                _operationInProgress = false;
                _cts = null;
            }
        }

        private void CloseDevice()
        {
            try
            {
                bool bRet;
                try { bRet = axFpreader1.OnoffLED(0); } catch { }
                try { bRet = axFpreader1.DisConnectUsb(); } catch { }
            }
            catch
            {
                // swallow any exception in close
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            if (_operationInProgress)
            {
                Message.Text = "Operation already in progress";
                Message.Update();
                return;
            }

            DialogResult dr;
            dr = MessageBox.Show("Continue?", "Delete All data in database? ", MessageBoxButtons.YesNo, MessageBoxIcon.Asterisk);
            if (dr == DialogResult.No)
            {
                return;
            }

            try
            {
                bool success = dbHelper.DeleteAllFingerprints();
                if (success)
                {
                    Message.Text = "Deleted all fingerprint data from database";
                }
                else
                {
                    Message.Text = "Failed to delete fingerprint data";
                }
                Message.Update();
            }
            catch (Exception ex)
            {
                Message.Text = $"Delete error: {ex.Message}";
                Message.Update();
            }
        }

        private void btn_setmonitor_Click(object sender, EventArgs e)
        {
            if (_operationInProgress)
            {
                Message.Text = "Operation already in progress";
                Message.Update();
                return;
            }

            try
            {
                bool bRet;
                int verIndex = 0;
                string strVersion = "";

                try { bRet = axFpreader1.ConnectUsb(); } catch (Exception ex) { Message.Text = $"ConnectUsb error: {ex.Message}"; Message.Update(); return; }
                try { bRet = axFpreader1.SetMonitorMode(10); } catch (Exception ex) { Message.Text = $"SetMonitorMode error: {ex.Message}"; Message.Update(); }
                Message.Text = "setmonitormode ok ";
                Message.Update();
            }
            finally
            {
                CloseDevice();
            }
        }

        private void Fingerprint_Load(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(EnrollmentID))
            {
                TB_ID.Text = EnrollmentID;
            }
        }

        private void btn_retry_Click(object sender, EventArgs e)
        {
            // Cancel ongoing work if any
            if (_operationInProgress && _cts != null)
            {
                try { _cts.Cancel(); } catch { }
            }

            // Ensure device is closed
            CloseDevice();

            // Reset flags
            _operationInProgress = false;
            _cts = null;

            // Clear status
            Message.Text = "Operation canceled. You can start again.";
            Message.Update();
        }
    }
}