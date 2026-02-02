using AxFP_CLOCKLib;
using System;

namespace smart_attendance_middleware
{
    /// <summary>
    /// A lightweight class to manage connection/disconnection to a device.
    /// </summary>
    public class DeviceConnector
    {
        private readonly AxFP_CLOCK _ocx;

        public DeviceConnector(AxFP_CLOCK ocx)
        {
            _ocx = ocx ?? throw new ArgumentNullException(nameof(ocx));
        }

        /// <summary>
        /// Connects to a device using the specified parameters.
        /// </summary>
        /// <param name="ip">IP address of the device.</param>
        /// <param name="port">Port number (common for all devices).</param>
        /// <param name="machineNo">Machine number (unique identifier for the device).</param>
        /// <param name="password">Password used by the device.</param>
        /// <returns>True if the connection is successful; false otherwise.</returns>
        public bool Connect(string ip, int port, int machineNo, int password)
        {
            // Set up connection parameters.
            bool ipSet = _ocx.SetIPAddress(ref ip, port, password);
            if (!ipSet)
            {
                return false;
            }

            return _ocx.OpenCommPort(machineNo);
        }

        /// <summary>
        /// Disconnects from the currently connected device.
        /// </summary>
        public void Disconnect()
        {
            _ocx.CloseCommPort();
        }
    }
}

