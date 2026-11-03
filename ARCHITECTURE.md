# EU Smart Attendance Middleware — Complete Architecture Guide

> Single authoritative reference. Where older docs (`README.md`, `SERVICES.md`) conflict with this file or the source code, **source code wins, this document is second**. Stale discrepancies are called out explicitly throughout.

---

## Table of Contents

1. [What this application is](#1-what-this-application-is)
2. [Tech stack](#2-tech-stack)
3. [Startup flow](#3-startup-flow-programcs)
4. [Session & in-memory store (AppSession)](#4-session--in-memory-store-appsessioncs)
5. [Two networking roles](#5-two-networking-roles)
6. [Device layer — OCX / COM](#6-device-layer--ocx--com)
7. [Background services (ServiceManager)](#7-background-services-servicemanagercs)
8. [Periodic maintenance](#8-periodic-maintenance-periodicmaintenanceservicecs)
9. [Offline-first enrollment & renewal](#9-offline-first-enrollment--renewal)
10. [Connectivity health (ConnectionStatusChecker)](#10-connectivity-health-connectionstatuschecker)
11. [Cache manager (CacheManager)](#11-cache-manager-cachemanagercs)
12. [Backup service](#12-backup-service-servicesbackupservicecs)
13. [SQL Server watchdog](#13-sql-server-watchdog-classessqlserverwatchdogcs)
14. [Data models](#14-data-models)
15. [Data layer (DatabaseHelper)](#15-data-layer-databasehelpercs)
16. [Database write queue (DatabaseQueueManager)](#16-database-write-queue-classesdatabasequeuemanagercs)
17. [Attendance event processing (AttendenceServerFunctions)](#17-attendance-event-processing-attendenceserverfunctionscs)
18. [Utility classes](#18-utility-classes)
19. [Logging system (LogHelper & aiChatLogger)](#19-logging-system)
20. [UI map](#20-ui-map)
21. [File & directory layout](#21-file--directory-layout)
22. [Known issues / pre-production checklist](#22-known-issues--pre-production-checklist)
23. [Where to look for what](#23-where-to-look-for-what)

---

## 1. What this application is

A **Windows Forms desktop application (.NET Framework 4.8)**, branded **"FitnessMarvel"**, deployed on a Windows PC at a gym or fitness facility. It acts as **middleware between physical biometric attendance devices** (fingerprint / RFID / face clock-in terminals driven via vendor ActiveX/OCX SDKs) **and a cloud gym-management backend**.

### Two primary responsibilities

| # | Responsibility | Direction |
|---|---|---|
| 1 | Talk to local devices on the LAN — enroll/delete members, read punch logs, open doors, set device clock | App ↔ Device |
| 2 | Keep devices and the cloud in sync — push attendance events up, pull member/package/trainer data down; support **offline operation** (enroll/renew with no internet, queue work, reconcile later) | App ↔ Cloud |

Single-instance app (named Mutex). Entry point is a Login / first-run Setup screen; after login the `Dashboard` form becomes the composition root and starts every background service.

---

## 2. Tech stack

| Concern | Technology / Version |
|---|---|
| UI framework | WinForms, .NET Framework 4.8 (`OutputType=WinExe`) |
| Build platform | AnyCPU + **x86** — x86 required because vendor OCX SDKs are 32-bit COM |
| Device SDKs | ActiveX/OCX COM interop: `AxFP_CLOCKLib` (primary), `AxFPCLOCK_SVRLib` (server variant), `AxFPREADERLib` (reader), plus `Interop.*` wrappers for the same |
| Local database | **SQL Server** — all access via hand-rolled `DatabaseHelper.cs` using `System.Data.SqlClient`; no ORM |
| Cloud real-time | Hand-rolled Pusher/Soketi protocol client over `System.Net.WebSockets.ClientWebSocket` |
| Local real-time | Self-hosted `HttpListener`-based WebSocket **server** on TCP **port 7788** |
| JSON | Newtonsoft.Json 13.x throughout |
| Camera | AForge.Video / AForge.Video.DirectShow (offline member photo capture) |
| PDF | iTextSharp 5.5.x |
| Cloud backup | Google.Apis.Drive.v3 (optional, user-configurable) |
| Email backup | `System.Net.Mail.SmtpClient` |
| Embedded browser | Microsoft.Web.WebView2 |
| Logging | `Classes/LogHelper.cs` → `Logs/MiddlewareLog_{timestamp}.txt` per run |
| Crash log | `Program.cs` global handlers → `crash.log` in app directory |

---

## 3. Startup flow (`Program.cs`)

```
Main()
 ├─ 1. Acquire named Mutex (single-instance guard)
 ├─ 2. Hook global crash handlers
 ├─ 3. Register ApplicationExit handler
 ├─ 4. Firewall bootstrap (first run only)
 ├─ 5. Application.EnableVisualStyles()
 ├─ 6. SqlServerWatchdog.EnsureRunningOnStartup()   ← checks/starts SQL Server service
 ├─ 7. SqlServerWatchdog.StartMonitor()             ← begins 30s background health loop
 ├─ 8. new DatabaseHelper().InitializeDatabase()    ← create tables + run migrations
 └─ 9. AdminUserExists() ? Application.Run(new Login()) : Application.Run(new UserSetup())
```

### Detailed steps

**1 — Single-instance Mutex**
`Mutex("smart_attendance_middleware_single_instance")`. `WaitOne(TimeSpan.Zero)` — if another instance holds it, show `ModernMessageBox` warning and return immediately.

**2 — Global crash handlers**

| Handler | What it catches |
|---|---|
| `Application.ThreadException` | Unhandled exceptions on the UI thread |
| `AppDomain.CurrentDomain.UnhandledException` | Unhandled exceptions on background threads |
| `TaskScheduler.UnobservedTaskException` | Unobserved `Task` exceptions; calls `SetObserved()` to prevent process termination |

All three append a structured entry to `crash.log` (UTC timestamp, tag, message, type, stack trace, inner exception). A `crash_fallback.log` is written if even that fails.

**3 — ApplicationExit**
`OnApplicationExit`:
1. `DisableDebugMode()` — saves `DebugMode=false` to settings
2. `SqlServerWatchdog.StopMonitor()` — cancels background SQL Server health loop
3. `ServiceManager.StopLogSyncService()` — stops log sync
4. Releases and disposes the single-instance Mutex

**4 — Firewall bootstrap**
Marker file `.firewall_rule_created` prevents re-running. On first launch only:
```powershell
New-NetFirewallRule -DisplayName 'FitnessMarvel' -Direction Inbound -LocalPort 7788 -Protocol TCP -Action Allow -RemoteAddress Any
```
> ⚠️ `-RemoteAddress Any` exposes port 7788 to all IP ranges — see §22 item 2.

**6–7 — SQL Server watchdog** (see §13 for full detail)
Ensures the local SQL Server Windows service is running before any database call is made. If the service is down, attempts to start it and waits up to 60 s. Kicks off a background 30-second polling loop that auto-restarts the service if it crashes while the app is running.

**8 — Database initialization**
`InitializeDatabase()` creates all tables if they do not exist and applies incremental schema migrations defined in `DatabaseHelper.Incremental.cs`.

**9 — First form**
- `AdminUserExists()` true → `Login` form (credential check against `LocalUsers` table)
- `AdminUserExists()` false → `UserSetup` form (first-run admin account creation)

After successful login, `Dashboard` is the composition root. It:
- Creates `WebSocketManager` and connects to the Pusher/Soketi cloud channel
- Creates and starts `ConnectionStatusChecker`
- Calls `ServiceManager` to start all background services
- Loads initial data into `AppSession` via `CacheManager.InitialLoadAsync()`

---

## 4. Session & in-memory store (`AppSession.cs`)

Static class. Holds the live runtime state for the entire application. Populated by `CacheManager` on startup and updated incrementally by services.

### Authentication state

| Property | Type | Description |
|---|---|---|
| `LoggedInUser` | `string` | Username of the logged-in local user |
| `UserRole` | `string` | Role: `"admin"`, `"developer"`, etc. Controls `LogHelper.LogDev` visibility |
| `LoggedInUserID` | `int` | Primary key of the logged-in `LocalUsers` row |
| `LoginTime` | `DateTime` | When the current session started |
| `IsDebugMode` | `bool` | Whether debug mode is active (disabled on exit) |

### Device collections

| Property | Type | Description |
|---|---|---|
| `DeviceList` | `List<Device>` | All member-attendance biometric devices |
| `StaffDeviceList` | `List<Device>` | All staff-attendance biometric devices |
| `DeviceCategoryMap` | `Dictionary<string, (string type, int deviceId)>` | Keyed by normalised IP — maps `"member"/"staff"` + deviceId for fast routing |
| `RegisteredDevices` | `ConcurrentDictionary<string, WebSocket>` | Live WebSocket connections keyed by device serial number |

### Data collections

| Property | Type | Description |
|---|---|---|
| `AllMembers` | `List<Member>` | Full member roster |
| `AllStaff` | `List<Staff>` | Full staff roster |
| `AllAttendanceLogs` | `List<dynamic>` | Member attendance log records |
| `AllStaffAttendanceLogs` | `List<dynamic>` | Staff attendance log records |
| `AllGateLogs` | `List<GateAccessed>` | Door-open event log |
| `AllUsers` | `List<LocalUser>` | Local app user accounts |
| `PendingJobs` | `List<PendingJob>` | Queued device operations |
| `PendingBiometricSyncLogs` | `List<LogEntry>` | Member log rows with at least one un-synced biometric |
| `StaffPendingBiometricSyncLogs` | `List<StaffLogEntry>` | Same for staff |
| `StaffEnrollmentIds` | `HashSet<string>` | Set of known staff enrollment IDs for fast lookup |
| `SyncStatus` | `SyncStatusResult` | Last computed synced/pending/missing counts |
| `LastCheckIn` / `LastCheckOut` | `LastAttendanceDetails` | Most recent attendance event (shown in UI) |

### Key helpers

- `GetDeviceGateInfo(deviceId)` — returns `(GateSerial, GateAlias)` for a member device
- `GetStaffDeviceGateInfo(staffDeviceId)` — same for staff device
- `ClearSession()` — resets all properties; called on logout

---

## 5. Two networking roles

### 5a. Outbound — cloud client

#### REST API (`Classes/HttpHandler.cs`)

Base URL (currently staging):
```csharp
private const string BaseUrl = "https://ws-stg.fitnessmarvel.com";
// Production (commented out): "https://eu.ws.fitnessmarvel.com"
```

All requests include `Authorization: Bearer {GWTToken}` from settings. All responses parsed with Newtonsoft.Json.

| Endpoint | Method | Purpose |
|---|---|---|
| `/api/sync/members-staff` | GET | Pull full member + staff roster from cloud |
| `/api/attendance/upload` | POST | Push attendance events (check-in / check-out) to cloud |
| `/api/sync/catalog` | GET | Pull packages + trainers list for offline enrollment |
| `/api/sync/enrollments` | POST | Push queued offline member enrollments & membership renewals |
| `/api/sync/member-picture` | POST | Upload a member photo as base64 data URL |

`HttpHandler` also contains a `Processor` inner class that orchestrates multi-step API workflows for the attendance pipeline.

#### Real-time Pusher/Soketi (`Classes/WebSocketManager.cs`)

Connection parameters:
```csharp
private const string DefaultHost    = "ws-stg.fitnessmarvel.com";
private const int    DefaultTlsPort = 443;
private const int    DefaultWsPort  = 6001;
private const string DefaultAppKey  = "ab28e935c0c63b8fabb4";
```

Primary connection URL:
```
wss://ws-stg.fitnessmarvel.com/app/ab28e935c0c63b8fabb4?protocol=7&client=csharp&version=1.0.0
```
Falls back to cleartext `ws://...:6001/...` if TLS fails.

**Channel authentication:** subscribes to `private-gym.{GymId}`. Auth handshake:
```
POST https://{host}/pusher-auth
Authorization: Bearer {GWTToken}
Body: { socket_id, channel_name }
```
Both `GymId` and `GWTToken` come from the local settings store.

**Inbound events:**

| Event name | Handler action |
|---|---|
| `pusher:connection_established` | Record socket ID, mark connected |
| `pusher:ping` | Reply with `pusher:pong` |
| `pusher_internal:subscription_succeeded` | Set `IsSubscribed = true` |
| `pusher:subscription_error` / `pusher:error` | Log error + fire `LiveUpdate` event |
| `attendance_created` | Fire `LiveUpdate` → Dashboard refreshes attendance list |
| `attendance_updated` | Fire `LiveUpdate` → Dashboard refreshes attendance list |
| `user_created` / `user_updated` | Fire `LiveUpdate` → Dashboard refreshes member cache |
| `member_status` | Fire `LiveUpdate` → Dashboard updates status display |

**Outbound methods:**

| Method | Purpose |
|---|---|
| `SendCommandAsync(command, payload)` | Wrap as `client-command` Pusher event |
| `SendClientEventAsync(eventName, data)` | Generic `client-*` event |
| `ConnectionStatusAsync()` | Manual ping — used by `ConnectionStatusChecker` for health check |

> ⚠️ **TLS certificate validation is disabled.** `ServicePointManager.ServerCertificateValidationCallback` always returns `true`. Must be fixed before production hardening (§22 item 1).

---

### 5b. Inbound — local device server (`WebsocketServer.cs` + `WebsocketServerThreadManager.cs`)

Runs an `HttpListener` on `http://*:7788/` accepting WebSocket upgrades from physical biometric terminals at path `/pub/chat`. Started and managed by `WebsocketServerThreadManager`.

**Two connection modes:**

| Mode | Description |
|---|---|
| Device → App | Device dials in: `RegisterDevice(serialNumber, socket, ip)` adds the socket to `AppSession.RegisteredDevices` |
| App → Device | App scans subnet (`ScanSubnetAsync`), pings known IPs (`PingAndConnectKnownDevicesAsync`), connects (`ConnectToDeviceAndRegister(ip, sn)`) — retries unconnected devices in background |

**Static command helpers (called by other services):**

| Method | Purpose |
|---|---|
| `SendJsonToDevice(sn, json)` | Raw JSON send to device by serial number |
| `ExecuteServiceGetCommand(sn, method, params)` | Read-only device command |
| `ExecuteServiceCommand(sn, method, params)` | Mutating command e.g. `"deleteuser"` with `{enrollid, backupnum:12}` (12 = delete all templates) |
| `SendCommandAndAwaitResultAsync(sn, ackCmd, enrollId, json, timeoutMs=15000)` | Correlated request/response — awaits a specific ack event with matching enrollId |

`WebsocketServerThreadManager.Start(showUI)` spins the server on a background thread and fires a `Ready` event; `ServiceManager` awaits that event (20 s timeout fallback) before considering the server started.

---

## 6. Device layer — OCX / COM

### `Classes/DeviceConnector.cs`

Lightweight connection manager wrapping `AxFP_CLOCK`:

```csharp
bool Connect(string ip, int port, int machineNo, int password)
  → _ocx.SetIPAddress(ip, port, password)
  → _ocx.OpenCommPort(machineNo)

void Disconnect()
  → _ocx.CloseCommPort()
```

### `Classes/OcxThreadInvoker.cs` & `Helpers/ServerThreadInvoker.cs`

Every COM/OCX call must run on an **STA thread** with an active message pump.

- **`OcxThreadInvoker.Run(ip, ocx => …)`** — creates an invisible `Form`, adds `AxFP_CLOCK` OCX to it, runs `Application.Run(hostForm)` on a dedicated STA background thread; the OCX action runs on `hostForm.Load`; returns a `Task<T>` via `TaskCompletionSource`.
- **`ServerThreadInvoker.Run(ip, ocx => …)`** — same pattern but uses `AxFPCLOCK_Svr` (the server OCX variant). Used by operations that go through the SVR SDK path.

### `Classes/DeviceService.cs`

Business wrapper over `OcxThreadInvoker`. All methods are async-wrapped COM calls.

| Method | Description |
|---|---|
| `GetSerial(ip, port, machineNo, password)` | Read device serial number |
| `AddFace(…)` | Enroll face biometric template |
| `AddRfidCard(…)` | Enroll RFID card number |
| `AddFingerprint(…)` | Enroll fingerprint template |
| `SetUserName(…)` / `GetUserName(…)` / `GetUserInfo(…)` | User record CRUD on device |
| `EnableUser(…)` / `DisableUser(…)` | Toggle access permission on device |
| `DeleteEnrollmentData(…)` | Remove biometric templates for a user |
| `DeleteUserControl(…)` | Remove user entirely from device |
| `ReadAndSaveGLogData(…)` | Download member punch log → `GLog.{MachineNo}.txt` |
| `ReadAndSaveStaffGLogData(…)` | Download staff punch log → `StaffGLogs.{MachineNo}.txt` |
| `EmptyGeneralLogData(…)` | Clear punch log storage on device |
| `EmptyEnrollmentData(…)` | Clear all enrollment storage on device |
| `SetDeviceTime(…)` | Sync device clock to local machine time |
| `OpenDoor(…)` | Trigger door relay (gate open) |

**Safety patterns enforced throughout:**
- Disable device → perform sensitive op → re-enable in `finally`
- Always close comm port in `finally` — even if op throws
- Marshal unmanaged memory for photo enrollment; free in `finally`
- Normalise RFID card numbers as **unsigned 32-bit integers** before storage

### `Classes/DeviceFunctions.cs` (~1,458 lines)

Business-logic layer on top of `DeviceService`. Returns `DeviceOpResult { Success, ErrorReason, ConflictingId }`.

| Method | Purpose |
|---|---|
| `ProcessLogCreation(device)` | Download + save punch log for one member device |
| `ProcessStaffLogCreation(device)` | Same for one staff device |
| `CheckMissingFingerprints(device)` | Identify members enrolled on device but lacking FP templates |
| `ProcessAllEntries(devices)` | Full sync of all member data across all devices |
| `ProcessAllOfflineEntries()` | Push queued `OfflineMemberLog` entries to physical devices |
| `VerifyAndUpdateBiometricLogsAsync()` | Reconcile biometric log records |
| `EmptyEnrollmentDataAndDeleteLogsByDeviceId(id)` | Factory-reset enrollment data for a member device |
| `EmptyEnrollmentDataAndDeleteLogsByStaffDeviceId(id)` | Same for staff device |
| `DisableInactiveUsers()` | Disable expired / inactive members on every device (called every 8 h) |
| `ProcessPendingJobsAsync()` | Drain `PendingJobs` table — runs every 60 min |
| `StatusSync()` | Push current member statuses to devices |

### `Classes/DeviceErrorLogger.cs`

Parses structured device error JSON responses and logs failures to the DB:
- `ParseDeviceError(JObject response)` — extracts `reason` code, `msg`, and optional `ConflictingId` (parsed from `"face is double,id=XXXXX"`)
- `LogDeviceOperationFailure(dbHelper, logTable, enrollmentId, deviceSN, operationType, errorReason)` — upserts a row in the specified error log table with attempt count and failure reason

---

## 7. Background services (`Classes/ServiceManager.cs`)

Static façade providing `Start*Async()` / `Stop*()` for every long-running service. Each start method is **idempotent** (guards against double-start) and returns an awaitable `Task` backed by an internal `TaskCompletionSource<bool>`.

### Services managed

| Service | Class | Start method | Stop method |
|---|---|---|---|
| Local device WebSocket server | `WebsocketServerThreadManager` | `StartWebsocketServerAsync(showUI)` | `StopWebsocketServer()` |
| Log sync (coordination) | `LogSyncService` | `StartLogSyncServiceAsync(deviceFunctions)` | `StopLogSyncService()` |
| Member attendance sync | `AttendanceLogSyncService` | `StartAttendanceLogSyncServiceAsync()` | `StopAttendanceLogSyncService()` |
| Staff attendance sync | `StaffAttendanceLogSyncService` | `StartStaffAttendanceLogSyncServiceAsync()` | `StopStaffAttendanceLogSyncService()` |
| Periodic maintenance | `PeriodicMaintenanceService` | `StartPeriodicMaintenanceServiceAsync(dashboard)` | `StopPeriodicMaintenanceService()` |
| Offline enrollment | `OfflineEnrollmentSubscriptionService` | `StartOfflineEnrollmentSubscriptionServiceAsync()` | `StopOfflineEnrollmentSubscriptionService()` |

`StopAllServices()` stops in this exact order:
```
WebsocketServer → LogSync → AttendanceLogSync → StaffAttendanceLogSync → PeriodicMaintenance → OfflineEnrollmentSubscription
```

### Attendance sync pipeline (full detail)

```
ExecuteFullSyncCycleAsync
 └─ SyncAllDeviceLogsAsync
      ├─ Device priority order: check-in devices → check-out devices → all others
      ├─ DownloadDeviceLogsWithRetry (3 attempts, 2 s gap between attempts)
      │    └─ DeviceService.ReadAndSaveGLogData → GLog.{MachineNo}.txt
      │         on repeated failure: _earlyRetryRequested = true → next cycle in 15 min
      ├─ VerifyAndProcessGlogFilesAsync
      │    ├─ Parse all GLog.*.txt files
      │    ├─ Skip records with invalid / all-zero enroll numbers
      │    ├─ Insert raw records into DB
      │    └─ Delete processed GLog files
      └─ ProcessFilteredGLogRecordsAsync
           ├─ DatabaseHelper.FilterAndUpdateGLogRecordsByDateTwoWay
           ├─ Debounce window check (AttendanceDebounceSeconds setting)
           │    — blocks a check-out within window of open check-in, and vice versa
           ├─ RecordExistsInAttendanceLogAsync — skip if duplicate
           ├─ AttendenceServerFunctions.ProcessAttendanceEvent
           └─ LogAttendanceAsync (WebSocket + REST)
                ├─ 1 s spacing between normal filtered posts
                └─ 7 s spacing for unsynced-recovery posts
```

Default cycle: **6 hours**. Early-retry on failure: **15 minutes**. Unexpected error retry: **30 seconds**.

---

## 8. Periodic maintenance (`Classes/PeriodicMaintenanceService.cs`)

All timers share a **10-minute initial startup delay** (`_initialDelay = 10 min`). Each timer uses an **`Interlocked.CompareExchange` reentrancy guard** — if the previous tick is still running, the new tick is silently skipped with a log entry.

### Timer table (all current values from source)

| Timer field | First fire | Interval | Operation |
|---|---|---|---|
| `_fetchDeviceSyncStatusTimer` | **Immediately** (dueTime=0) | **6 hours** | Count synced members, pending biometric jobs, missing fingerprints; save to Settings; update `AppSession.SyncStatus`; schedule one-shot `UpdateSyncStatusLabels` 10 s later |
| `_logClearTimer` | 10 min | **9 hours** check cadence; clears only if ≥ 47 h 59 m 59 s since last clear | Send `cleanlog` WS command to all member + staff devices; clear `GLogs`/`StaffGLogs` DB tables; update `LastLogClearTime` setting |
| `_pendingJobsTimer` | 10 min | **60 min** | `DeviceFunctions.ProcessPendingJobsAsync()` — drain `PendingJobs` queue |
| `_unsyncedLogsTimer` | 10 min | **10 min** | `AttendanceLogSyncService.SyncUnsyncedAttendanceLogsAsync` + `StaffAttendanceLogSyncService.SyncUnsyncedStaffAttendanceLogsAsync` |
| `_disableInactiveUsersTimer` | 10 min | **8 hours** | `DeviceFunctions.DisableInactiveUsers()` — disable expired / inactive members on all devices |
| `_forceCheckOutTimer` | 10 min | **1 hour** | `CommonUtils.ProcessForceCheckOutAsync` — force check-out for sessions left open without a check-out event |

### One-time startup tasks (fire-and-forget via `Task.Run`, no timer)

| Task | Fires | What it does |
|---|---|---|
| `ExecuteOneTimeSyncedLogCleanup` | Immediately on `Start()` | Delete synced attendance logs older than `BackupRetentionDays` setting |
| `ExecuteOneTimeLongExpiredMembersCleanup` | After initial 10-min delay | Cascading delete for members with package expired > 2 months: remove from devices → log table → attendance logs → member table |

### UpdateSyncStatusLabels scheduling

After each `ExecuteFetchDeviceSyncStatusAsync()` completes, a **one-shot timer** fires 10 s later to call `Dashboard.RefreshDeviceSyncStatusAsync()` on the UI thread. If a previous one-shot is already pending it is disposed and replaced. The timer disposes itself after firing.

### Log clear gate (`ShouldClearLogs`)

Reads `LastLogClearTime` setting (format `yyyy-MM-dd HH:mm:ss`). If absent or parse fails → clear. If `DateTime.Now - lastClearTime >= 47h 59m 59s` → clear. Otherwise log remaining time and skip.

---

## 9. Offline-first enrollment & renewal

### `Classes/OfflineEnrollmentSubscriptionService.cs` (~780 lines)

Three independent `System.Threading.Timer`s running concurrently after service start.

#### Timer 1 — Catalog pull

| Item | Value |
|---|---|
| Interval | 60 min |
| Retry delay on error | 30 s |
| Endpoint | `GET /api/sync/catalog` |
| Tables written | `Packages`, `Trainers` |
| Watermark | `CatalogSync` setting — advanced only after full successful batch |

#### Timer 2 — Enrollment / renewal push

| Item | Value |
|---|---|
| Interval | **60 s** (code constant `EnrollmentSyncIntervalMs`; older docs incorrectly say "10 minutes") |
| Internet gate | `ConnectionStatusChecker.IsInternetConnected` must be `true` |

**Pass 1 — New offline members** (`OfflineMembers` rows with `ServerSync = false`):
```
For each unsynced offline member:
 ├─ POST /api/sync/member-picture  (photo as base64)
 │    └─ FAIL → skip this member entirely (never push without committed photo)
 └─ POST /api/sync/enrollments     (member record + SubscriptionJson)
      └─ SUCCESS → set ServerSync = true
```

**Pass 2 — Cleanup after successful server sync** (`TryCleanupOfflineMemberAsync`):
```
For each member with ServerSync = true:
 └─ For each device not yet confirmed in DeviceCleanupStatus:
      └─ WebsocketServer.ExecuteServiceCommand(sn, "deleteuser", {enrollid, backupnum:12})
           └─ SUCCESS → mark DeviceCleanupStatus[deviceId] = true
 └─ Once ALL devices confirmed:
      → Delete OfflineMemberLog rows for this member
      → Delete offlinefaces\{EnrollmentID}.png photo file
      → Delete OfflineMembers row
```

**Pass 3 — Pending renewals** (`SyncPendingRenewalsAsync`):
```
For each OfflineSubscriptions row:
 └─ POST /api/sync/enrollments
      └─ SUCCESS → write package_expiry back onto Member row → delete OfflineSubscriptions row
```

**On-demand helpers** (bypass timer, call immediately from UI after Save):
- `SyncPendingRenewalsWithQuickRetryAsync()` — push now, retry once in 3 s
- `PushOfflineEntriesToDevicesAsync()` — push offline device entries now

#### Timer 3 — Device push

| Item | Value |
|---|---|
| Interval | 60 s |
| Quick retry | 3 s after failure |
| Max attempts | 5 per member |
| Operation | `DeviceFunctions.ProcessAllOfflineEntries()` |

### Offline enrollment UI

#### `AddOfflineMemberForm.cs` (code-built — no Designer file)

- Captures live photo via AForge webcam feed or file browse (`OpenFileDialog`)
- Assigns **decrementing fake `EnrollmentID` starting at 999998** (999998 → 999997 → …) to avoid clashing with real cloud IDs
- Saves photo to `offlinefaces\{EnrollmentID}.png`
- Writes new row to `OfflineMembers` table with all personal fields + `SubscriptionJson`

#### `RenewMembershipForm.cs` (code-built — no Designer file)

Offline membership renewal:
1. Immediately flips local member to `Status = active`, clears `PackageExpiry`, enables them on every device already synced
2. Queues the subscription in `OfflineSubscriptions` for later server sync
3. When server sync succeeds, the authoritative `package_expiry` date from the server is written back to the local member row

---

## 10. Connectivity health (`Classes/ConnectionStatusChecker.cs`)

**Singleton** (`Lazy<ConnectionStatusChecker>`). Runs on its own dedicated STA thread. Other services read:
- `IsInternetConnected` — gate for all cloud uploads
- `IsSubscriptionValid` — gate for triggering reconnection

Events fired:
- `InternetStatusChanged` → `Action<bool>`
- `SubscriptionStatusChanged` → `Action<bool>`

### Internet check timer

| Item | Value |
|---|---|
| Normal interval | 60 s |
| After 3 failures | 2 min backoff |
| Beyond that | 5 min backoff |
| Recovery | Resets to 60 s on first success |
| Ping target | `google.com`, 2 s timeout |

### Subscription / WebSocket health timer

| Item | Value |
|---|---|
| Interval | 15 s |
| Ping rate-limit | Min 10 s between pings |
| Healthy = | Pong received within last 30 s **AND** `IsSubscribed = true` |

**Reconnection flow** (when subscription is stale):
```
TriggerDashboardReconnection()
 ├─ Throttle: once / 60 s max
 ├─ Max consecutive attempts: 5
 ├─ After 5 failures: 5-minute cooldown
 └─ Find live Dashboard via Application.OpenForms
      → Invoke(TriggerWebSocketReconnection) on UI thread
```

`SetWebSocketManager(wsManager)` must be called by Dashboard after it creates `WebSocketManager`. Until then, subscription checks are skipped.

---

## 11. Cache manager (`Classes/CacheManager.cs`)

Manages incremental updates to `AppSession` collections to avoid full DB re-queries on every UI refresh.

### Methods

| Method | Description |
|---|---|
| `InitialLoadAsync()` | Full load of all collections from DB into `AppSession` on startup |
| `StartRefreshTimer()` | Starts periodic background refresh (default every **3600 s / 1 hour**) |
| `StopRefreshTimer()` | Cancels periodic refresh |

### Reentrancy guard

`Interlocked.CompareExchange(ref _refreshInProgress, 1, 0)` — if a refresh is already running, the next timer tick is silently skipped. Prevents concurrent refreshes from racing on `AppSession` lists.

### Watermarks tracked

| Watermark field | Tracks last sync time for |
|---|---|
| `_lastMembersSync` | `AllMembers` |
| `_lastStaffSync` | `AllStaff` |
| `_lastAttendanceLogsSync` | `AllAttendanceLogs` |
| `_lastStaffAttendanceLogsSync` | `AllStaffAttendanceLogs` |
| `_lastGateLogsSync` | `AllGateLogs` |
| `_lastUsersSync` | `AllUsers` |
| `_lastPendingJobsSync` | `PendingJobs` |

### Event

`CacheUpdated` event (`EventHandler<CacheUpdateEventArgs>`) fired after each refresh — Dashboard subscribes to update UI bindings.

---

## 12. Backup service (`Services/BackupService.cs`)

| Item | Value |
|---|---|
| Interval | **5 hours** |
| Settings read each tick | `EnableEmailBackup`, `EnableGoogleDriveBackup` |
| Email | `System.Net.Mail.SmtpClient` — sends DB snapshot + log zip |
| Google Drive | `Google.Apis.Drive.v3` — uploads same bundle |

`RefreshSettings()` re-reads both flags at each backup tick so toggling them in the UI takes effect without a restart.

Backup bundle creation (`CommonUtils.CreateDatabaseSnapshotAndZipLogs`):
1. `DatabaseHelper.CreateDatabaseSnapshot()` — copies the live DB file using `FileStream` with `FileShare.ReadWrite` (handles in-use file)
2. Copies all `Logs/*.txt` files with the same open-file-safe copy method
3. ZIPs everything into a temp work directory
4. Returns the ZIP path for the email / Drive upload

---

## 13. SQL Server watchdog (`Classes/SqlServerWatchdog.cs`)

> **Requires Administrator** — restarting a Windows service requires elevated privileges. Ensure `app.manifest` contains `<requestedExecutionLevel level="requireAdministrator"/>`.

### Service name resolution

Probed in order; first successful `ServiceController.Status` read wins:
1. `MSSQLSERVER` — SQL Server default instance
2. `MSSQL$SQLEXPRESS` — SQL Server Express

### Startup check (`EnsureRunningOnStartup`)

Called from `Program.cs` **before** `InitializeDatabase()`.

```
1. ResolveServiceName() → if null: log + return false
2. sc.Status == Running → log "already running" + fire StatusChanged("running") + return true
3. Status == StopPending / StartPending → WaitForStatus(Stopped, 15 s)
4. sc.Start()
5. sc.WaitForStatus(Running, 60 s)
6. return (sc.Status == Running)
   └─ false → ModernMessageBox warning shown, but app continues (graceful degradation)
```

### Runtime monitor (`StartMonitor` / `StopMonitor`)

Background `Task` running until `CancellationTokenSource` is cancelled on `ApplicationExit`.

```
Loop (every 30 s):
 └─ sc.Refresh()
      ├─ Running → continue
      └─ Not running → TryRestart(sc)
           Attempts 1–3 (5 s pause between each):
            ├─ If StopPending: WaitForStatus(Stopped, 15 s)
            ├─ If not Stopped: sc.Stop() + WaitForStatus(Stopped, 20 s)
            ├─ sc.Start()
            └─ WaitForStatus(Running, 60 s)
                 └─ Success → fire StatusChanged("running (restarted)") + return true
           All 3 failed → fire RestartFailed(message) → ModernMessageBox.Error
```

### Events

| Event | Signature | Wired to in `Program.cs` |
|---|---|---|
| `StatusChanged` | `Action<string>` | `LogHelper.Log($"[SqlServer] {msg}")` |
| `RestartFailed` | `Action<string>` | `ModernMessageBox.Show(…, MsgType.Error)` |

---

## 14. Data models

All models are plain C# classes defined at the top of `Classes/DatabaseHelper.cs`. No EF annotations.

### `Member`
```
MemberID (int)          ClientId (string)       Name (string)
Status (string)         RFID (string)           Picture (string — file path)
EnrollmentID (int?)     Fingerprint (bool)      PackageExpiry (string)
is_staff (bool)         SyncStatus (Dictionary<int, bool>)   — keyed by DeviceId
```

### `Staff`
```
StaffID (string)        Name (string)           Status (string)
RFID (string)           Picture (string)        Fingerprint (bool)
EnrollmentID (int?)     SyncStatus (Dictionary<int, bool>)
```

### `Device`
```
DeviceId (int)          StaffDeviceId (int)     DeviceSerial (string)
DeviceIP (string)       Port (int)              MachineNo (int)
Password (string)       AttendanceType (bool)   StaffAttendanceType (string)
Department (string)     Location (string)       TimeZone (string)
ConnectionType (string) IsActive (bool)         LastSyncTime (string)
FaceCapacity (int)      RFIDCapacity (int)      FingerCapacity (int)
CreatedAt (string)      UpdatedAt (string)
```

### `LogEntry` (member attendance)
```
Id (int)                MemberID (int)          DeviceID (int)
EnrollmentID (int)      BiometricData (Dictionary<string,string>)
SyncStatus (Dictionary<string,bool>)
CreatedAt (string)      UpdatedAt (string)
```

### `StaffLogEntry`
Same shape as `LogEntry` but with `StaffID (string)` instead of `MemberID`.

### `OfflineLogEntry`
Like `LogEntry` but keyed by `ClientId (string)` instead of `MemberID`.

### `OfflineMember`
```
ClientId / Name / FirstName / LastName / Gender / Phone / Email / Whatsapp
SubscriptionJson (string — JSON)
RFID (string)           Fingerprint (bool)
EnrollmentID (int)      — fake, decrementing from 999998
ServerSync (bool)       — true once uploaded to cloud
Picture (string)        — path to offlinefaces\{EnrollmentID}.png
SyncStatus (Dictionary<int,bool>)        — device sync per DeviceId
DeviceCleanupStatus (Dictionary<int,bool>) — device deletion tracking after ServerSync
CreatedAt (string)
```

### `OfflineSubscription`
```
Id (int)    MemberId (int)    ClientId (string)    SubscriptionJson (string)
```

### `Package`
```
PackageId (int)         Title (string)          Code (string)
Description (string)    PricingTierJson (string) — e.g. {"monthly":5000,"quarterly":14000}
RegistrationFee (decimal?)    RegistrationFeeBasis (string)
GroupMode (string)      Active (bool)
```

### `Trainer`
```
TrainerId (string)    FirstName (string)    LastName (string)
Name (computed) = $"{FirstName} {LastName}".Trim()
```

### `LocalUser`
```
LocalUserID (int)     UserName (string)     Role (string)
Password (string)     BackupCode (string)   CreatedAt (string)    UpdatedAt (string)
```

### `FingerprintData`
```
EnrollNumber (int)      FingerNumber (int, 0–9)    Privilege (int — access level)
TemplateData (byte[])   TemplateString (string)
CreatedAt (string)      UpdatedAt (string)
```

### `BiometricChanges`
```
HasChanges (bool)    FingerprintChanged (bool)    RfidChanged (bool)    PictureChanged (bool)
```

### `LastAttendanceDetails`
```
MemberId (int)       EnrollmentId (int)      MemberName (string)
Action (string)      — "check-in" or "check-out"
Time (DateTime)      ImagePath (string)
```

---

## 15. Data layer (`Classes/DatabaseHelper.cs`)

A single hand-written SQL Server data-access class using `System.Data.SqlClient`. All queries use parameterized `SqlCommand` calls.

Schema creation and incremental migrations are in `Classes/DatabaseHelper.Incremental.cs` and run on every `InitializeDatabase()` call.

### Settings store (key/value table)

| Key | Type | Description |
|---|---|---|
| `GymId` | string | Cloud gym identifier for Pusher channel + REST auth |
| `GWTToken` | string | Bearer token for cloud API authentication |
| `AttendanceDebounceSeconds` | int | Window to block duplicate check-in / check-out |
| `CatalogSync` | string | Watermark for incremental catalog pull |
| `DebugMode` | `"true"/"false"` | Enables verbose logging; forcibly set to `false` on app exit |
| `LastLogClearTime` | `yyyy-MM-dd HH:mm:ss` | Timestamp of last device log clear |
| `MembersSynced` | int | Cached count: members fully synced across all devices |
| `PendingBiometricJobs` | int | Cached count: pending biometric sync jobs |
| `MissingFingerprints` | int | Cached count: members missing fingerprint templates |
| `BackupRetentionDays` | int | Days of synced attendance logs to keep |
| `EnableEmailBackup` | `"true"/"false"` | Enable email backup |
| `EnableGoogleDriveBackup` | `"true"/"false"` | Enable Google Drive backup |

### Key data-access methods (representative)

| Method | Purpose |
|---|---|
| `InitializeDatabase()` | Create tables + run incremental migrations |
| `AdminUserExists()` | Check if any `LocalUsers` row exists |
| `LoadDevices()` / `LoadStaffDevices()` | Load all device records into `List<Device>` |
| `SaveSetting(key, value)` / `GetSetting(key)` | Read/write key-value settings store |
| `FilterAndUpdateGLogRecordsByDateTwoWay` | Core attendance dedup + date range filter |
| `RecordExistsInAttendanceLogAsync(enrollId, deviceId, action, time)` | Duplicate guard before inserting |
| `GetUnsyncedLogEntriesAsync()` | Attendance rows with `SyncStatus` not fully true |
| `GetMembersWithMissingFingerprintsAsync()` | Members enrolled on a device but lacking FP template |
| `GetFullySyncedMembersCountAgainstAllDevices(onlyActiveDevices)` | Dashboard stat |
| `DeleteAllGLogs()` / `DeleteAllStaffGLogs()` | Purge raw log tables after device clear |
| `ExecuteNonQuery(sql)` | Raw non-parameterised query (used by `DeviceErrorLogger` — see §6) |
| `CreateDatabaseSnapshot()` | Copy live DB file using shared-read to a temp path; returns path |

---

## 16. Database write queue (`Classes/DatabaseQueueManager.cs`)

Serializes all concurrent DB writes from timers and services to prevent data corruption.

### Priority levels (`Classes/DbTaskPriority.cs`)

| Value | Int | Use case |
|---|---|---|
| `High` | 0 | Critical user-facing operations: create/update member |
| `Medium` | 1 | Status changes |
| `Low` | 2 | Background: attendance logging, sync, maintenance |

### Usage pattern

```csharp
await DatabaseQueueManager.EnterDbWriteLockAsync("MyOperationName", DbTaskPriority.Low);
try
{
    // perform DB write(s)
}
finally
{
    DatabaseQueueManager.ReleaseDbWriteLock("MyOperationName");
}
```

### Internals

- Internal `List<Waiter>` sorted by `Priority` (ascending = High first)
- `SemaphoreSlim`-style single-holder count (`_currentHolderCount = 0 or 1`)
- Each waiter holds a `TaskCompletionSource<bool>` resolved when lock is released
- Lock acquisition logged at `🔒` with caller name and priority
- Waiting logged at `⏳`

---

## 17. Attendance event processing (`Classes/AttendenceServerFunctions.cs`)

Handles incoming attendance records from devices and produces final cloud-pushed events.

### Key types

**`AttendanceRecord`** — raw event from device:
```
DevID (int)    IP (string)    Enroll (int)    Time (DateTime — device timestamp)
Name (string)  Image (string)
```

> ⚠️ `record.Time` is always used as-is from the device. It is **never replaced with `DateTime.Now`**.

**`AttendanceEventResult`** — result returned after processing:
```
Success (bool)      MemberId (int)        EnrollmentId (int)
Action (string)     EventTime (DateTime)  AttendanceId (string)   DeviceId (int?)
```

### `Process(AttendanceRecord record)` flow

1. Look up member by `EnrollmentID` in `AppSession.AllMembers`
2. Determine action (`check-in` or `check-out`) based on device type from `AppSession.DeviceCategoryMap`
3. Apply debounce: check `DatabaseHelper.RecordExistsInAttendanceLogAsync` within the configured window
4. Insert `LogEntry` row
5. Call `HttpHandler` → `POST /api/attendance/upload`
6. On success: update `AppSession.LastCheckIn` / `LastCheckOut`; fire `FacePopupForm` on UI thread; play sound

### `StaffAttendenceServerFunctions.cs`

Identical pipeline for staff devices. Uses `AppSession.StaffEnrollmentIds` for staff lookup and `StaffLogEntry` instead of `LogEntry`.

---

## 18. Utility classes

### `Classes/CommonUtils.cs`

Static utility bag. Key methods:

| Method | Description |
|---|---|
| `GetDeviceIpBySn(sn)` | Look up device IP from `AppSession.DeviceList` + `StaffDeviceList` by serial number |
| `GetDeviceSnByIp(ip)` | Reverse lookup — IP to serial number |
| `PingDeviceWithStatusAsync(ip, onStatusUpdate)` | Async ping with 3 retries, 2 s gaps; returns `"ONLINE"/"OFFLINE"/"RETRYING"` |
| `CopyFileAllowOpen(src, dest, maxAttempts, delayMs)` | Copy a file even if another process has it open (`FileShare.ReadWrite`) |
| `CreateDatabaseSnapshotAndZipLogs(dbHelper, includeDb, includeLogs)` | Bundle DB snapshot + `/Logs/*.txt` into a ZIP; returns ZIP path |
| `CleanupOldSyncedAttendanceLogsAsync(dbHelper)` | Delete synced logs older than `BackupRetentionDays` |
| `ProcessForceCheckOutAsync(dbHelper, httpHandler)` | Force check-out for open sessions older than threshold |
| `ProcessLongExpiredMembersCleanupAsync(dbHelper)` | Cascading delete for members expired > 2 months |

### `Helpers/NetworkHelper.cs`

```csharp
static bool IsDeviceReachable(string ipAddress, int timeout = 1000)
  → Ping.Send(ip, timeout) → true if IPStatus.Success
```
Synchronous; used in places where a quick reachability check is needed without the full async retry of `CommonUtils.PingDeviceWithStatusAsync`.

### `Classes/DeviceErrorLogger.cs`

See §6 for full detail. Parses device JSON error responses and writes failure records to DB error log tables.

---

## 19. Logging system

### `Classes/LogHelper.cs` — structured file logger

**Architecture:** producer/consumer using `BlockingCollection<string>`. `Log()` enqueues and returns immediately. A single dedicated background `Thread` named `"LogHelper.Writer"` is the only thread that ever writes to the file — no locking needed on the file handle.

| Item | Value |
|---|---|
| Log directory | `{AppBase}/Logs/` |
| File name | `MiddlewareLog_{yyyyMMdd_HHmmss}.txt` — new file per app run |
| Entry format | `{yyyy-MM-dd HH:mm:ss} - [{LoggedInUser}] {message}` |
| Console output | `Console.WriteLine` always (before queuing) |
| UI event | `LogUpdated` (`Action<string>`) — `UI/LogForm.cs` subscribes for live display |

**`LogDev(message)`** — only emits when `AppSession.UserRole == "developer"`. Use for raw API payloads, PII, or biometric data.

**`Close()`** — called on `AppDomain.ProcessExit`: `CompleteAdding()` to stop accepting entries, `Join(5 s)` to let the writer drain, then flush and close the `StreamWriter`.

### `Classes/aiChatLogger.cs` — AI assistant chat feed

Event-based bridge for the embedded AI assistant UI panel (WebView2). Raises events that the form subscribes to and injects as JavaScript calls.

**Events:**
- `OnAddMessage` — `Action<string icon, string message>` (icon is emoji, message is JS-escaped)
- `OnClearMessages` — `Action`

**Convenience methods:**

| Method | Icon |
|---|---|
| `AddSuccessMessage(msg)` | ✅ |
| `AddWarningMessage(msg)` | ⚠️ |
| `AddErrorMessage(msg)` | ❌ |
| `AddInfoMessage(msg)` | ℹ️ |
| `AddFingerprintMessage(msg)` | 👆 |
| `AddDeviceMessage(msg)` | 💻 |
| `AddUserMessage(msg)` | 👤 |
| `AddSyncMessage(msg)` | 🔄 |
| `AddConnectionMessage(msg)` | 📡 |
| `AddTimeMessage(msg)` | ⏰ |
| `AddAttendanceMessage(msg)` | 📋 |
| `AddDatabaseMessage(msg)` | 💾 |
| `AddNetworkMessage(msg)` | 🌐 |
| `AddSecurityMessage(msg)` | 🔒 |
| `AddConfigurationMessage(msg)` | ⚙️ |
| `AddGreeting()` | time-of-day greeting |
| `AddCelebrationMessage(achievement)` | 🎉 |
| `AddMotivationalMessage()` | random motivational string |
| `AddHelpfulTip(tip)` | 💡 |

---

## 20. UI map

### Main forms

| Form | File | Role |
|---|---|---|
| `Login` | `Login.cs` | Authentication — username + password against `LocalUsers` table |
| `UserSetup` | `UserSetup.cs` | First-run admin account creation |
| `Dashboard` | `Dashboard.cs` (~2,310 lines) | **Composition root** — starts/stops all services, member list, device summaries, sync status cards, AI chat panel |
| `DeviceManager` | `DeviceManager.cs` | List, add, remove, edit, test biometric devices |
| `AddNewDevice` | `AddNewDevice.cs` | Device form: IP, port, machine no, password, action type |
| `EditDevice` | `EditDevice.cs` | Edit existing device configuration |
| `AddNewUser` | `AddNewUser.cs` | New member enrollment |
| `EditUser` | `EditUser.cs` | Edit existing member record |
| `FingerManagement` | `FingerManagement.cs` | Fingerprint template management per member |
| `Fingerprint` | `Fingerprint.cs` | OCX-based fingerprint capture UI |
| `Fingerprintmanagment` | `Fingerprintmanagment.cs` | Batch fingerprint template management |
| `FacePopupForm` | `FacePopupForm.cs` | Bottom-right popup on attendance event; 3 s default display |
| `CropForm` | `CropForm.cs` | Photo cropping utility |
| `MemberDeviceStatusDialog` | `MemberDeviceStatusDialog.cs` | Per-member per-device sync status diagnostics |
| `RenewMembershipForm` | `RenewMembershipForm.cs` | Offline membership renewal (code-built, no Designer) |
| `AddOfflineMemberForm` | `AddOfflineMemberForm.cs` | Offline member enrollment (code-built, no Designer) |
| `VisitorForm` | `VisitorForm.cs` | Walk-in / visitor logging |
| `WebsocketServer` | `WebsocketServer.cs` | Hosts local device WebSocket server (also a visible form) |
| `MembershipManagement` | `MembershipManagement.cs` | Membership list + management |
| `FailureReasonDetailForm` | `FailureReasonDetailForm.cs` | Detail view of a failed device/sync operation |
| `ModernMessageBox` | `ModernMessageBox.cs` | Styled replacement for `MessageBox.Show` — types: `Info`, `Warning`, `Error`, `Success` |

### UI sub-windows

| Form | File | Role |
|---|---|---|
| `UI/LogForm` | `UI/LogForm.cs` | Live log viewer — subscribes to `LogHelper.LogUpdated` |
| `UI/LoaderPopupForm` | `UI/LoaderPopupForm.cs` | Full-screen blocking loader overlay |
| `UI/ToastNotificationForm` | `UI/ToastNotificationForm.cs` | Transient toast notifications (bottom-right) |

### Custom controls (`Controls/`)

| Control | Purpose |
|---|---|
| `CircularLoader` | Spinning circular progress indicator |
| `RectangularLoader` | Rectangular progress/loading bar |
| `DataPairPanel` | Label + value pair display (used for device status fields) |
| `BottomStatusBar` | Status bar shown at the bottom of the Dashboard |
| `HeaderStatusCard` | Large header card with connection/sync status |
| `StatCard` | Single-stat display card (number + label) |
| `TileCard` | Clickable tile used in device and member grids |
| `PillsPanel` | Horizontal row of pill/tag badges |
| `CardListView` | Card-based list view (alternative to standard ListView) |
| `ModernListView` | Enhanced `ListView` with custom draw |
| `ModernTabPanel` | Tabbed panel with custom-drawn tab headers |
| `RoundedButton` | Button with rounded corners |
| `RoundedTextBox` | TextBox with rounded corners |
| `RoundedComboBox` | ComboBox with rounded corners |
| `ToggleSwitch` | On/off toggle control |

---

## 21. File & directory layout

```
{AppBase}/
 ├─ smart-attendance-middleware.exe          Main executable
 ├─ App.config                               Connection strings + appSettings
 ├─ app.manifest                             UAC manifest (should request requireAdministrator)
 ├─ crash.log                                Appended on unhandled exceptions
 ├─ crash_fallback.log                       Written if crash.log write itself fails
 ├─ .firewall_rule_created                   Marker: firewall rule already created (first-run gate)
 ├─ Logs/
 │   └─ MiddlewareLog_{yyyyMMdd_HHmmss}.txt  New log file per app run
 ├─ offlinefaces/
 │   └─ {EnrollmentID}.png                   Cached photos for offline members
 ├─ GLog.{MachineNo}.txt                     Temporary: member device punch log (deleted after parse)
 └─ StaffGLogs.{MachineNo}.txt              Temporary: staff device punch log (deleted after parse)

Source tree:
 Classes/
  AppSession.cs                 In-memory session / live collections
  AttendenceServerFunctions.cs  Attendance event processing (member)
  StaffAttendenceServerFunctions.cs  Same for staff
  BackupService.cs              Email + Google Drive backup (5h interval)
  CacheManager.cs               Incremental AppSession refresh
  CommonUtils.cs                General utilities (ping, file copy, ZIP, cleanup)
  ConnectionStatusChecker.cs    Internet + Pusher subscription health
  DatabaseHelper.cs             All SQL Server data access (hand-rolled)
  DatabaseHelper.Incremental.cs Incremental schema migrations
  DatabaseQueueManager.cs       Priority write-lock queue for DB writes
  DbTaskPriority.cs             Enum: High=0, Medium=1, Low=2
  DeviceConnector.cs            Low-level OCX connect/disconnect
  DeviceErrorLogger.cs          Parse + log device error responses
  DeviceFunctions.cs            Device business logic (~1,458 lines)
  DeviceOperations.cs           Additional device operation helpers
  DeviceService.cs              OCX wrapper (async-friendly COM calls)
  HttpHandler.cs                Cloud REST API client
  LogHelper.cs                  File logger (producer/consumer queue)
  LogSyncService.cs             Coordination-only log sync service
  NetworkHelper.cs (Helpers/)   Synchronous device ping
  OfflineEnrollmentSubscriptionService.cs  Offline sync engine (~780 lines)
  PeriodicMaintenanceService.cs Periodic timers (6 independent) + one-time startup tasks
  ServiceManager.cs             Start/stop façade for all services
  ServerThreadInvoker.cs (Helpers/) STA thread runner for AxFPCLOCK_Svr
  OcxFpreaderInvoker.cs         STA thread runner for AxFPREADERLib
  OcxThreadInvoker.cs           STA thread runner for AxFP_CLOCK
  SqlServerWatchdog.cs          SQL Server service health + auto-restart
  WebsocketServerThreadManager.cs  Local WS server thread management
  aiChatLogger.cs               AI assistant chat feed (event bridge)

 UI/
  LogForm.cs          Live log viewer
  LoaderPopupForm.cs  Blocking loader overlay
  ToastNotificationForm.cs  Toast notifications

 Controls/
  (see §20 Custom controls table)
```

---

## 22. Known issues / pre-production checklist

| # | Severity | Issue | Location | Fix |
|---|---|---|---|---|
| 1 | 🔴 High | **TLS cert validation disabled** — any certificate accepted | `WebSocketManager.ConnectAsync` — `ServicePointManager.ServerCertificateValidationCallback = () => true` | Remove override; validate the real server certificate |
| 2 | 🔴 High | **Firewall rule `-RemoteAddress Any`** — port 7788 reachable from any IP | `Program.cs` `CreateFirewallRule()` | Restrict to LAN subnet e.g. `-RemoteAddress "192.168.1.0/24"` |
| 3 | 🔴 High | **App ships pointing at staging** (`ws-stg.fitnessmarvel.com`) | `HttpHandler.cs` and `WebSocketManager.cs` — production URL only in a comment | Swap both constants to `eu.ws.fitnessmarvel.com` for production deploy; consider making this a config value |
| 4 | 🟡 Medium | **SQL Server watchdog silently fails if not Administrator** | `SqlServerWatchdog.cs` — `ServiceController.Start()` throws `InvalidOperationException` if not elevated; exception is caught and logged | Verify `app.manifest` requests `requireAdministrator`; add user-visible warning if not elevated |
| 5 | 🟡 Medium | **Enrollment push interval 60 s in code, described as "10 minutes" in older docs** | `OfflineEnrollmentSubscriptionService.cs` `EnrollmentSyncIntervalMs` | Documentation issue only; treat the source constant as truth |
| 6 | 🟢 Low | **`DeviceErrorLogger.LogDeviceOperationFailure` uses string interpolation in SQL** | `Classes/DeviceErrorLogger.cs` line 58–70 — `escapedReason` is manually escaped with `Replace("'", "''")` | Refactor to use `SqlCommand` parameters |

---

## 23. Where to look for what

| Need to understand… | Start at |
|---|---|
| App boot sequence | `Program.cs` |
| SQL Server health / auto-restart | `Classes/SqlServerWatchdog.cs` |
| Main UI / service orchestration | `Dashboard.cs` |
| Session state / in-memory data | `Classes/AppSession.cs` |
| Talking to a biometric device (COM/OCX) | `Classes/DeviceService.cs`, `Classes/DeviceFunctions.cs` |
| COM STA thread plumbing | `Classes/OcxThreadInvoker.cs`, `Helpers/ServerThreadInvoker.cs` |
| Local device-facing WS server (port 7788) | `WebsocketServer.cs`, `Classes/WebsocketServerThreadManager.cs` |
| Cloud real-time (Pusher/Soketi) | `Classes/WebSocketManager.cs` |
| Cloud REST calls | `Classes/HttpHandler.cs` |
| Attendance event pipeline | `Classes/AttendenceServerFunctions.cs`, `Classes/StaffAttendenceServerFunctions.cs` |
| Attendance sync (device → DB → cloud) | `Classes/AttendanceLogSyncService.cs`, `Classes/StaffAttendanceLogSyncService.cs` |
| Periodic maintenance timers | `Classes/PeriodicMaintenanceService.cs` |
| Service start / stop | `Classes/ServiceManager.cs` |
| Connectivity / health monitoring | `Classes/ConnectionStatusChecker.cs` |
| In-memory cache refresh | `Classes/CacheManager.cs` |
| Offline enroll / renew UI | `AddOfflineMemberForm.cs`, `RenewMembershipForm.cs` |
| Offline sync engine | `Classes/OfflineEnrollmentSubscriptionService.cs` |
| Backup (email / Google Drive) | `Services/BackupService.cs` |
| ZIP + DB snapshot utility | `Classes/CommonUtils.cs` → `CreateDatabaseSnapshotAndZipLogs` |
| All SQL Server data access | `Classes/DatabaseHelper.cs` + `Classes/DatabaseHelper.Incremental.cs` |
| DB write serialization / priority queue | `Classes/DatabaseQueueManager.cs`, `Classes/DbTaskPriority.cs` |
| All C# data models | Top of `Classes/DatabaseHelper.cs` |
| Device error parsing + logging | `Classes/DeviceErrorLogger.cs` |
| File / network utilities | `Classes/CommonUtils.cs`, `Helpers/NetworkHelper.cs` |
| File logger | `Classes/LogHelper.cs` → `Logs/MiddlewareLog_*.txt` |
| AI assistant chat feed | `Classes/aiChatLogger.cs` |
| Crash log | `Program.cs` crash handlers → `crash.log` |
