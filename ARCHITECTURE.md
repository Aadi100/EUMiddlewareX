# EU Smart Attendance Middleware — Complete Architecture Guide

> Single-file reference for understanding this codebase end to end. Supersedes/consolidates `README.md` and `SERVICES.md` (which remain in the repo but contain some stale numbers — this doc calls those out explicitly and treats current source as ground truth).

## 1. What this application is

A **Windows Forms desktop app, .NET Framework 4.8**, branded "FitnessMarvel", that runs on a PC at a gym/fitness facility and acts as the **middleware between physical biometric attendance devices** (fingerprint / RFID / face clock-in terminals, driven via vendor ActiveX/OCX SDKs) **and a cloud gym-management backend**.

It has two jobs:
1. Talk to the devices on the local network: enroll/delete members, read punch logs, open doors, set device time.
2. Keep the cloud backend and the devices in sync: push attendance events up, pull member/package/trainer data down, and support **offline operation** (enroll/renew members with no internet, queue the work, reconcile later).

It is a single-instance app (named Mutex) with a Login/first-run Setup screen, then a `Dashboard` main window that starts and supervises all background services.

## 2. Tech stack

| Concern | Technology |
|---|---|
| UI | WinForms, .NET Framework 4.8 (`OutputType=WinExe`) |
| Build platforms | AnyCPU and **x86** (x86 required — device OCX SDKs are 32-bit COM) |
| Device SDK | ActiveX/OCX COM interop: `AxFP_CLOCKLib`, `AxFPCLOCK_SVRLib`, `AxFPREADERLib` (+ raw `Interop.*` variants) |
| Local persistence | SQLite via hand-rolled `DatabaseHelper.cs` (NOT Entity Framework, despite EF6 being referenced — see §9) |
| Cloud real-time | Hand-rolled Pusher/Soketi protocol client over raw `System.Net.WebSockets.ClientWebSocket` |
| Local real-time | Self-hosted `HttpListener`-based WebSocket **server** on TCP port 7788 for inbound device connections |
| JSON | Newtonsoft.Json 13.x everywhere |
| Camera | AForge.Video / AForge.Video.DirectShow (offline member photo capture) |
| PDF | iTextSharp 5.5.x |
| Cloud backup | Google.Apis.Drive.v3 (optional) |
| Embedded browser | Microsoft.Web.WebView2 |

## 3. Startup flow (`Program.cs`)

1. Acquire named `Mutex("smart_attendance_middleware_single_instance")` — exit with a MessageBox if another instance holds it.
2. Hook global crash handlers (`Application.ThreadException`, `AppDomain.UnhandledException`, `TaskScheduler.UnobservedTaskException`) → append to `crash.log` in the app directory.
3. Register `Application.ApplicationExit` → disable debug mode, `ServiceManager.StopLogSyncService()`, release mutex.
4. **Firewall bootstrap (new, recent commits)**: on first run only (marker file `.firewall_rule_created`), shell out to PowerShell:
   ```
   New-NetFirewallRule -DisplayName 'FitnessMarvel' -Direction Inbound -LocalPort 7788 -Protocol TCP -Action Allow -RemoteAddress Any
   ```
   This opens inbound TCP 7788 **to all remote addresses, not just the LAN** — see security note in §12.
5. `Application.EnableVisualStyles()`.
6. `new DatabaseHelper().InitializeDatabase()` — creates/migrates local SQLite DB.
7. If an admin user exists → `Login` form; else → `UserSetup` (first-run admin creation).

Once logged in, `Dashboard` becomes the composition root: it connects `WebSocketManager`, starts `ConnectionStatusChecker`, and starts every service through `ServiceManager`.

## 4. Two networking roles

### 4a. Outbound — cloud client

- **REST** via `Classes/HttpHandler.cs`. Base URL (currently staging, production commented out):
  ```csharp
  private const string BaseUrl = "https://ws-stg.fitnessmarvel.com"; // prod: eu.ws.fitnessmarvel.com (commented out)
  ```
  Endpoints:
  | Endpoint | Purpose |
  |---|---|
  | `GET /api/sync/members-staff` | pull member/staff roster |
  | `POST /api/attendance/upload` | push attendance events |
  | `GET /api/sync/catalog` | pull packages/trainers catalog (for offline enrollment) |
  | `POST /api/sync/enrollments` | push queued offline member enrollments & renewals |
  | `POST /api/sync/member-picture` | upload a member's photo (base64 data URL) |

- **Real-time** via `Classes/WebSocketManager.cs` — a hand-rolled Pusher/Soketi client:
  ```csharp
  private const string DefaultHost = "ws-stg.fitnessmarvel.com"; // prod: eu.ws.fitnessmarvel.com (commented out)
  private const int DefaultTlsPort = 443;
  private const int DefaultWsPort  = 6001;
  private const string DefaultAppKey = "ab28e935c0c63b8fabb4";
  ```
  Connects to `wss://ws-stg.fitnessmarvel.com/app/ab28e935c0c63b8fabb4?protocol=7&client=csharp&version=1.0.0`, falls back to cleartext `ws://...:6001/...` if TLS fails. Subscribes to private channel `private-gym.{GymId}`, authenticated via `POST https://{host}/pusher-auth` with `Authorization: Bearer {GWTToken}` (both `GymId`/`GWTToken` are local settings).

  Inbound events handled: `pusher:connection_established`, `pusher:ping/pong`, `pusher_internal:subscription_succeeded`, `pusher:subscription_error`/`pusher:error`, plus app events `attendance_created`, `attendance_updated`, `user_created`, `user_updated`, `member_status` — each re-raised via the `LiveUpdate` event for `Dashboard` to react to.

  Outbound: `SendCommandAsync(command, payload)` wraps as a `client-command` event; `SendClientEventAsync` for generic `client-*` events; `ConnectionStatusAsync()` does a manual ping used for health checks.

  **⚠️ TLS certificate validation is disabled** — `ServicePointManager.ServerCertificateValidationCallback` always returns `true`. Flag for remediation before production hardening.

### 4b. Inbound — local device server

`WebsocketServer.cs` (a `Form`) + `WebsocketServerThreadManager.cs` run an `HttpListener` on `http://*:7788/`, accepting WebSocket upgrades from physical devices at path `/pub/chat`. The relationship is bidirectional:
- Devices can connect **into** this app (`RegisterDevice(sn, socket, ip)`), or
- The app can scan the subnet and connect **out** to devices (`ScanSubnetAsync`, `PingAndConnectKnownDevicesAsync`, `ConnectToDeviceAndRegister(ip, serialNumber)`), retrying unconnected devices in the background.

Static helpers used by other services to talk to a device by serial number:
- `SendJsonToDevice(sn, json)`
- `ExecuteServiceGetCommand(sn, methodName, jsonParams)`
- `ExecuteServiceCommand(sn, methodName, jsonParams)` — e.g. `methodName="deleteuser"`, `{enrollid, backupnum:12}` (12 = delete all templates: fingerprint/face/card/password)
- `SendCommandAndAwaitResultAsync(sn, ackCmd, enrollId, json, timeoutMs=15000)` — correlates request/response by enrollId

## 5. Device layer (OCX)

- `DeviceService` wraps the vendor OCX (`AxFP_CLOCKLib`), called through `OcxThreadInvoker.Run(ip, ocx => ...)` to keep all COM calls on a single STA thread per device.
- Key operations: `GetSerial`, `AddFace`, `AddRfidCard`, `AddFingerprint`, `SetUserName`/`GetUserName`/`GetUserInfo`, `EnableUser`/`DisableUser`, `DeleteEnrollmentData`, `DeleteUserControl`, `ReadAndSaveGLogData` (writes `GLog.{EMachineNo}.txt`), `ReadAndSaveStaffGLogData` (writes `StaffGLogs.{EMachineNo}.txt`), `EmptyGeneralLogData`, `EmptyEnrollmentData`, `SetDeviceTime`, `OpenDoor`.
- Safety pattern used throughout: disable device → do sensitive op → re-enable in `finally`; always close the comm port in `finally`; marshal unmanaged memory for photo enrollment; normalize RFID numbers as unsigned 32-bit.
- `Classes/DeviceFunctions.cs` (1,458 lines) is the business-logic layer on top: `ProcessLogCreation`, `ProcessStaffLogCreation`, `CheckMissingFingerprints`, `ProcessAllEntries`, `ProcessAllOfflineEntries` (pushes queued offline enrollments to real devices), `VerifyAndUpdateBiometricLogsAsync`, `EmptyEnrollmentDataAndDeleteLogsByDeviceId(ByStaffDeviceId)`, `DisableInactiveUsers`, `ProcessPendingJobsAsync`, `StatusSync`. Returns a `DeviceOpResult { Success, ErrorReason, ConflictingId }`.

## 6. Background services (`Classes/ServiceManager.cs`)

Static façade exposing `Start*Async()`/`Stop*()` for every long-running service, each idempotent (guards against double-start) and awaitable via an internal `Started` `TaskCompletionSource`.

| Service | Trigger | Interval | Notes |
|---|---|---|---|
| `WebsocketServer` (local, port 7788) | app start | continuous | 20s startup timeout fallback |
| `LogSyncService` | on-demand from UI/`DeviceFunctions` | — | short-lived coordination during device-driven ops |
| `AttendanceLogSyncService` | app start | 6h cycle, 15 min early retry on priority-device failure, 30s retry on unexpected error | per-device retries: 3 attempts, 2s apart |
| `StaffAttendanceLogSyncService` | app start | mirrors above for staff devices | |
| `PeriodicMaintenanceService` | 15 min after start | every 30 min | clears device general logs once per 24h; disables inactive users |
| `OfflineEnrollmentSubscriptionService` | app start | see §7 | catalog pull + offline-queue push |

`ServiceManager.StopAllServices()` stops everything in order: websocket server → log sync → attendance sync → staff attendance sync → maintenance → offline enrollment.

> **Doc-vs-code discrepancy**: `README.md`/`SERVICES.md` describe `ServiceManager` methods taking a `WebSocketManager` parameter (e.g. `StartAttendanceLogSyncService(wsManager)`). The current source exposes parameterless async variants (`StartAttendanceLogSyncServiceAsync()`, `StartOfflineEnrollmentSubscriptionServiceAsync()`). **Treat `ServiceManager.cs` source as ground truth.**

### Attendance sync pipeline (detail)

1. `ExecuteFullSyncCycleAsync` → `SyncAllDeviceLogsAsync` prioritizes devices: check-in devices → check-out devices → others. `DownloadDeviceLogsWithRetry` (3 attempts, 2s apart) calls `DeviceService.ReadAndSaveGLogData` → `GLog.{EMachineNo}.txt`.
2. `VerifyAndProcessGlogFilesAsync` parses all `GLog.*.txt`, skips invalid/all-zero enroll numbers, inserts raw records into DB, deletes processed files.
3. `ProcessFilteredGLogRecordsAsync` — gets records via `DatabaseHelper.FilterAndUpdateGLogRecordsByDateTwoWay`, maps `DeviceId → machineNo → action`, applies a **debounce window** (`Settings.AttendanceDebounceSeconds`): blocks a check-out within the window of an open check-in, and vice versa; checks for duplicates (`RecordExistsInAttendanceLogAsync`); calls `AttendenceServerFunctions.ProcessAttendanceEvent`; sends result over WebSocket (`LogAttendanceAsync`) with 1s spacing between filtered posts, 7s spacing for unsynced-recovery posts.
4. On repeated download failure → `_earlyRetryRequested = true` → next cycle in 15 minutes instead of waiting the full 6 hours.

## 7. Offline-first enrollment/renewal (`Classes/OfflineEnrollmentSubscriptionService.cs`, 780 lines)

The core feature letting front-desk staff keep working with no internet connection. Runs three independent timers:

1. **Catalog pull** — every 60 min (30s retry delay on error): `GET /api/sync/catalog` → upserts into local `Packages`/`Trainers` tables; advances a watermark setting only after a full successful batch.
2. **Enrollment/renewal push** — `EnrollmentSyncIntervalMs` (code default **60s**; written docs elsewhere say "10 minutes" — code wins):
   - Pass 1: for each `OfflineMembers` row with `ServerSync=false`, upload photo first (`POST /api/sync/member-picture`), then the member record (`POST /api/sync/enrollments`) — skipped entirely if photo upload fails, so a member is never pushed without its already-committed picture.
   - Pass 2: for already-synced offline members, `TryCleanupOfflineMemberAsync` deletes the member from every device it hasn't been confirmed-deleted from yet (tracked per-device in `DeviceCleanupStatus`); only once **all** devices confirm does it purge local `OfflineMemberLog` rows, the cached photo, and the `OfflineMembers` row.
   - Also drives `SyncPendingRenewalsAsync()` for queued `OfflineSubscriptions` rows — on success writes `package_expiry` back onto the member and deletes the queued row.
   - `SyncPendingRenewalsWithQuickRetryAsync()` / `PushOfflineEntriesToDevicesAsync()` — on-demand "push now, retry once more in 3s" helpers callable right after a Save in the UI, rather than waiting for the next timer tick.
3. **Device push** — every 60s (quick retry 3s, max 5 attempts): if any unsynced `OfflineMemberLog` entry remains under the attempt cap, calls `DeviceFunctions.ProcessAllOfflineEntries()`.

`ConnectionStatusChecker` gates the enrollment push on actual internet availability before attempting uploads.

### Offline enrollment UI
- `AddOfflineMemberForm.cs` (built entirely in code, no Designer file): captures a live photo via AForge webcam feed or file browse, writes a new row to `OfflineMembers` with a **decrementing fake EnrollmentID starting at 999998**, saves the photo to `offlinefaces\{EnrollmentID}.png`.
- `RenewMembershipForm.cs` (also code-built): renews an existing member offline — immediately flips the local member to `Status=active` / clears `PackageExpiry` and enables them on every device already synced to them, then queues the real subscription in `OfflineSubscriptions` for server reconciliation (which later writes back the authoritative `package_expiry`).

## 8. Connectivity health (`Classes/ConnectionStatusChecker.cs`)

Singleton (`Lazy<T>`), runs on its own dedicated STA thread with two `System.Threading.Timer`s:
- **Internet check** every 60s (ping `google.com`, 2s timeout); backs off to 2 min after 3 failures, 5 min beyond that; resets to 60s on success.
- **Subscription check** every 15s: pings the WebSocket (`WebSocketManager.ConnectionStatusAsync()`, rate-limited to ≥10s apart); connection considered healthy only if a pong arrived in the last 30s **and** `IsSubscribed` is true. If stale → `TriggerDashboardReconnection()` (throttled to once/60s, capped at 5 consecutive attempts then a 5-minute cooldown), which finds the live `Dashboard` via `Application.OpenForms` and invokes `TriggerWebSocketReconnection()` on the UI thread.

## 9. Data layer

- **`Classes/DatabaseHelper.cs`** — a single hand-written 10,000+ line SQLite data-access class. This is the actual persistence layer, covering settings (key/value: `GymId`, `GWTToken`, `AttendanceDebounceSeconds`, `CatalogSync` watermark, `DebugMode`, …), members, devices, attendance logs, packages/trainers cache, and all the `OfflineMembers`/`OfflineMemberLog`/`OfflineSubscriptions` tables described above.
- **Entity Framework 6.5.1 / `EntityFramework.SqlServer` are referenced in the `.csproj` and `App.config`**, and `App.config` even has a SQL Server connection string (`Server=localhost\SQLEXPRESS;Database=SmartAttendance;...`), but **no EF `DbContext` exists in the codebase** — this appears to be legacy/vestigial from an earlier design. Treat SQLite via `DatabaseHelper` as the real data layer; don't assume SQL Server is in play.
- `App.config` appSettings also carry `CrashLogPath` / `MiddlewareLogPath` pointing at a differently-named folder (`D:\SmartAttendanceMiddleware\...`) than the current repo (`EUSmartAttendanceMiddlewareX`) — stale dev-machine paths, not load-bearing.

## 10. UI map

| Form | Role |
|---|---|
| `Login` / `UserSetup` | auth / first-run admin creation |
| `Dashboard` (2,310 lines) | composition root — starts/stops all services, shows status, member/device summaries |
| `DeviceManager`, `AddNewDevice` | configure devices (IP/port/machine no/password/action) |
| `AddNewUser`, `EditUser`, `FingerManagement`, `Fingerprint(managment)` | member/biometric enrollment UI |
| `FacePopupForm` | bottom-right popup on attendance event, 3s default display |
| `GateAccessForm`, `CropForm` | door-open flow, photo cropping |
| `MemberDeviceStatusDialog`, `SimulatorForm` | diagnostics / device simulation |
| `WebsocketServer` | hosts the local device-facing WS server (also a visible form) |
| `AddOfflineMemberForm`, `RenewMembershipForm` | offline enrollment/renewal (code-built, see §7) |

Supporting custom controls: `Controls/CircularLoader.cs`, `RectangularLoader.cs`, `DataPairPanel.cs`; `UI/LoaderPopupForm.cs`, `LogForm.cs`, `ToastNotificationForm.cs`.

## 11. Git history narrative

```
10f3f2a Add project files
b78fdee New Websocket and http handling        (sync layer added)
b2f41e2 Staff Module Added
6db175c / 7473beb Major Fixes / Final Release  (stabilization)
d8eedb7 CacheManager
32a682c / 6921f55 New Webserver / Websocket Server Implementation  (local port-7788 server added)
9bacd86 new stable version 3.1.0
82aa6b4 / 0c269ff / 83abc36 / c6c4040 crash/bug fix passes
eef1881 Add automatic Windows Firewall rule creation for port 7788
313b93a Rename firewall rule to FitnessMarvel, allow all remote addresses
f3d2aa4 New Reason of Failure added            (latest)
```

## 12. Known issues / things to fix before calling this production-hardened

1. **TLS cert validation disabled** on the cloud WebSocket connection (`WebSocketManager.ConnectAsync`) — accepts any certificate.
2. **Firewall rule is `-RemoteAddress Any`**, not restricted to the LAN subnet — port 7788 is reachable from anywhere that can route to the machine.
3. **Shipped build points at staging** (`ws-stg.fitnessmarvel.com`) in both `HttpHandler.cs` and `WebSocketManager.cs`; the production host is present only as a comment — a deploy step must swap these or it ships talking to staging.
4. **`EnrollmentSyncIntervalMs` is 60s in code** but documented elsewhere as "10 minutes" — if touching this service, go by the constant, not the prose.
5. **EF6 / SQL Server config is vestigial** — don't be misled into thinking there's a SQL Server backing store; it's SQLite via `DatabaseHelper`.
6. **`ServiceManager` method signatures changed** (now parameterless async) from what older docs describe.

## 13. Where to look for what

| Need to understand... | Start at |
|---|---|
| App boot sequence | `Program.cs` |
| Main UI / service orchestration | `Dashboard.cs` |
| Talking to a physical device | `Classes/DeviceService.cs`, `Classes/DeviceFunctions.cs` |
| Local device-facing WS server (port 7788) | `WebsocketServer.cs`, `Classes/WebsocketServerThreadManager.cs` |
| Cloud real-time connection | `Classes/WebSocketManager.cs` |
| Cloud REST calls | `Classes/HttpHandler.cs` |
| Offline enroll/renew UI | `AddOfflineMemberForm.cs`, `RenewMembershipForm.cs` |
| Offline sync engine | `Classes/OfflineEnrollmentSubscriptionService.cs` |
| Attendance log pipeline | `Classes/AttendanceLogSyncService.cs`, `Classes/StaffAttendanceLogSyncService.cs`, `Classes/AttendenceServerFunctions.cs` |
| Service start/stop | `Classes/ServiceManager.cs` |
| Connectivity/health monitoring | `Classes/ConnectionStatusChecker.cs` |
| All local data access | `Classes/DatabaseHelper.cs` (+ `DatabaseHelper.Incremental.cs`) |
