# EU Smart Attendance Middleware — Configuration Reference

> Every hardcoded constant, timer interval, timeout, URL, and tunable value in the codebase.
> Change these when switching environments, tuning performance, or adjusting behaviour.
> Values are taken directly from source — no guessing.

---

## Table of Contents

1. [Environment — URLs & Endpoints](#1-environment--urls--endpoints)
2. [Cloud WebSocket (Pusher/Soketi)](#2-cloud-websocket-pushersoketi)
3. [Attendance Sync Timers](#3-attendance-sync-timers-attendancelogsynchservicecs--staffattendancelogsynchservicecs)
4. [Periodic Maintenance Timers](#4-periodic-maintenance-timers-periodicmaintenanceservicecs)
5. [Offline Enrollment Sync Timers](#5-offline-enrollment-sync-timers-offlineenrollmentsubscriptionservicecs)
6. [Local Device WebSocket Server](#6-local-device-websocket-server-websocketservercs)
7. [Connectivity Health Checker](#7-connectivity-health-checker-connectionstatuscheckerce)
8. [SQL Server Watchdog](#8-sql-server-watchdog-classessqlserverwatchdogcs)
9. [Backup Service](#9-backup-service-servicesbackupservicecs)
10. [HTTP Client Timeouts](#10-http-client-timeouts)
11. [Device Ping & Network Utils](#11-device-ping--network-utils-classescommonutilscs)
12. [UI Timers & Display Durations](#12-ui-timers--display-durations)
13. [Offline Member Limits](#13-offline-member-limits)
14. [Cache Manager](#14-cache-manager-classescachemanagercs)
15. [Quick-change Checklist (Per-environment)](#15-quick-change-checklist-per-environment)

---

## 1. Environment — URLs & Endpoints

### `Classes/HttpHandler.cs`

```csharp
// Line 25 — active
private const string BaseUrl = "https://eu.ws.fitnessmarvel.com";

// Line 27 — staging (swap by commenting/uncommenting)
//private const string BaseUrl = "https://ws-stg.fitnessmarvel.com";
```

| Constant | Line | Value | Description |
|---|---|---|---|
| `BaseUrl` | 25 | `https://eu.ws.fitnessmarvel.com` | Base URL for all REST API calls |
| `GetMembersEndpoint` | 28 | `/api/sync/members-staff` | Pull member + staff roster |
| `UploadAttendanceEndpoint` | 29 | `/api/attendance/upload` | Push attendance events |
| `GetCatalogEndpoint` | 30 | `/api/sync/catalog` | Pull packages + trainers |
| `PostEnrollmentsEndpoint` | 32 | `/api/sync/enrollments` | Push offline enrollments + renewals |
| `PostMemberPictureEndpoint` | 33 | `/api/sync/member-picture` | Upload member photo (base64) |
| `ApiKeySettingKey` | 35 | `"GWTToken"` | DB settings key used to read the bearer token |
| `CatalogSyncSettingKey` | 31 | `"CatalogSync"` | DB settings key for catalog watermark |

---

## 2. Cloud WebSocket (Pusher/Soketi)

### `Classes/WebSocketManager.cs`

```csharp
// Line 61 — active
private const string DefaultHost = "eu.ws.fitnessmarvel.com";

// Line 62 — staging (swap by commenting/uncommenting)
//private const string DefaultHost = "ws-stg.fitnessmarvel.com";
```

| Constant / Field | Line | Value | Description |
|---|---|---|---|
| `DefaultHost` | 61 | `eu.ws.fitnessmarvel.com` | Pusher/Soketi server hostname |
| `DefaultTlsPort` | 63 | `443` | WSS (secure) port |
| `DefaultWsPort` | 64 | `6001` | WS (plain) fallback port |
| `DefaultAppKey` | 65 | `ab28e935c0c63b8fabb4` | Pusher app key — must match server config |
| `PusherProtocolVersion` | 66 | `7` | Pusher protocol version string |
| `PusherClientName` | 67 | `csharp` | Client identifier sent on connect |
| `PusherClientVersion` | 68 | `1.0.0` | Client version string sent on connect |
| `_activityTimeoutSeconds` | 79 | `120` (server may override) | WebSocket keepalive = this ÷ 2 (min 15 s); server sends authoritative value on `connection_established` |

> **To switch environment:** comment out the active line and uncomment the staging line for both `HttpHandler.cs` **and** `WebSocketManager.cs`.

---

## 3. Attendance Sync Timers (`Classes/AttendanceLogSyncService.cs` & `Classes/StaffAttendanceLogSyncService.cs`)

Both files share the same structure and identical values.

| Field | Line (Member / Staff) | Value | Description |
|---|---|---|---|
| `_syncInterval` | 17 / 14 | `6 hours` (`6 * 60 * 60 * 1000 ms`) | Main full-sync cycle — pulls logs from all devices and pushes to cloud |
| `_retryDelay` | 18 / 15 | `30 seconds` (`30,000 ms`) | Retry delay after an unexpected error mid-cycle |
| `_earlyRetryInterval` | 19 / 16 | `15 minutes` (`15 * 60 * 1000 ms`) | Shortened next cycle when all devices fail to download |
| Per-device download retries | 247 / 241 | `3 attempts` | Max download attempts per device before marking as failed |
| Per-device retry gap | 248 / 242 | `2 seconds` | Delay between each device download retry |
| Attendance post spacing — normal | — | `1 second` | Delay between consecutive cloud attendance posts during sync |
| Attendance post spacing — recovery | — | `7 seconds` | Delay between posts when re-pushing previously unsynced records |
| Batch size — unsynced recovery | 575 / 814 | `100 records` | Max records per cloud batch during unsynced recovery |
| Completed-session check-in debounce | 131 / 685 | `10 seconds` | Extra debounce applied when a check-in session is already complete |

---

## 4. Periodic Maintenance Timers (`Classes/PeriodicMaintenanceService.cs`)

All timers share the same **10-minute initial startup delay** before their first fire.

| Field | Line | Value | Description |
|---|---|---|---|
| `_initialDelay` | 28 | `10 minutes` (`10 * 60 * 1000 ms`) | Delay before any timer fires after service start |
| `_fetchDeviceSyncStatusInterval` | 37 | `6 hours` | Compute synced/pending/missing member counts; fires **immediately** on startup (dueTime = 0) |
| `_delayAfterFetchForUpdate` | 39 | `10 seconds` | One-shot delay before refreshing Dashboard sync-status labels after each fetch |
| `_clearLogCheckInterval` | 29 | `9 hours` | How often to check whether device log clear is due |
| `CLEAR_INTERVAL_HOURS` | 75 | `47h 59m 59s` (`"47.9997222222"`) | Minimum elapsed time since last clear before clearing again |
| `_pendingJobsInterval` | 30 | `60 minutes` | How often to drain the `PendingJobs` queue via `DeviceFunctions.ProcessPendingJobsAsync()` |
| `_unsyncedInterval` | 33 | `10 minutes` | How often to retry any attendance records stuck in unsynced state |
| `_disableInactiveUsersInterval` | 34 | `8 hours` | How often to disable expired/inactive members on all devices |
| `_forceCheckOutInterval` | 38 | `1 hour` | How often to force check-out sessions that never received a check-out event |
| `LAST_CLEAR_TIME_KEY` | 74 | `"LastLogClearTime"` | DB settings key storing timestamp of last device log clear |

---

## 5. Offline Enrollment Sync Timers (`Classes/OfflineEnrollmentSubscriptionService.cs`)

Three independent timers run concurrently.

| Field / Constant | Line | Value | Description |
|---|---|---|---|
| `_syncInterval` (Timer 1 — catalog pull) | 23 | `60 minutes` (`60 * 60 * 1000 ms`) | How often to pull packages + trainers from `GET /api/sync/catalog` |
| `_retryDelay` (Timer 1 — catalog error) | 24 | `30 seconds` (`30 * 1000 ms`) | Delay before retrying a failed catalog pull |
| Enrollment push interval (Timer 2) | ~23 | `60 seconds` | How often to push offline members + renewals to cloud |
| `DevicePushIntervalMs` (Timer 3) | 43 | `60 seconds` (`60 * 1000 ms`) | How often to push offline member data to physical devices |
| `DevicePushQuickRetryMs` | 44 | `3 seconds` (`3,000 ms`) | Quick-retry delay after an on-demand device push failure |
| `DevicePushMaxAttempts` | 45 | `5 attempts` | Max device push attempts per offline member before giving up |
| `CatalogSyncSettingKey` | 21 | `"CatalogSync"` | DB key storing catalog pull watermark |

> ⚠️ **Older documentation says enrollment push is "10 minutes" — the code constant is 60 seconds. The code is correct.**

---

## 6. Local Device WebSocket Server (`WebsocketServer.cs`)

### Connection settings

| Value | Line | Current | Description |
|---|---|---|---|
| Listen port | 306 | `7788` | TCP port the local WS server binds to |
| Listen path | 306 | `/pub/chat` | WebSocket URL path devices connect to |
| Idle connection timeout | 308 | `1090 minutes (~18 hours)` | `HttpListener.TimeoutManager.IdleConnection` — keep device sockets alive overnight |

### Per-connection timeouts

| Value | Line | Current | Description |
|---|---|---|---|
| Initial ACK timeout | 472 | `10 seconds` | Wait for device's first handshake message after WebSocket upgrade |
| Session idle timeout | 472 | `15 minutes` | Max idle time after initial handshake before dropping connection |
| Max message size | 501 | `10 MB` | Hard limit on a single incoming WebSocket frame |
| Command ACK await | 1040 / 1248 | `15,000 ms (15 s)` | Default wait for device to acknowledge a sent command (`SendCommandAndAwaitResultAsync`) |

### Outbound scan & reconnect

| Value | Line | Current | Description |
|---|---|---|---|
| Port-scan timeout per IP | 348 | `1,500 ms` | Per-IP `TcpClient` connect timeout during subnet scan |
| Connect-out timeout | 904 | `10 seconds` | Timeout when app dials out to a known device IP |
| Startup settle delay | 925 | `30 seconds` | Initial wait before first unconnected-device retry scan |
| Retry scan interval | 940 | `60 seconds` | How often to retry connecting to unconnected known devices |

### Image processing (local service)

| Value | Line | Current | Description |
|---|---|---|---|
| `ImageProcessUrl` | 1287 | `http://10.20.69.81:5000/process` | URL of local image processing service — **hardcoded LAN IP, must be updated per deployment** |
| `_imageProcessClient.Timeout` | 1286 | `2 minutes` | HTTP timeout for image processing calls |

### Server start timeout (`Classes/WebsocketServerThreadManager.cs`)

| Value | Line | Current | Description |
|---|---|---|---|
| Ready wait timeout | 41 / 55 | `20 seconds` | Max time `ServiceManager.StartWebsocketServerAsync()` waits for the server to signal ready |

---

## 7. Connectivity Health Checker (`Classes/ConnectionStatusChecker.cs`)

### Internet check timer

| Value | Line | Current | Description |
|---|---|---|---|
| Normal check interval | 164 | `60,000 ms (1 min)` | Ping interval when internet is healthy |
| After 3 consecutive failures | 171 | `120,000 ms (2 min)` | Backoff after 3 failures |
| Beyond 3 failures | 173 | `300,000 ms (5 min)` | Further backoff |
| Ping target host | 188 | `google.com` | Host pinged to test internet connectivity |
| Ping timeout | 188 | `2,000 ms` | Per-ping timeout |

### Pusher subscription check timer

| Value | Line | Current | Description |
|---|---|---|---|
| Check interval | 221 | `15,000 ms (15 s)` | How often to verify Pusher pong |
| Pong staleness threshold | 285 | `30 seconds` | How old a received pong can be before subscription is considered stale |
| Min ping rate | — | `10 seconds` | Minimum gap between outbound pings (rate-limit guard) |

### Reconnection throttle

| Value | Line | Current | Description |
|---|---|---|---|
| Min gap between reconnects (normal) | 329 | `60 seconds` | Default minimum time between reconnection attempts |
| Min gap (pong OK, not subscribed) | 297 | `15 seconds` | Faster retry when socket is alive but channel not subscribed |
| Max consecutive attempts | 343 | `5` | After 5 attempts, enter cooldown |
| Cooldown duration | ~350 | `5 minutes` | Wait before resetting attempt counter after max hit |

---

## 8. SQL Server Watchdog (`Classes/SqlServerWatchdog.cs`)

| Constant | Line | Value | Description |
|---|---|---|---|
| `CheckIntervalMs` | 25 | `30,000 ms (30 s)` | How often background monitor checks SQL Server service status |
| `RestartTimeoutMs` | 26 | `60,000 ms (60 s)` | Max time to wait for service to reach `Running` state after start |
| `MaxRestartAttempts` | 27 | `3` | Restart attempts before firing `RestartFailed` event |
| Stop-pending wait (startup) | 71 | `15 seconds` | Wait for `StopPending` → `Stopped` transition on startup |
| Stop wait (restart) | 169 | `20 seconds` | Wait for service to fully stop before issuing `Start()` on restart |
| Gap between restart attempts | ~180 | `5 seconds` | `Thread.Sleep` between each restart attempt |

### Service names probed (in order)

| # | Service name | Instance |
|---|---|---|
| 1 | `MSSQLSERVER` | SQL Server default instance |
| 2 | `MSSQL$SQLEXPRESS` | SQL Server Express |

> To support a named instance (e.g. `MSSQL$MYINSTANCE`), add it to the `KnownServiceNames` array in `SqlServerWatchdog.cs`.

---

## 9. Backup Service (`Services/BackupService.cs`)

| Value | Line | Current | Description |
|---|---|---|---|
| `BackupInterval` | 10 | `5 hours` (`TimeSpan.FromHours(5)`) | How often backup runs |
| `EnableEmailBackup` setting | — | `"true"/"false"` in DB | Toggle email backup — changed at runtime via UI |
| `EnableGoogleDriveBackup` setting | — | `"true"/"false"` in DB | Toggle Google Drive backup — changed at runtime via UI |

> Both settings are re-read at every backup tick — no restart needed when toggled.

---

## 10. HTTP Client Timeouts

| Location | Line | Value | Description |
|---|---|---|---|
| `Classes/HttpHandler.cs` | 41 | `30 seconds` | `HttpClient` timeout for all cloud REST API calls |
| `WebsocketServer.cs` `_imageProcessClient` | 1286 | `2 minutes` | `HttpClient` timeout for local image processing service |

---

## 11. Device Ping & Network Utils (`Classes/CommonUtils.cs`)

| Constant | Line | Value | Description |
|---|---|---|---|
| `timeout` | 63 | `2,000 ms` | Per-ping timeout in `PingDeviceWithStatusAsync` |
| `maxRetries` | 64 | `3` | Ping retry count before returning `"OFFLINE"` |
| `retryDelayMs` | 65 | `2,000 ms` | Delay between ping retries |
| File copy `maxAttempts` | 101 | `3` | Retry count for `CopyFileAllowOpen` |
| File copy `delayMs` | 101 | `200 ms` | Gap between copy retries |

### `Helpers/NetworkHelper.cs`

| Value | Default | Description |
|---|---|---|
| `IsDeviceReachable` timeout | `1,000 ms` | Synchronous ping timeout (used for quick reachability checks) |

---

## 12. UI Timers & Display Durations

### `FacePopupForm.cs`

| Constant | Line | Value | Description |
|---|---|---|---|
| `DEFAULT_DISPLAY_SECONDS` | 21 | `7 seconds` | How long the face popup shows on each attendance event |

> The Dashboard passes `1.3` as `displaySeconds` for most attendance events, overriding the default. The default is only used when the form is constructed without an explicit duration.

### `UI/ToastNotificationForm.cs`

| Value | Line | Value | Description |
|---|---|---|---|
| Default display time | 41 | `5,000 ms (5 s)` | Default toast notification display duration |

---

## 13. Offline Member Limits

### `AddOfflineMemberForm.cs`

| Constant | Line | Value | Description |
|---|---|---|---|
| `MaxFaceFileBytes` | 450 | `1,000 KB (1 MB)` | Max allowed file size for uploaded face photo |
| `MaxFaceDimension` | 451 | `480 px` | Max width/height — photo is resized if larger |

### `Classes/DatabaseHelper.cs`

| Value | Line | Value | Description |
|---|---|---|---|
| Offline EnrollmentID seed | 1923 | `999998` | First fake EnrollmentID assigned to an offline member; each subsequent one decrements by 1 |

---

## 14. Cache Manager (`Classes/CacheManager.cs`)

| Constructor param | Default | Description |
|---|---|---|
| `refreshIntervalSeconds` | `3600 s (1 hour)` | How often `CacheManager` re-queries the DB and refreshes `AppSession` collections |

> Pass a different value when constructing `CacheManager` from `Dashboard.cs` to change the refresh frequency without modifying the class.

---

## 15. Quick-change Checklist (Per-environment)

Use this checklist whenever deploying to a new environment or switching staging ↔ production.

| Priority | File | Line | What to change |
|---|---|---|---|
| 🔴 Must | `Classes/HttpHandler.cs` | 25–27 | `BaseUrl` — swap comment for correct environment |
| 🔴 Must | `Classes/WebSocketManager.cs` | 61–62 | `DefaultHost` — swap comment for correct environment |
| 🔴 Must | `Classes/WebSocketManager.cs` | 65 | `DefaultAppKey` — must match Pusher/Soketi app key for environment |
| 🔴 Must | `WebsocketServer.cs` | 1287 | `ImageProcessUrl` — update to correct local image processing server IP |
| 🟡 Review | `Services/BackupService.cs` | 10 | `BackupInterval` — confirm `5h` is right for this deployment |
| 🟡 Review | `Classes/AttendanceLogSyncService.cs` | 17 | `_syncInterval` — `6h` default; reduce for higher-traffic gyms |
| 🟡 Review | `Classes/PeriodicMaintenanceService.cs` | 34 | `_disableInactiveUsersInterval` — `8h`; adjust to membership policy |
| 🟡 Review | `Classes/SqlServerWatchdog.cs` | 19–21 | `KnownServiceNames` — add named instance if not default/Express |
| 🟢 Optional | `FacePopupForm.cs` | 21 | `DEFAULT_DISPLAY_SECONDS` — `7s`; adjust face popup duration |
| 🟢 Optional | `Classes/OfflineEnrollmentSubscriptionService.cs` | 45 | `DevicePushMaxAttempts` — `5`; raise if devices are frequently unreachable |
| 🟢 Optional | `Classes/ConnectionStatusChecker.cs` | 343 | Max reconnect attempts — `5`; adjust for flaky WAN connections |
| 🟢 Optional | `Classes/DatabaseHelper.cs` | 1923 | Offline EnrollmentID seed — `999998`; change only if clashing with real IDs |
