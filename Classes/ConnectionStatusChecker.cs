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
    public bool IsSubscriptionValid { get; private set; }

    // Events that other parts of the app can subscribe to for real-time updates
    public event Action<bool> InternetStatusChanged;
    public event Action<bool> SubscriptionStatusChanged;

    private System.Threading.Timer internetCheckTimer;
    private System.Threading.Timer subscriptionCheckTimer;

    private int internetAttemptCounter = 0;
    private WebSocketManager _wsManager;
    private DatabaseHelper _dbHelper;

    // Track when we last sent a ping to avoid spamming
    private DateTime lastPingSent = DateTime.MinValue;

    // Track reconnection attempts to avoid spamming
    private DateTime lastReconnectionAttempt = DateTime.MinValue;
    private int reconnectionAttempts = 0;

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
            SubscriptionCheck(null);

            // Initialize timers within the new thread
            internetCheckTimer = new System.Threading.Timer(InternetCheck, null, 60000, 60000); // 1 minute
            subscriptionCheckTimer = new System.Threading.Timer(SubscriptionCheck, null, 15000, 15000); // 15 seconds

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
        subscriptionCheckTimer?.Dispose();
        internetCheckTimer = null;
        subscriptionCheckTimer = null;
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

            // Limit reconnection attempts to 5 in a row, then cool down and try again
            // instead of giving up permanently for the rest of the app's runtime.
            if (reconnectionAttempts >= 5)
            {
                if (timeSinceLastReconnection.TotalMinutes < 5)
                {
                    LogHelper.Log("Maximum reconnection attempts reached (5) - cooling down before retrying");
                    return;
                }

                LogHelper.Log("Reconnection cooldown elapsed - resetting attempt counter and retrying");
                reconnectionAttempts = 0;
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