using System.Net.NetworkInformation;

namespace smart_attendance_middleware.Helpers
{
    /// <summary>
    /// Provides network-level utilities such as checking device availability via ping.
    /// </summary>
    public static class NetworkHelper
    {
        /// <summary>
        /// Pings a device IP to check if it is reachable on the network.
        /// </summary>
        /// <param name="ipAddress">The IP address of the device to ping.</param>
        /// <param name="timeout">Optional timeout in milliseconds (default is 1000ms).</param>
        /// <returns>True if the device responds to ping, otherwise false.</returns>
        public static bool IsDeviceReachable(string ipAddress, int timeout = 1000)
        {
            try
            {
                using (Ping ping = new Ping())
                {
                    PingReply reply = ping.Send(ipAddress, timeout);
                    return reply.Status == IPStatus.Success;
                }
            }
            catch
            {
                // Catch any exception (like invalid IP format or no network)
                return false;
            }
        }
    }
}
