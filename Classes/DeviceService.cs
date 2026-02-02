using AxFP_CLOCKLib;
using smart_attendance_middleware.Helpers;
using smart_attendance_middleware.OCX;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace smart_attendance_middleware.Services
{
    /// <summary>
    /// Provides simplified methods to perform high-level operations on biometric devices.
    /// Internally handles ping, OCX execution, and exception safety.
    /// </summary>
    public static class DeviceService
    {

        /// <summary>
        /// Disables the device for maintenance operations
        /// </summary>
        private static bool DisableDevice(AxFP_CLOCK ocx, int machineNo)
        {
            try
            {
                return ocx.EnableDevice(machineNo, 0); // 0 to disable
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Device disable failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Re-enables the device after operations
        /// </summary>
        private static bool EnableDevice(AxFP_CLOCK ocx, int machineNo)
        {
            try
            {
                return ocx.EnableDevice(machineNo, 1); // 1 to enable
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Device enable failed: {ex.Message}");
                return false;
            }
        }


        /// <summary>
        /// Retrieves the serial number of a biometric device using safe OCX execution.
        /// Automatically checks if the device is reachable before attempting connection.
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port (e.g., 5005)</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password (default is usually "0")</param>
        /// <returns>Trimmed serial number string</returns>
        public static async Task<string> GetSerial(string ip, int port, int machineNo, string password)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                    throw new Exception("SetIPAddress failed.");

                if (!ocx.OpenCommPort(machineNo))
                    throw new Exception("OpenCommPort failed.");

                try
                {
                    // Disable device before reading serial
                    if (!DisableDevice(ocx, machineNo))
                        throw new Exception("Could not disable device for serial retrieval.");

                    string serial = DeviceOperations.GetSerialNumber(ocx, machineNo);
                    return serial;
                }
                finally
                {
                    // Always re-enable device and close port
                    EnableDevice(ocx, machineNo);
                    ocx.CloseCommPort();
                }
            });
        }


        public static async Task<bool> AddFace(string ip, int port, int machineNo, string password, int enrollNumber, string imageFilePath)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                // Validate input parameters
                if (string.IsNullOrWhiteSpace(imageFilePath))
                    throw new ArgumentException("Image file path cannot be empty", nameof(imageFilePath));

                if (!File.Exists(imageFilePath))
                    throw new FileNotFoundException("Facial image file not found", imageFilePath);

                // Read image data
                byte[] imageData;
                try
                {
                    imageData = File.ReadAllBytes(imageFilePath);
                }
                catch (IOException ex)
                {
                    throw new Exception("Failed to read image file", ex);
                }

                // Establish device connection
                if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                    throw new Exception("Failed to set device IP address");

                if (!ocx.OpenCommPort(machineNo))
                    throw new Exception("Failed to open communication port with device");

                try
                {
                    // Prepare image data for device
                    IntPtr photoPtr = Marshal.AllocHGlobal(imageData.Length);
                    try
                    {
                        Marshal.Copy(imageData, 0, photoPtr, imageData.Length);

                        // Disable device during sensitive operation using helper
                        if (!DisableDevice(ocx, machineNo))
                            throw new Exception("Could not disable device for enrollment");

                        // Execute enrollment
                        bool enrollmentSuccess = ocx.SetEnrollPhotoCS(machineNo, enrollNumber, imageData.Length, photoPtr);

                        if (!enrollmentSuccess)
                            throw new Exception("Device reported enrollment failure");

                        return true;
                    }
                    finally
                    {
                        // Clean up unmanaged resources
                        Marshal.FreeHGlobal(photoPtr);
                        // Re-enable device using helper
                        EnableDevice(ocx, machineNo);
                    }
                }
                finally
                {
                    // Ensure communication port is closed
                    ocx.CloseCommPort();
                }
            });
        }



        /// <summary>
        /// Sets the user name for a specific enrollment number on the biometric device.
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <param name="enrollmentNumber">The enrollment number to update</param>
        /// <param name="userName">The name to set (empty string to clear)</param>
        /// <param name="enrollmentMachineId">The machine ID for enrollment (defaults to 0)</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static async Task<bool> SetUserName(string ip, int port, int machineNo, string password,
            int enrollmentNumber, string userName, int enrollmentMachineId = 0)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                    throw new Exception("SetIPAddress failed.");

                if (!ocx.OpenCommPort(machineNo))
                    throw new Exception("OpenCommPort failed.");

                try
                {
                    // Disable device before setting user name
                    if (!DisableDevice(ocx, machineNo))
                        throw new Exception("Could not disable device for setting user name.");

                    // Convert empty string to empty string (consistent with reference code)
                    string nameToSet = string.IsNullOrEmpty(userName) ? "" : userName;
                    object obj = new VariantWrapper(nameToSet);

                    bool result = ocx.SetUserName(0, machineNo, enrollmentNumber, enrollmentMachineId, ref obj);

                    return result;
                }
                finally
                {
                    // Always re-enable device and close port
                    EnableDevice(ocx, machineNo);
                    ocx.CloseCommPort();
                }
            });
        }

        /// <summary>
        /// Enrolls an RFID card number for a specific user on the biometric device
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <param name="enrollmentNumber">The user's enrollment number</param>
        /// <param name="cardNumber">The RFID card number to enroll</param>
        /// <param name="enrollmentMachineId">The machine ID for enrollment (default 1)</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static async Task<bool> AddRfidCard(string ip, int port, int machineNo, string password,
      int enrollmentNumber, long cardNumber, int enrollmentMachineId = 1)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                try
                {
                    // Initialize device connection
                    if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                        throw new Exception($"SetIPAddress failed for {ip}:{port}");

                    if (!ocx.OpenCommPort(machineNo))
                        throw new Exception($"OpenCommPort failed for machine {machineNo}");

                    // Disable device before operation using helper function
                    if (!DisableDevice(ocx, machineNo))
                        throw new Exception("Failed to disable device for enrollment");

                    // Convert card number (handling 32-bit overflow)
                    int convertedCardNum = 0;
                    if (cardNumber > 0)
                    {
                        if (cardNumber > 2147483647)
                        {
                            cardNumber -= 4294967296;
                        }
                        convertedCardNum = (int)cardNumber;
                    }

                    // Prepare empty data buffer (size matching original implementation)
                    int[] emptyData = new int[1420 / 4];
                    object obj = new VariantWrapper(emptyData);

                    // Execute RFID enrollment (backupNum = 11)
                    bool result = ocx.SetEnrollData(
                        machineNo,
                        enrollmentNumber,
                        enrollmentMachineId,
                        11, // BackupNum = 11 for RFID card
                        0,  // Privilege level (0 = normal user)
                        ref obj,
                        convertedCardNum);

                    return result;
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"RFID Enrollment Error - Machine: {machineNo}, " +
                                $"Enrollment: {enrollmentNumber}, Card: {cardNumber}\n" +
                                $"Error: {ex.Message}");
                    throw;
                }
                finally
                {
                    try
                    {
                        // Always re-enable device using helper function and close port
                        EnableDevice(ocx, machineNo);
                        ocx.CloseCommPort();
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Device cleanup error: {ex.Message}");
                    }
                }
            });
        }

        /// <summary>
        /// Enrolls a fingerprint template for a user on the biometric device
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <param name="enrollmentNumber">The user's enrollment number</param>
        /// <param name="fingerNumber">Finger number (1-10 for fingerprints)</param>
        /// <param name="privilege">Privilege level (0=normal user)</param>
        /// <param name="templateData">Fingerprint template data</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static async Task<bool> AddFingerprint(string ip, int port, int machineNo, string password,
            int enrollmentNumber, int fingerNumber, int privilege, byte[] templateData)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                try
                {
                    // Validate input
                    if (templateData == null || templateData.Length == 0)
                        throw new ArgumentException("Fingerprint template data cannot be empty");

                    // Establish connection
                    if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                        throw new Exception($"Failed to connect to device at {ip}:{port}");

                    if (!ocx.OpenCommPort(machineNo))
                        throw new Exception("Failed to open communication port");

                    try
                    {
                        // Disable device during operation
                        if (!DisableDevice(ocx, machineNo))
                            throw new Exception("Could not disable device");

                        // Prepare template data
                        int[] templateIntArray = new int[1420 / 4];
                        Buffer.BlockCopy(templateData, 0, templateIntArray, 0, Math.Min(templateData.Length, 1420));
                        object templateObj = new VariantWrapper(templateIntArray);

                        // Execute enrollment
                        bool result = ocx.SetEnrollData(
                            machineNo,
                            enrollmentNumber,
                            0, // Current device
                            fingerNumber,
                            privilege,
                            ref templateObj,
                            0); // No password

                        return result;
                    }
                    finally
                    {
                        // Cleanup
                        EnableDevice(ocx, machineNo);
                        ocx.CloseCommPort();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Fingerprint enrollment failed - Machine: {machineNo}, " +
                                 $"User: {enrollmentNumber}, Error: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Enables or disables a user on the biometric device
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <param name="enrollmentNumber">The user's enrollment number</param>
        /// <param name="enrollmentMachineId">The machine ID for enrollment</param>
        /// <param name="backupNumber">Backup number (fingerprint/RFID slot)</param>
        /// <param name="enable">True to enable user, false to disable</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static async Task<bool> SetUserEnabledStatus(string ip, int port, int machineNo, string password,
            int enrollmentNumber, int enrollmentMachineId, int backupNumber, bool enable)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                try
                {
                    // Establish connection
                    if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                        throw new Exception("Failed to set device IP address");

                    if (!ocx.OpenCommPort(machineNo))
                        throw new Exception("Failed to open communication port");

                    try
                    {
                        // Disable device during operation
                        if (!DisableDevice(ocx, machineNo))
                            throw new Exception("Could not disable device");

                        // Execute enable/disable operation
                        bool result = ocx.EnableUser(
                            machineNo,
                            enrollmentNumber,
                            enrollmentMachineId,
                            backupNumber,
                            enable ? 1 : 0); // 1 to enable, 0 to disable

                        return result;
                    }
                    finally
                    {
                        // Cleanup
                        EnableDevice(ocx, machineNo);
                        ocx.CloseCommPort();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"User enable/disable failed - Machine: {machineNo}, " +
                                $"User: {enrollmentNumber}, Enable: {enable}, Error: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Enables a user on the biometric device (convenience method)
        /// </summary>
        public static async Task<bool> EnableUser(string ip, int port, int machineNo, string password,
            int enrollmentNumber, int enrollmentMachineId, int backupNumber)
        {
            return await SetUserEnabledStatus(ip, port, machineNo, password,
                enrollmentNumber, enrollmentMachineId, backupNumber, true);
        }

        /// <summary>
        /// Disables a user on the biometric device (convenience method)
        /// </summary>
        public static async Task<bool> DisableUser(string ip, int port, int machineNo, string password,
            int enrollmentNumber, int enrollmentMachineId, int backupNumber)
        {
            return await SetUserEnabledStatus(ip, port, machineNo, password,
                enrollmentNumber, enrollmentMachineId, backupNumber, false);
        }


        /// <summary>
        /// Deletes enrollment data for a specific user on the biometric device.
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <param name="enrollmentNumber">The user's enrollment number</param>
        /// <param name="enrollmentMachineId">The machine ID for enrollment</param>
        /// <param name="backupNumber">Backup number (e.g., 15 for all data types)</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static async Task<bool> DeleteEnrollmentData(
            string ip,
            int port,
            int machineNo,
            string password,
            int enrollmentNumber,
            int enrollmentMachineId,
            int backupNumber)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                try
                {
                    // Establish connection
                    if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                        throw new Exception("Failed to set device IP address");

                    if (!ocx.OpenCommPort(machineNo))
                        throw new Exception("Failed to open communication port");

                    try
                    {
                        // Disable device during operation
                        if (!DisableDevice(ocx, machineNo))
                            throw new Exception("Could not disable device");

                        // Execute delete operation
                        bool result = ocx.DeleteEnrollData(
                            machineNo,
                            enrollmentNumber,
                            enrollmentMachineId,
                            backupNumber);

                        if (!result)
                            throw new Exception("DeleteEnrollData failed");

                        return true;
                    }
                    finally
                    {
                        // Cleanup
                        EnableDevice(ocx, machineNo);
                        ocx.CloseCommPort();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Delete enrollment data failed - Machine: {machineNo}, " +
                                  $"User: {enrollmentNumber}, EnrollmentMachineId: {enrollmentMachineId}, Backup: {backupNumber}, Error: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Deletes a user control for a specific enrollment number on the biometric device.
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <param name="enrollmentNumber">The user's enrollment number</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static async Task<bool> DeleteUserControl(string ip, int port, int machineNo, string password, int enrollmentNumber)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                try
                {
                    // Establish connection
                    if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                        throw new Exception("Failed to set device IP address");

                    if (!ocx.OpenCommPort(machineNo))
                        throw new Exception("Failed to open communication port");

                    try
                    {
                        // Disable device during operation
                        if (!DisableDevice(ocx, machineNo))
                            throw new Exception("Could not disable device");

                        // Execute delete operation
                        bool result = ocx.DeleteUserCtrl(machineNo, enrollmentNumber);

                        if (!result)
                            throw new Exception("DeleteUserCtrl failed");

                        return true;
                    }
                    finally
                    {
                        // Cleanup
                        EnableDevice(ocx, machineNo);
                        ocx.CloseCommPort();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Delete user control failed - Machine: {machineNo}, " +
                                  $"User: {enrollmentNumber}, Error: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Empties all enrollment data on the biometric device.
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static async Task<bool> EmptyEnrollmentData(string ip, int port, int machineNo, string password)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                try
                {
                    // Establish connection
                    if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                        throw new Exception("Failed to set device IP address");

                    if (!ocx.OpenCommPort(machineNo))
                        throw new Exception("Failed to open communication port");

                    try
                    {
                        // Disable device during operation
                        if (!DisableDevice(ocx, machineNo))
                            throw new Exception("Could not disable device");

                        // Execute empty enrollment data operation
                        bool result = ocx.EmptyEnrollData(machineNo);

                        if (!result)
                            throw new Exception("EmptyEnrollData failed");

                        return true;
                    }
                    finally
                    {
                        // Cleanup
                        EnableDevice(ocx, machineNo);
                        ocx.CloseCommPort();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Empty enrollment data failed - Machine: {machineNo}, Error: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Reads all general log data from a biometric device and saves specific fields (EnrollNo, EMachineNo, DateTime)
        /// to a TXT file named "GLog.{EMachineNo}.txt", ignoring records with EnrollNo "00000000000".
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <returns>Number of records read and saved to file</returns>
        public static async Task<int> ReadAndSaveGLogData(string ip, int port, int machineNo, string password)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                try
                {
                    LogHelper.Log($"Reading attendance logs from device at {ip}:{port}");

                    // Establish connection
                    if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                        throw new Exception("Failed to set device IP address");

                    if (!ocx.OpenCommPort(machineNo))
                        throw new Exception("Failed to open communication port");

                    try
                    {
                        // Disable device during operation
                        if (!DisableDevice(ocx, machineNo))
                            throw new Exception("Could not disable device for reading logs");

                        // Read all general log data
                        bool result = ocx.ReadAllGLogData(machineNo);
                        if (!result)
                            throw new Exception("ReadAllGLogData failed");

                        // Create a list to store records
                        List<string> records = new List<string>();

                        // Add header
                        records.Add("EnrollNo,EMachineNo,DateTime");

                        int recordCount = 0;
                        bool hasMore;
                        int eMachineNo = 0;

                        do
                        {
                            // Variables to hold data
                            int dwTMachineNumber = 0;
                            int dwEnrollNumber = 0;
                            int dwEMachineNumber = 0;
                            int dwVerifyMode = 0;
                            int dwInout = 0;
                            int dwEvent = 0;
                            int dwYear = 0;
                            int dwMonth = 0;
                            int dwDay = 0;
                            int dwHour = 0;
                            int dwMinute = 0;
                            int dwSecond = 0;

                            // Get log data with second
                            hasMore = ocx.GetAllGLogDataWithSecond(machineNo,
                                ref dwTMachineNumber,
                                ref dwEnrollNumber,
                                ref dwEMachineNumber,
                                ref dwVerifyMode,
                                ref dwInout,
                                ref dwEvent,
                                ref dwYear,
                                ref dwMonth,
                                ref dwDay,
                                ref dwHour,
                                ref dwMinute,
                                ref dwSecond);

                            if (hasMore)
                            {
                                // Save EMachineNo for filename
                                if (recordCount == 0)
                                {
                                    eMachineNo = dwEMachineNumber;
                                }

                                // Skip records with EnrollNo "00000000000" (0)
                                if (dwEnrollNumber != 0)
                                {
                                    // Create DateTime from components
                                    DateTime dt = new DateTime(
                                        dwYear, dwMonth, dwDay,
                                        dwHour, dwMinute, dwSecond);

                                    // Format line with only the required fields
                                    string line = $"{dwEnrollNumber:D8},{dwEMachineNumber},{dt:yyyy/MM/dd HH:mm:ss}";
                                    records.Add(line);
                                    recordCount++;
                                }
                            }
                        } while (hasMore);

                        // Create the file with the pattern "GLog.{EMachineNo}.txt"
                        string fileName = $"GLog.{eMachineNo}.txt";
                        string folderPath = AppDomain.CurrentDomain.BaseDirectory;
                        string filePath = Path.Combine(folderPath, fileName);

                        // Write all records to TXT file
                        File.WriteAllLines(filePath, records);

                        LogHelper.Log($"Successfully saved {recordCount} attendance records to TXT file {filePath}, excluding EnrollNo 00000000000");

                        return recordCount;
                    }
                    finally
                    {
                        // Re-enable device
                        EnableDevice(ocx, machineNo);
                        ocx.CloseCommPort();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Failed to read attendance log data: {ex.Message}");
                    throw;
                }
            });
        }


        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <returns>Number of records read and saved to file</returns>
        public static async Task<int> ReadAndSaveStaffGLogData(string ip, int port, int machineNo, string password)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                try
                {
                    LogHelper.Log($"Reading attendance logs from device at {ip}:{port}");

                    // Establish connection
                    if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                        throw new Exception("Failed to set device IP address");

                    if (!ocx.OpenCommPort(machineNo))
                        throw new Exception("Failed to open communication port");

                    try
                    {
                        // Disable device during operation
                        if (!DisableDevice(ocx, machineNo))
                            throw new Exception("Could not disable device for reading logs");

                        // Read all general log data
                        bool result = ocx.ReadAllGLogData(machineNo);
                        if (!result)
                            throw new Exception("ReadAllGLogData failed");

                        // Create a list to store records
                        List<string> records = new List<string>();

                        // Add header
                        records.Add("EnrollNo,EMachineNo,DateTime");

                        int recordCount = 0;
                        bool hasMore;
                        int eMachineNo = 0;

                        do
                        {
                            // Variables to hold data
                            int dwTMachineNumber = 0;
                            int dwEnrollNumber = 0;
                            int dwEMachineNumber = 0;
                            int dwVerifyMode = 0;
                            int dwInout = 0;
                            int dwEvent = 0;
                            int dwYear = 0;
                            int dwMonth = 0;
                            int dwDay = 0;
                            int dwHour = 0;
                            int dwMinute = 0;
                            int dwSecond = 0;

                            // Get log data with second
                            hasMore = ocx.GetAllGLogDataWithSecond(machineNo,
                                ref dwTMachineNumber,
                                ref dwEnrollNumber,
                                ref dwEMachineNumber,
                                ref dwVerifyMode,
                                ref dwInout,
                                ref dwEvent,
                                ref dwYear,
                                ref dwMonth,
                                ref dwDay,
                                ref dwHour,
                                ref dwMinute,
                                ref dwSecond);

                            if (hasMore)
                            {
                                // Save EMachineNo for filename
                                if (recordCount == 0)
                                {
                                    eMachineNo = dwEMachineNumber;
                                }

                                // Skip records with EnrollNo "00000000000" (0)
                                if (dwEnrollNumber != 0)
                                {
                                    // Create DateTime from components
                                    DateTime dt = new DateTime(
                                        dwYear, dwMonth, dwDay,
                                        dwHour, dwMinute, dwSecond);

                                    // Format line with only the required fields
                                    string line = $"{dwEnrollNumber:D8},{dwEMachineNumber},{dt:yyyy/MM/dd HH:mm:ss}";
                                    records.Add(line);
                                    recordCount++;
                                }
                            }
                        } while (hasMore);

                        // Create the file with the pattern "StaffGLogs.{EMachineNo}.txt"
                        string fileName = $"StaffGLogs.{eMachineNo}.txt";
                        string folderPath = AppDomain.CurrentDomain.BaseDirectory;
                        string filePath = Path.Combine(folderPath, fileName);

                        // Write all records to TXT file
                        File.WriteAllLines(filePath, records);

                        LogHelper.Log($"Successfully saved {recordCount} attendance records to TXT file {filePath}, excluding EnrollNo 00000000000");

                        return recordCount;
                    }
                    finally
                    {
                        // Re-enable device
                        EnableDevice(ocx, machineNo);
                        ocx.CloseCommPort();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Failed to read attendance log data: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Sets the current system time on the biometric device to synchronize it with the host machine.
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static async Task<bool> SetDeviceTime(string ip, int port, int machineNo, string password)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                try
                {
                    // Establish connection
                    if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                        throw new Exception("Failed to set device IP address");

                    if (!ocx.OpenCommPort(machineNo))
                        throw new Exception("Failed to open communication port");

                    try
                    {
                        // Disable device during operation
                        if (!DisableDevice(ocx, machineNo))
                            throw new Exception("Could not disable device for time synchronization");

                        // Set device time to current system time
                        bool result = ocx.SetDeviceTime(machineNo);

                        if (!result)
                            throw new Exception("SetDeviceTime operation failed");

                        LogHelper.Log($"Device time successfully synchronized - Machine: {machineNo}");
                        return true;
                    }
                    finally
                    {
                        // Cleanup - always re-enable device
                        EnableDevice(ocx, machineNo);
                        ocx.CloseCommPort();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Set device time failed - Machine: {machineNo}, Error: {ex.Message}");
                    throw;
                }
            });
        }

        /// <summary>
        /// Empties all general log data (attendance logs) on the biometric device.
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <returns>True if operation succeeded, false otherwise</returns>
        public static async Task<bool> EmptyGeneralLogData(string ip, int port, int machineNo, string password)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                try
                {
                    // Establish connection
                    if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                        throw new Exception("Failed to set device IP address");

                    if (!ocx.OpenCommPort(machineNo))
                        throw new Exception("Failed to open communication port");

                    try
                    {
                        // Disable device during operation
                        if (!DisableDevice(ocx, machineNo))
                            throw new Exception("Could not disable device");

                        // Execute empty general log data operation
                        bool result = ocx.EmptyGeneralLogData(machineNo);

                        if (!result)
                            throw new Exception("EmptyGeneralLogData failed");

                        return true;
                    }
                    finally
                    {
                        // Cleanup
                        EnableDevice(ocx, machineNo);
                        ocx.CloseCommPort();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Empty general log data failed - Machine: {machineNo}, Error: {ex.Message}");
                    throw;
                }
            });
        }

        enum DoorStatus
        {
            FORCEOPEN = 1,
            FORCECLOSE,
            SOFTWAREOPEN,
            RESTORETOAUTO,
            REBOOT_FPA_MACHINE, //Finger printer acquisition
            DEASSERT_ALARM

        };

        public static async Task<bool> OpenDoor(string ip, int port, int machineNo, string password)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                    throw new Exception("Failed to set device IP address");

                if (!ocx.OpenCommPort(machineNo))
                    throw new Exception("Failed to open communication port");

                try
                {
                    // Disable device before door operation
                    if (!ocx.EnableDevice(machineNo, 0))
                        throw new Exception("Could not disable device for door open");

                    // Attempt to open the door using the SOFTWAREOPEN command
                    bool result = ocx.SetDoorStatus(machineNo, (int)DoorStatus.SOFTWAREOPEN);
                    return result;
                }
                finally
                {
                    // Ensure the device is re-enabled and the communication port is closed
                    ocx.EnableDevice(machineNo, 1);
                    ocx.CloseCommPort();
                }
            });
        }

        /// <summary>
        /// Retrieves the user name for a specific enrollment number from the biometric device.
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <param name="enrollmentNumber">The enrollment number to query</param>
        /// <param name="enrollmentMachineId">The machine ID for enrollment (defaults to 0)</param>
        /// <returns>The user name string if successful, empty string otherwise</returns>
        public static async Task<string> GetUserName(string ip, int port, int machineNo, string password,
     int enrollmentNumber, int enrollmentMachineId = 0)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                    throw new Exception("SetIPAddress failed.");

                if (!ocx.OpenCommPort(machineNo))
                    throw new Exception("OpenCommPort failed.");

                try
                {
                    // Disable device before reading user name
                    if (!DisableDevice(ocx, machineNo))
                        throw new Exception("Could not disable device for reading user name.");

                    string strName = "";
                    object obj = new VariantWrapper(strName);

                    bool result = ocx.GetUserName(0, machineNo, enrollmentNumber, enrollmentMachineId, ref obj);

                    return result ? (string)obj : "";
                }
                finally
                {
                    // Always re-enable device and close port
                    EnableDevice(ocx, machineNo);
                    ocx.CloseCommPort();
                }
            });
        }
        /// <summary>
        /// Represents user information retrieved from a biometric device.
        /// </summary>
        public class UserInfo
        {
            public string AliasId { get; set; }
            public string UserName { get; set; }
            public int Password { get; set; }
            public string CardNumber { get; set; }
            public int FaceFlag { get; set; }
            public int FPFlag { get; set; }
            public int PalmFlag { get; set; }
            public int PostID { get; set; }
            public int Privilege { get; set; }
            public int Enabled { get; set; }
            public int ShiftID { get; set; }
            public int ZoneID { get; set; }
            public int GroupID { get; set; }
            public int UserCtrl { get; set; }
            public int StartTime { get; set; }
            public int EndTime { get; set; }
            public int BirthDay { get; set; }
        }

        /// <summary>
        /// Retrieves comprehensive user information for a specific alias ID from the biometric device.
        /// </summary>
        /// <param name="ip">Device IP address</param>
        /// <param name="port">Device port</param>
        /// <param name="machineNo">Device machine number</param>
        /// <param name="password">Device password</param>
        /// <param name="aliasId">The alias ID to query</param>
        /// <returns>UserInfo object containing all retrieved user data</returns>
        public static async Task<UserInfo> GetUserInfo(string ip, int port, int machineNo, string password, string aliasId)
        {
            return await OcxThreadInvoker.Run(ip, ocx =>
            {
                try
                {
                    // Establish connection
                    if (!ocx.SetIPAddress(ref ip, port, int.Parse(password)))
                        throw new Exception("Failed to set device IP address");

                    if (!ocx.OpenCommPort(machineNo))
                        throw new Exception("Failed to open communication port");

                    try
                    {
                        // Disable device during operation
                        if (!DisableDevice(ocx, machineNo))
                            throw new Exception("Could not disable device");

                        // Prepare parameters for GetUserInfoLongID
                        object aliasid = new VariantWrapper(aliasId);
                        string userNameStr = "";
                        object lpszUserName = new VariantWrapper(userNameStr);
                        int pwd = 0;
                        long cardInit = 0;
                        object card = new VariantWrapper(cardInit);
                        int faceFlag = 0;
                        int fPFlag = 0;
                        int palmFlag = 0;
                        int postID = 0;
                        int privilege = 0;
                        int enabled = 0;
                        int shiftID = 0;
                        int zoneID = 0;
                        int groupID = 0;
                        int userCtrl = 0;
                        int startTime = 0;
                        int endTime = 0;
                        int birthDay = 0;

                        // Execute GetUserInfoLongID
                        bool bRet = ocx.GetUserInfoLongID(
                            machineNo,
                            ref aliasid,
                            ref lpszUserName,
                            ref pwd,
                            ref card,
                            ref faceFlag,
                            ref fPFlag,
                            ref palmFlag,
                            ref postID,
                            ref privilege,
                            ref enabled,
                            ref shiftID,
                            ref zoneID,
                            ref groupID,
                            ref userCtrl,
                            ref startTime,
                            ref endTime,
                            ref birthDay
                        );

                        if (!bRet)
                            throw new Exception("GetUserInfoLongID failed");

                        // Extract card number and normalize if negative int (unsigned 32-bit handling)
                        string cardText;
                        if (card is long cardLong)
                        {
                            cardText = cardLong.ToString();
                        }
                        else if (card is int cardInt)
                        {
                            long unsignedCard = cardInt < 0 ? cardInt + 4294967296L : cardInt;
                            cardText = unsignedCard.ToString();
                        }
                        else if (card is string cardStr)
                        {
                            cardText = cardStr;
                        }
                        else
                        {
                            cardText = "";
                        }

                        return new UserInfo
                        {
                            AliasId = aliasId,
                            UserName = (string)lpszUserName,
                            Password = pwd,
                            CardNumber = cardText,
                            FaceFlag = faceFlag,
                            FPFlag = fPFlag,
                            PalmFlag = palmFlag,
                            PostID = postID,
                            Privilege = privilege,
                            Enabled = enabled,
                            ShiftID = shiftID,
                            ZoneID = zoneID,
                            GroupID = groupID,
                            UserCtrl = userCtrl,
                            StartTime = startTime,
                            EndTime = endTime,
                            BirthDay = birthDay
                        };
                    }
                    finally
                    {
                        // Cleanup
                        EnableDevice(ocx, machineNo);
                        ocx.CloseCommPort();
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Get user info failed - Machine: {machineNo}, Alias: {aliasId}, Error: {ex.Message}");
                    throw;
                }
            });
        }

    }
}
