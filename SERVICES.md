# Background Services and Key Components

Based on the services defined in your project, here is a detailed summary of their purpose, triggers, and execution schedules.

## `AttendanceLogSyncService`

This is the primary service for acquiring attendance data from devices and synchronizing it with the local database and remote server.

*   **Purpose**: Manages the end-to-end process of downloading logs from biometric devices, processing them, and syncing them to a remote server.
*   **Trigger**: Started once at application launch via `ServiceManager`.
*   **Execution Schedule**:
    *   **Initial Sync**: An immediate, full sync cycle runs as soon as the service starts.
    *   **Continuous Mode**: After the initial sync, the service will stop unless `_continuousSyncEnabled` is set to `true`.
    *   **Regular Interval**: If continuous mode is enabled, it runs every **6 hours** (`_syncInterval`).
    *   **Early Retry**: If a prioritized device sync (check-in or check-out) fails, the next cycle is scheduled for **15 minutes** (`_earlyRetryInterval`) instead of 6 hours.
    *   **Error Backoff**: If an unexpected error occurs during the sync loop, it waits for **30 seconds** (`_retryDelay`) before continuing.

## `PeriodicMaintenanceService`

This service handles various recurring background tasks to maintain the application and device state.

*   **Purpose**: Performs scheduled maintenance tasks like UI updates, log clearing, and syncing miscellaneous data.
*   **Trigger**: Started at application launch via `ServiceManager`, with an initial delay.
*   **Execution Schedule**: It uses multiple timers for different tasks.
    *   **Initial Delay**: The service waits **2 minutes** (`_initialDelay`) after starting before its timers begin firing.
    *   **Unsynced Attendance Sync**: Every **1 minute** (`_unsyncedInterval`), it triggers a bulk sync of any pending attendance logs.
    *   **UI & Status Updates**: Every **30 minutes** (`_updateInterval`), it updates status labels on the dashboard.
    *   **Device Log Clearing**: Every **1 hour** (`_clearLogCheckInterval`), it checks if it should clear general logs from the devices, a task that runs at most once every ~24 hours.
    *   **Pending Jobs Processing**: Every **60 minutes** (`_pendingJobsInterval`), it processes any queued jobs.
    *   **Inactive User Disabling**: Every **12 hours** (`_disableInactiveUsersInterval`), it runs a task to disable inactive users on the devices.

## `LogSyncService`

A more specialized, on-demand service for handling unsynced logs related to user workflows.

*   **Purpose**: Checks for unsynced logs and triggers a user processing workflow if any are found.
*   **Trigger**: Started on-demand, typically from UI actions within the `Dashboard`.
*   **Execution Schedule**:
    *   **Interval**: Once started, it runs every **1 hour** (`_intervalSeconds`) to check for unsynced logs. It can be stopped manually.

## Other Key Components

*   **`WebSocketManager`**: While not a traditional service with intervals, it's a long-running component that maintains a persistent, real-time connection to the server for immediate event streaming (like check-ins/check-outs) and receives commands. It is initialized on application startup and runs continuously with auto-reconnect logic.
*   **`EmailBackupService`**: As noted in the documentation, this service is responsible for creating and emailing a daily backup of the local SQLite database. It runs on a daily schedule.