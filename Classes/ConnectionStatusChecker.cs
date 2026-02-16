using smart_attendance_middleware;
using smart_attendance_middleware.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

public sealed class ConnectionStatusChecker
{
    // Singleton instance — ensures only one instance is ever created
    private static readonly Lazy<ConnectionStatusChecker> instance =
        new Lazy<ConnectionStatusChecker>(() => new ConnectionStatusChecker());

    // Public accessor for the singleton instance
    public static ConnectionStatusChecker Instance => instance.Value;

    public bool IsInternetConnected { get; private set; }
    public bool IsBiometricDeviceConnected { get; private set; }
    public bool IsSubscriptionValid { get; private set; }
    public bool IsStaffDeviceConnected { get; private set; }

    // Events that other parts of the app can subscribe to for real-time updates
    public event Action<bool> InternetStatusChanged;
    public event Action<bool> BiometricDeviceStatusChanged;
    public event Action<bool> SubscriptionStatusChanged;
    public event Action<bool> StaffDeviceStatusChanged;

    private System.Threading.Timer internetCheckTimer;
    private System.Threading.Timer biometricCheckTimer;
    private System.Threading.Timer subscriptionCheckTimer;
    private System.Threading.Timer staffDeviceCheckTimer;

    private int internetAttemptCounter = 0;
    private WebSocketManager _wsManager;
    private DatabaseHelper _dbHelper;

    // Track when we last sent a ping to avoid spamming
    private DateTime lastPingSent = DateTime.MinValue;

    // Track reconnection attempts to avoid spamming
    private DateTime lastReconnectionAttempt = DateTime.MinValue;
    private int reconnectionAttempts = 0;

    // Add fields to track previous device states and retry states
    private Dictionary<int, bool> _previousDeviceStates = new Dictionary<int, bool>();
    private Dictionary<int, bool> _deviceRetryStates = new Dictionary<int, bool>();

    // Add fields to track previous staff device states
    private Dictionary<int, bool> _previousStaffDeviceStates = new Dictionary<int, bool>();
    private Dictionary<int, bool> _staffDeviceRetryStates = new Dictionary<int, bool>();

    private int _biometricCheckInProgress = 0;
    private int _staffCheckInProgress = 0;

    private Thread _checkerThread;
    private bool _isRunning = false;
    private readonly object _lock = new object();

    private ConnectionStatusChecker()
    {
        _dbHelper = new DatabaseHelper();
    }

    // Method to set the WebSocketManager 
    public void SetWebSocketManager(WebSocketManager wsManager)
    {
        _wsManager = wsManager;
        LogHelper.Log("WebSocketManager set in ConnectionStatusChecker");
    }

    // Start all timers and perform initial status checks
    public void Start()
    {
        lock (_lock)
        {
            if (_isRunning) return;

            _isRunning = true;
            _checkerThread = new Thread(ThreadStartRoutine)
            {
                IsBackground = true,
                Name = "ConnectionStatusCheckerThread"
            };
            _checkerThread.SetApartmentState(ApartmentState.STA);
            _checkerThread.Start();
            LogHelper.Log("ConnectionStatusChecker thread started.");
        }
    }

    private void ThreadStartRoutine()
    {
        try
        {
            // Initial checks
            InternetCheck(null);
            BiometricDeviceCheck(null);
            SubscriptionCheck(null);
            StaffDeviceCheck(null);

            // Initialize timers within the new thread
            internetCheckTimer = new System.Threading.Timer(InternetCheck, null, 60000, 60000); // 1 minute
            biometricCheckTimer = new System.Threading.Timer(BiometricDeviceCheck, null, 60000, 60000); // 1 minute
            subscriptionCheckTimer = new System.Threading.Timer(SubscriptionCheck, null, 15000, 15000); // 15 seconds
            staffDeviceCheckTimer = new System.Threading.Timer(StaffDeviceCheck, null, 60000, 60000); // 1 minute

            // Keep the thread alive
            while (_isRunning)
            {
                Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            LogHelper.Log($"[ConnectionStatusChecker] Thread exception: {ex.Message}");
        }
        finally
        {
            DisposeTimers();
            LogHelper.Log("ConnectionStatusChecker thread stopped and timers disposed.");
        }
    }


    // Stop all timers — useful when shutting down the app
    public void Stop()
    {
        lock (_lock)
        {
            if (!_isRunning) return;
            _isRunning = false;
        }

        // Wait for the thread to finish
        _checkerThread?.Join(TimeSpan.FromSeconds(5));
        _checkerThread = null;
    }

    private void DisposeTimers()
    {
        internetCheckTimer?.Dispose();
        biometricCheckTimer?.Dispose();
        subscriptionCheckTimer?.Dispose();
        staffDeviceCheckTimer?.Dispose();
        internetCheckTimer = null;
        biometricCheckTimer = null;
        subscriptionCheckTimer = null;
        staffDeviceCheckTimer = null;
    }


    // Check internet connection and raise event if the status has changed
    private void InternetCheck(object state)
    {
        try
        {
            bool previousStatus = IsInternetConnected;

            // Check the actual internet status
            IsInternetConnected = CheckInternet();

            // Raise the event if there was a change
            if (previousStatus != IsInternetConnected)
                InternetStatusChanged?.Invoke(IsInternetConnected);

            // If internet just came back online, check WebSocket subscription status
            if (!previousStatus && IsInternetConnected)
            {
                LogHelper.Log("Internet connection restored - checking WebSocket subscription status");

                // Use the existing CheckSubscription method to verify and potentially reconnect
                bool subscriptionValid = CheckSubscription();

                // Update subscription status if it changed
                if (IsSubscriptionValid != subscriptionValid)
                {
                    IsSubscriptionValid = subscriptionValid;
                    SubscriptionStatusChanged?.Invoke(IsSubscriptionValid);
                }
            }

            // Adjust check interval based on current connectivity status
            if (IsInternetConnected)
            {
                internetAttemptCounter = 0;
                internetCheckTimer?.Change(60000, 60000); // back to 1 minute
            }
            else
            {
                internetAttemptCounter++;

                if (internetAttemptCounter == 3)
                    internetCheckTimer?.Change(120000, 120000); // after 3 attempts, slow down to 2 minutes
                else if (internetAttemptCounter > 3)
                    internetCheckTimer?.Change(300000, 300000); // then check every 5 minutes
            }
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Internet check error: {ex.Message}");
        }
    }
    // Actual method to ping internet — checks connectivity
    private bool CheckInternet()
    {
        try
        {
            using (var ping = new Ping())
            {
                var reply = ping.Send("google.com", 2000); // Ping Google with 2 sec timeout
                return reply.Status == IPStatus.Success;
            }
        }
        catch
        {
            return false; // If anything fails, consider internet disconnected
        }
    }


    // Check biometric device connection and raise event if status changes
    private void BiometricDeviceCheck(object state)
    {
        try
        {
            bool previousStatus = IsBiometricDeviceConnected;

            // Check all available devices and determine if any are connected
            IsBiometricDeviceConnected = CheckBiometricDevice();

            if (previousStatus != IsBiometricDeviceConnected)
                BiometricDeviceStatusChanged?.Invoke(IsBiometricDeviceConnected);
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Biometric device check error: {ex.Message}");
        }
    }

    // Check staff device connection and raise event if status changes
    private void StaffDeviceCheck(object state)
    {
        try
        {
            bool previousStatus = IsStaffDeviceConnected;

            // Check all available staff devices and determine if any are connected
            IsStaffDeviceConnected = CheckStaffDevice();

            if (previousStatus != IsStaffDeviceConnected)
                StaffDeviceStatusChanged?.Invoke(IsStaffDeviceConnected);
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Staff device check error: {ex.Message}");
        }
    }

    // Check biometric device connectivity by pinging all available devices
    // Replace CheckBiometricDevice() with this version
    private bool CheckBiometricDevice()
    {
        try
        {
            // Load devices from database
            List<Device> devices = null;

            if (AppSession.DeviceList?.Count > 0)
            {
                devices = AppSession.DeviceList;
                LogHelper.Log($"Device check: Using {devices.Count} devices from AppSession");
            }
            else
            {
                devices = _dbHelper.LoadDevices();
                LogHelper.Log($"Device check: Loaded {devices?.Count ?? 0} devices from database");
            }

            if (devices == null || devices.Count == 0)
            {
                LogHelper.Log("Device check: No devices configured");
                return false;
            }

            // Don't start another reachability sweep if one is already running
            if (Interlocked.Exchange(ref _biometricCheckInProgress, 1) == 1)
            {
                LogHelper.Log("Device check: Previous biometric sweep still running - skipping this trigger");
                return IsBiometricDeviceConnected; // keep current stable UI state
            }

            // Start async sweep; it will raise events when retries are complete
            Task.Run(async () =>
            {
                try
                {
                    await CheckAllDevicesAsync(devices).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Device check: Background sweep failed - {ex.Message}");
                }
                finally
                {
                    Interlocked.Exchange(ref _biometricCheckInProgress, 0);
                }
            });

            // IMPORTANT: do not change status here; keep current value until sweep completes
            return IsBiometricDeviceConnected;
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Device check error: {ex.Message}");
            return false;
        }
    }

    // Check staff device connectivity by pinging all available staff devices
    private bool CheckStaffDevice()
    {
        try
        {
            List<Device> staffDevices = null;

            if (AppSession.StaffDeviceList?.Count > 0)
            {
                staffDevices = AppSession.StaffDeviceList;
                LogHelper.Log($"Staff device check: Using {staffDevices.Count} staff devices from AppSession");
            }
            else
            {
                staffDevices = _dbHelper.LoadStaffDevices();
                LogHelper.Log($"Staff device check: Loaded {staffDevices?.Count ?? 0} staff devices from database");
            }

            if (staffDevices == null || staffDevices.Count == 0)
            {
                LogHelper.Log("Staff device check: No staff devices configured");
                return false;
            }

            if (Interlocked.Exchange(ref _staffCheckInProgress, 1) == 1)
            {
                LogHelper.Log("Staff device check: Previous staff sweep still running - skipping this trigger");
                return IsStaffDeviceConnected;
            }

            Task.Run(async () =>
            {
                try
                {
                    await CheckAllStaffDevicesAsync(staffDevices).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"Staff device check: Background sweep failed - {ex.Message}");
                }
                finally
                {
                    Interlocked.Exchange(ref _staffCheckInProgress, 0);
                }
            });

            return IsStaffDeviceConnected;
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Staff device check error: {ex.Message}");
            return false;
        }
    }

    // Modified CheckAllDevicesAsync method
    // Asynchronously check all devices
    private async Task CheckAllDevicesAsync(List<Device> devices)
    {
        const int pingTimeout = 2000; // 2 seconds
        const int maxRetries = 3; // Number of retry attempts
        const int retryDelay = 5000; // 5 seconds between retries

        int connectedDevices = 0;
        int totalDevices = devices.Count;
        bool anyStatusChanged = false;

        foreach (var device in devices)
        {
            try
            {
                // First ping attempt
                bool isReachable = await PingDeviceWithRetriesAsync(device.DeviceIP, pingTimeout, 1, 0);

                // Get previous status
                bool previousStatus = _previousDeviceStates.ContainsKey(device.DeviceId)
                                    ? _previousDeviceStates[device.DeviceId]
                                    : false;

                if (isReachable)
                {
                    // Device is reachable on first try
                    connectedDevices++;

                    // Clear retry state if it was set and reset border color
                    if (_deviceRetryStates.ContainsKey(device.DeviceId))
                    {
                        _deviceRetryStates.Remove(device.DeviceId);
                        await UpdateDevicePanelBorderColorOnDashboard(device.DeviceId, false);
                    }

                    // Check if status changed
                    if (previousStatus != isReachable)
                    {
                        anyStatusChanged = true;
                        _previousDeviceStates[device.DeviceId] = isReachable;
                        LogHelper.Log($"Device check: {device.DeviceIP} (ID: {device.DeviceId}) became reachable");
                    }
                    else
                    {
                        _previousDeviceStates[device.DeviceId] = isReachable;
                    }
                }
                else
                {
                    // Device is not reachable on first try - start retry process
                    LogHelper.Log($"Device check: {device.DeviceIP} (ID: {device.DeviceId}) unreachable - starting retry process");

                    // Set retry state and change border to orange (but don't refresh dashboard yet)
                    _deviceRetryStates[device.DeviceId] = true;
                    await UpdateDevicePanelBorderColorOnDashboard(device.DeviceId, true);

                    bool retrySuccessful = false;

                    // Retry attempts with 5-second delays
                    for (int retryAttempt = 1; retryAttempt <= maxRetries; retryAttempt++)
                    {
                        LogHelper.Log($"Device check: Retrying {device.DeviceIP} (ID: {device.DeviceId}) - Attempt {retryAttempt}/{maxRetries}");

                        // Wait 5 seconds before retry
                        await Task.Delay(retryDelay);

                        // Retry ping
                        bool retryResult = await PingDeviceWithRetriesAsync(device.DeviceIP, pingTimeout, 1, 0);

                        if (retryResult)
                        {
                            // Device became reachable during retry
                            retrySuccessful = true;
                            connectedDevices++;

                            // Clear retry state and reset border color
                            _deviceRetryStates.Remove(device.DeviceId);
                            await UpdateDevicePanelBorderColorOnDashboard(device.DeviceId, false);

                            LogHelper.Log($"Device check: {device.DeviceIP} (ID: {device.DeviceId}) became reachable on retry attempt {retryAttempt}");

                            // Check if status changed
                            if (previousStatus != retryResult)
                            {
                                anyStatusChanged = true;
                                _previousDeviceStates[device.DeviceId] = retryResult;
                            }
                            else
                            {
                                _previousDeviceStates[device.DeviceId] = retryResult;
                            }
                            break;
                        }
                    }

                    if (!retrySuccessful)
                    {
                        // All retries failed - device is still unreachable
                        LogHelper.Log($"Device check: {device.DeviceIP} (ID: {device.DeviceId}) unreachable after all {maxRetries} retries");

                        // **IMPORTANT**: Reset border color to normal BEFORE marking as failed and refreshing
                        _deviceRetryStates.Remove(device.DeviceId);
                        await UpdateDevicePanelBorderColorOnDashboard(device.DeviceId, false);
                        LogHelper.Log($"Device check: Reset border color to normal for device {device.DeviceId} before marking as failed");

                        // Check if status changed (from reachable to unreachable)
                        if (previousStatus != false)
                        {
                            anyStatusChanged = true;
                            _previousDeviceStates[device.DeviceId] = false;
                            LogHelper.Log($"Device check: {device.DeviceIP} (ID: {device.DeviceId}) marked as unreachable after retries");
                        }
                        else
                        {
                            _previousDeviceStates[device.DeviceId] = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Device check: Error pinging {device.DeviceIP} - {ex.Message}");

                // Handle error case - consider device unreachable
                bool previousStatus = _previousDeviceStates.ContainsKey(device.DeviceId)
                                    ? _previousDeviceStates[device.DeviceId]
                                    : false;

                // Set retry state and orange border for error case initially
                _deviceRetryStates[device.DeviceId] = true;
                await UpdateDevicePanelBorderColorOnDashboard(device.DeviceId, true);

                // Wait a moment then reset border color before marking as failed
                await Task.Delay(2000); // 2 second delay to show the orange border briefly
                _deviceRetryStates.Remove(device.DeviceId);
                await UpdateDevicePanelBorderColorOnDashboard(device.DeviceId, false);
                LogHelper.Log($"Device check: Reset border color to normal for device {device.DeviceId} after error");

                if (previousStatus != false)
                {
                    anyStatusChanged = true;
                    _previousDeviceStates[device.DeviceId] = false;
                    LogHelper.Log($"Device check: {device.DeviceIP} (ID: {device.DeviceId}) error - marking as unreachable");
                }
            }
        }

        // Update the biometric device status based on connectivity
        bool anyDeviceConnected = connectedDevices > 0;

        LogHelper.Log($"Device check: {connectedDevices}/{totalDevices} devices reachable");

        // Update status if changed
        if (IsBiometricDeviceConnected != anyDeviceConnected)
        {
            IsBiometricDeviceConnected = anyDeviceConnected;
            BiometricDeviceStatusChanged?.Invoke(IsBiometricDeviceConnected);
        }

        // Only trigger refresh if any device status actually changed after all retries are complete
        if (anyStatusChanged)
        {
            LogHelper.Log("Device status changes detected after retries - triggering Dashboard refresh");
            await TriggerDashboardRefreshAfterRetries();
        }
    }

    // Asynchronously check all staff devices (simplified version without UI interactions)
    private async Task CheckAllStaffDevicesAsync(List<Device> staffDevices)
    {
        const int pingTimeout = 2000; // 2 seconds
        const int maxRetries = 3; // Number of retry attempts
        const int retryDelay = 5000; // 5 seconds between retries

        int connectedStaffDevices = 0;
        int totalStaffDevices = staffDevices.Count;
        bool anyStatusChanged = false;

        foreach (var device in staffDevices)
        {
            try
            {
                // First ping attempt
                bool isReachable = await PingDeviceWithRetriesAsync(device.DeviceIP, pingTimeout, 1, 0);

                // Get previous status (using StaffDeviceId for staff devices)
                bool previousStatus = _previousStaffDeviceStates.ContainsKey(device.StaffDeviceId)
                                    ? _previousStaffDeviceStates[device.StaffDeviceId]
                                    : false;

                if (isReachable)
                {
                    // Staff device is reachable on first try
                    connectedStaffDevices++;

                    // Check if status changed
                    if (previousStatus != isReachable)
                    {
                        anyStatusChanged = true;
                        _previousStaffDeviceStates[device.StaffDeviceId] = isReachable;
                        LogHelper.Log($"Staff device check: {device.DeviceIP} (ID: {device.StaffDeviceId}) became reachable");
                    }
                    else
                    {
                        _previousStaffDeviceStates[device.StaffDeviceId] = isReachable;
                    }
                }
                else
                {
                    // Staff device is not reachable on first try - start retry process
                    LogHelper.Log($"Staff device check: {device.DeviceIP} (ID: {device.StaffDeviceId}) unreachable - starting retry process");

                    bool retrySuccessful = false;

                    // Retry attempts with 5-second delays
                    for (int retryAttempt = 1; retryAttempt <= maxRetries; retryAttempt++)
                    {
                        LogHelper.Log($"Staff device check: Retrying {device.DeviceIP} (ID: {device.StaffDeviceId}) - Attempt {retryAttempt}/{maxRetries}");

                        // Wait 5 seconds before retry
                        await Task.Delay(retryDelay);

                        // Retry ping
                        bool retryResult = await PingDeviceWithRetriesAsync(device.DeviceIP, pingTimeout, 1, 0);

                        if (retryResult)
                        {
                            // Staff device became reachable during retry
                            retrySuccessful = true;
                            connectedStaffDevices++;

                            LogHelper.Log($"Staff device check: {device.DeviceIP} (ID: {device.StaffDeviceId}) became reachable on retry attempt {retryAttempt}");

                            // Check if status changed
                            if (previousStatus != retryResult)
                            {
                                anyStatusChanged = true;
                                _previousStaffDeviceStates[device.StaffDeviceId] = retryResult;
                            }
                            else
                            {
                                _previousStaffDeviceStates[device.StaffDeviceId] = retryResult;
                            }
                            break;
                        }
                    }

                    if (!retrySuccessful)
                    {
                        // All retries failed - staff device is still unreachable
                        LogHelper.Log($"Staff device check: {device.DeviceIP} (ID: {device.StaffDeviceId}) unreachable after all {maxRetries} retries");

                        // Check if status changed (from reachable to unreachable)
                        if (previousStatus != false)
                        {
                            anyStatusChanged = true;
                            _previousStaffDeviceStates[device.StaffDeviceId] = false;
                            LogHelper.Log($"Staff device check: {device.DeviceIP} (ID: {device.StaffDeviceId}) marked as unreachable after retries");
                        }
                        else
                        {
                            _previousStaffDeviceStates[device.StaffDeviceId] = false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"Staff device check: Error pinging {device.DeviceIP} - {ex.Message}");

                // Handle error case - consider staff device unreachable
                bool previousStatus = _previousStaffDeviceStates.ContainsKey(device.StaffDeviceId)
                                    ? _previousStaffDeviceStates[device.StaffDeviceId]
                                    : false;

                if (previousStatus != false)
                {
                    anyStatusChanged = true;
                    _previousStaffDeviceStates[device.StaffDeviceId] = false;
                    LogHelper.Log($"Staff device check: {device.DeviceIP} (ID: {device.StaffDeviceId}) error - marking as unreachable");
                }
            }
        }

        // Update the staff device status based on connectivity
        bool anyStaffDeviceConnected = connectedStaffDevices > 0;

        LogHelper.Log($"Staff device check: {connectedStaffDevices}/{totalStaffDevices} staff devices reachable");

        // Update status if changed
        if (IsStaffDeviceConnected != anyStaffDeviceConnected)
        {
            IsStaffDeviceConnected = anyStaffDeviceConnected;
            StaffDeviceStatusChanged?.Invoke(IsStaffDeviceConnected);
        }
    }

    // Helper method to update panel border color on Dashboard
    private async Task UpdateDevicePanelBorderColorOnDashboard(int deviceId, bool isRetrying)
    {
        try
        {
            // Find the Dashboard form
            var dashboardForm = Application.OpenForms.Cast<Form>()
                .FirstOrDefault(f => f is Dashboard) as Dashboard;

            if (dashboardForm != null)
            {
                await dashboardForm.UpdateDevicePanelBorderColor(deviceId, isRetrying);
            }
            else
            {
                LogHelper.Log("Dashboard form not found - could not update panel border color");
            }
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error updating device panel border color: {ex.Message}");
        }
    }

    // Modified refresh trigger method for after retries
    private async Task TriggerDashboardRefreshAfterRetries()
    {
        try
        {
            // Find the Dashboard form
            var dashboardForm = Application.OpenForms.Cast<Form>()
                .FirstOrDefault(f => f is Dashboard) as Dashboard;

            if (dashboardForm != null)
            {
                if (dashboardForm.InvokeRequired)
                {
                    dashboardForm.Invoke(new Action(async () => await dashboardForm.OnDeviceStatusChangedAfterRetries()));
                }
                else
                {
                    await dashboardForm.OnDeviceStatusChangedAfterRetries();
                }
                LogHelper.Log("Successfully triggered Dashboard refresh after retries");
            }
            else
            {
                LogHelper.Log("Dashboard form not found - could not trigger refresh");
            }
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Error triggering Dashboard refresh after retries: {ex.Message}");
        }
    }
    // Ping a single device with retries (similar to DevicePingService)
    private async Task<bool> PingDeviceWithRetriesAsync(string ipAddress, int timeout, int maxRetries, int retryDelay)
    {
        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using (var ping = new Ping())
                {
                    var reply = await ping.SendPingAsync(ipAddress, timeout);
                    if (reply.Status == IPStatus.Success)
                    {
                        return true;
                    }
                }
            }
            catch (PingException)
            {
                // Ignore ping exceptions and try again
            }

            // Wait before retry (except for last attempt)
            if (attempt < maxRetries)
            {
                await Task.Delay(retryDelay);
            }
        }
        return false;
    }

    // Add these methods to the ConnectionStatusChecker class

    /// <summary>
    /// Temporarily stops the subscription checking timer
    /// </summary>
    public void StopSubscriptionChecking()
    {
        try
        {
            subscriptionCheckTimer?.Change(Timeout.Infinite, Timeout.Infinite);
            LogHelper.Log("🛑 Subscription checking timer stopped");
        }
        catch (Exception ex)
        {
            LogHelper.Log($"❌ Error stopping subscription checking: {ex.Message}");
        }
    }

    /// <summary>
    /// Restarts the subscription checking timer
    /// </summary>
    public void StartSubscriptionChecking()
    {
        try
        {
            subscriptionCheckTimer?.Change(15000, 15000);
            LogHelper.Log("🔄 Subscription checking timer restarted");
        }
        catch (Exception ex)
        {
            LogHelper.Log($"❌ Error starting subscription checking: {ex.Message}");
        }
    }

    // Check subscription status (WebSocketmanager)
    private void SubscriptionCheck(object state)
    {
        try
        {
            bool previousStatus = IsSubscriptionValid;

            // Check subscription status using WebSocket ping/pong
            IsSubscriptionValid = CheckSubscription();

            if (previousStatus != IsSubscriptionValid)
                SubscriptionStatusChanged?.Invoke(IsSubscriptionValid);
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Subscription check error: {ex.Message}");
        }
    }

    // Check subscription validity using WebSocket ping/pong mechanism
    private bool CheckSubscription()
    {
        // Check if WebSocketManager is available
        if (_wsManager == null)
        {
            LogHelper.Log("Subscription check: WebSocketManager not available");
            return false; // No WebSocket manager available
        }

        try
        {
            // Avoid sending pings too frequently (minimum 10 seconds apart)
            var timeSinceLastPing = DateTime.Now - lastPingSent;
            if (timeSinceLastPing.TotalSeconds >= 10)
            {
                // Send ping asynchronously without blocking
                Task.Run(async () =>
                {
                    try
                    {
                        await _wsManager.ConnectionStatusAsync();
                        LogHelper.Log("Ping");
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Subscription check: Ping failed - {ex.Message}");
                    }
                });

                lastPingSent = DateTime.Now;
            }

            // Check if we received a pong within the last 30 seconds
            var timeSinceLastPong = DateTime.Now - _wsManager.LastPongReceived;
            bool isPongRecent = _wsManager.LastPongReceived != DateTime.MinValue &&
                               timeSinceLastPong.TotalSeconds <= 30;

            if (isPongRecent)
            {
                // Reset reconnection attempts when connection is healthy
                reconnectionAttempts = 0;
                return true;
            }
            else
            {
                if (_wsManager.LastPongReceived == DateTime.MinValue)
                {
                    LogHelper.Log("Subscription check: No pong received yet");
                }
                else
                {
                    LogHelper.Log($"Subscription check: Invalid - Last pong was {timeSinceLastPong.TotalSeconds:F1} seconds ago");
                }

                // Trigger reconnection if no pong response and not too frequent
                TriggerDashboardReconnection();
                return false;
            }
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Subscription check error: {ex.Message}");
            return false;
        }
    }

    // Trigger reconnection through Dashboard form (not here)
    private void TriggerDashboardReconnection()
    {
        try
        {
            // Avoid reconnection attempts too frequently (minimum 60 seconds apart)
            var timeSinceLastReconnection = DateTime.Now - lastReconnectionAttempt;
            if (timeSinceLastReconnection.TotalSeconds < 60)
            {
                LogHelper.Log($"Reconnection skipped - last attempt was {timeSinceLastReconnection.TotalSeconds:F1} seconds ago");
                return;
            }

            // Limit reconnection attempts (max 5 attempts)
            if (reconnectionAttempts >= 5)
            {
                LogHelper.Log("Maximum reconnection attempts reached (5) - stopping reconnection attempts");
                return;
            }

            // Get Dashboard form from open forms
            var dashboard = Application.OpenForms.OfType<Dashboard>().FirstOrDefault();
            if (dashboard == null)
            {
                LogHelper.Log("Dashboard form not found - cannot trigger reconnection");
                return;
            }

            reconnectionAttempts++;
            lastReconnectionAttempt = DateTime.Now;

            LogHelper.Log($"🔄 Triggering WebSocket reconnection through Dashboard (attempt {reconnectionAttempts}/5)");

            // Use Invoke to ensure we're on the UI thread
            dashboard.BeginInvoke(new Action(() =>
            {
                _ = dashboard.TriggerWebSocketReconnection(); // fire-and-forget
            }));
        }
        catch (Exception ex)
        {
            LogHelper.Log($"Failed to trigger Dashboard reconnection: {ex.Message}");
        }
    }
}