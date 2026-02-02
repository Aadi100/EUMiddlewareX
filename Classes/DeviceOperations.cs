using AxFP_CLOCKLib;
using System;

namespace smart_attendance_middleware.OCX
{
    /// <summary>
    /// Contains raw OCX-based operations performed on an AxFP_CLOCK device.
    /// Each method assumes the device is already connected.
    /// </summary>
    public static class DeviceOperations
    {
        /// <summary>
        /// Retrieves the serial number of a connected device using the OCX control.
        /// </summary>
        /// <param name="ocx">An initialized and connected AxFP_CLOCK control instance.</param>
        /// <param name="machineNo">The machine number of the target device.</param>
        /// <returns>The trimmed serial number string from the device.</returns>
        /// <exception cref="Exception">Throws if the OCX call fails.</exception>
        public static string GetSerialNumber(AxFP_CLOCK ocx, int machineNo)
        {
            string serialBuffer = new string(' ', 64);

            // Attempt to retrieve the serial number from the connected device
            bool success = ocx.GetSerialNumber(machineNo, ref serialBuffer);

            if (!success)
                throw new Exception("Failed to retrieve serial number from the device.");

            return serialBuffer.Trim();
        }
    }
}
