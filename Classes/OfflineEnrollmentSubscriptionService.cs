using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace smart_attendance_middleware.Classes
{
    /// <summary>
    /// Periodically pulls the packages/trainers catalog (/api/sync/catalog) so staff can
    /// enroll members / assign trainers while offline, and upserts the results locally.
    /// Also periodically pushes pending offline-enrolled members up to the server
    /// (/api/sync/enrollments), and on success removes them from the biometric devices
    /// and from the local OfflineMembers/OfflineMemberLog tables.
    /// </summary>
    public class OfflineEnrollmentSubscriptionService : IDisposable
    {
        private const string CatalogSyncSettingKey = "CatalogSync";

        private readonly int _syncInterval = 60 * 60 * 1000;   // 60 minutes
        private readonly int _retryDelay = 30 * 1000;          // 30 seconds on unexpected error

        // How often the pending-offline-enrollments push runs. Public so it can be tuned
        // (e.g. from Setup Management) without touching the loop logic itself.
        public int EnrollmentSyncIntervalMs = 1 * 60 * 1000; // 10 minutes — change as needed

        private readonly DatabaseHelper _dbHelper;
        private readonly HttpHandler _httpHandler;

        private CancellationTokenSource _cts;
        private Task _syncTask;
        private volatile bool _loopRunning = false;

        private System.Threading.Timer _enrollmentSyncTimer;
        private volatile bool _enrollmentSyncRunning = false;

        private System.Threading.Timer _devicePushTimer;
        private readonly SemaphoreSlim _devicePushGate = new SemaphoreSlim(1, 1);
        private readonly SemaphoreSlim _renewalGate = new SemaphoreSlim(1, 1);
        private const int DevicePushIntervalMs = 60 * 1000;
        private const int DevicePushQuickRetryMs = 3000;
        private const int DevicePushMaxAttempts = 5;

        private readonly TaskCompletionSource<bool> _startedTcs =
            new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        public Task Started => _startedTcs.Task;

        public OfflineEnrollmentSubscriptionService()
        {
            _dbHelper = new DatabaseHelper();
            _httpHandler = new HttpHandler(_dbHelper);
        }

        public void Start()
        {
            if (_syncTask != null && !_syncTask.IsCompleted)
                return;

            if (_loopRunning)
                return;

            _cts = new CancellationTokenSource();
            LogHelper.Log("Offline enrollment/subscription catalog sync service started");

            _syncTask = Task.Run(async () =>
            {
                try
                {
                    _loopRunning = true;
                    await RunSyncLoopAsync(_cts.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"❌ Unhandled exception in catalog sync task: {ex.Message}");
                }
                finally
                {
                    _loopRunning = false;
                }
            }, _cts.Token);

            _enrollmentSyncTimer = new System.Threading.Timer(
                _ => _ = SyncPendingOfflineEnrollmentsAsync(),
                null,
                EnrollmentSyncIntervalMs,
                EnrollmentSyncIntervalMs);
            LogHelper.Log($"Offline enrollment push timer started (every {EnrollmentSyncIntervalMs / 1000}s)");

            _devicePushTimer = new System.Threading.Timer(
                _ => _ = CheckPendingOfflineDevicePushAsync(),
                null,
                DevicePushIntervalMs,
                DevicePushIntervalMs);
        }

        public void Stop()
        {
            if (_cts != null)
            {
                LogHelper.Log("Offline enrollment/subscription catalog sync service stopping");
                _cts.Cancel();
            }

            _enrollmentSyncTimer?.Dispose();
            _enrollmentSyncTimer = null;

            _devicePushTimer?.Dispose();
            _devicePushTimer = null;
        }

        /// <summary>
        /// Pushes unsynced OfflineMemberLog entries to the devices. If anything is still pending
        /// afterwards, retries exactly once more after 3 seconds.
        /// </summary>
        public async Task PushOfflineEntriesToDevicesAsync()
        {
            await RunOfflineDevicePushOnceAsync(waitForGate: true);

            if (await HasRetryableOfflineDeviceEntriesAsync())
            {
                LogHelper.Log("[OfflineDevicePush] Entries still pending - retrying in 3 seconds");
                await Task.Delay(DevicePushQuickRetryMs);
                await RunOfflineDevicePushOnceAsync(waitForGate: true);
            }
        }

        // Runs every minute: retries only when there is a pending offline log below the attempt limit.
        private async Task CheckPendingOfflineDevicePushAsync()
        {
            try
            {
                if (!await HasRetryableOfflineDeviceEntriesAsync()) return;
                LogHelper.Log("[OfflineDevicePush] Pending offline entries found - retrying device push");
                await RunOfflineDevicePushOnceAsync(waitForGate: false);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[OfflineDevicePush] Periodic check failed: {ex.Message}");
            }
        }

        private async Task RunOfflineDevicePushOnceAsync(bool waitForGate)
        {
            if (waitForGate) await _devicePushGate.WaitAsync();
            else if (!await _devicePushGate.WaitAsync(0)) return;

            try
            {
                await new DeviceFunctions().ProcessAllOfflineEntries();
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[OfflineDevicePush] ProcessAllOfflineEntries failed: {ex.Message}");
            }
            finally
            {
                _devicePushGate.Release();
            }
        }

        // True if at least one unsynced offline log entry has fewer than 5 failed attempts.
        private async Task<bool> HasRetryableOfflineDeviceEntriesAsync()
        {
            var entries = await _dbHelper.GetUnsyncedOfflineLogEntriesAsync();
            if (entries == null) return false;

            foreach (var entry in entries)
            {
                dynamic log = _dbHelper.GetLatestOfflineLogForMember(entry.ClientId);
                int attempts = 0;
                if (log != null)
                {
                    int? value = (int?)log.AttemptCount;
                    attempts = value ?? 0;
                }
                if (attempts < DevicePushMaxAttempts) return true;
            }
            return false;
        }

        private async Task RunSyncLoopAsync(CancellationToken token)
        {
            try
            {
                await SyncCatalogAsync(token).ConfigureAwait(false);

                if (!_startedTcs.Task.IsCompleted)
                    _startedTcs.TrySetResult(true);

                while (!token.IsCancellationRequested)
                {
                    try
                    {
                        await Task.Delay(_syncInterval, token).ConfigureAwait(false);
                        await SyncCatalogAsync(token).ConfigureAwait(false);
                    }
                    catch (TaskCanceledException)
                    {
                        break;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"Catalog sync error: {ex.Message}");
                        try
                        {
                            await Task.Delay(_retryDelay, token).ConfigureAwait(false);
                        }
                        catch (TaskCanceledException) { break; }
                    }
                }
            }
            catch (TaskCanceledException)
            {
                // normal cancellation
            }
            finally
            {
                LogHelper.Log("Offline enrollment/subscription catalog sync service stopped");
                if (!_startedTcs.Task.IsCompleted)
                    _startedTcs.TrySetResult(true);
            }
        }

        public async Task SyncCatalogAsync(CancellationToken token)
        {
            try
            {
                var (packages, trainers, syncedAt) = await _httpHandler.GetCatalogAsync(token).ConfigureAwait(false);

                if (packages == null || trainers == null)
                {
                    LogHelper.Log("❌ Catalog sync failed: no response from server.");
                    return;
                }

                int packagesUpserted = 0;
                foreach (JObject package in packages)
                {
                    if (token.IsCancellationRequested) break;
                    try
                    {
                        _dbHelper.UpsertPackage(package);
                        packagesUpserted++;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"❌ Failed to upsert package {package["id"]}: {ex.Message}");
                    }
                }

                int trainersUpserted = 0;
                foreach (JObject trainer in trainers)
                {
                    if (token.IsCancellationRequested) break;
                    try
                    {
                        _dbHelper.UpsertTrainer(trainer);
                        trainersUpserted++;
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"❌ Failed to upsert trainer {trainer["id"]}: {ex.Message}");
                    }
                }

                // Only advance the watermark once the whole batch was applied successfully,
                // so a partial/failed run doesn't skip rows on the next incremental pull.
                if (!string.IsNullOrWhiteSpace(syncedAt))
                {
                    _dbHelper.SaveSetting(CatalogSyncSettingKey, syncedAt);
                }

                LogHelper.Log($"✅ Catalog sync complete: {packagesUpserted} packages, {trainersUpserted} trainers upserted. SyncedAt={syncedAt}");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"❌ Catalog sync error: {ex.Message}");
            }
        }

        /// <summary>
        /// Pushes every offline-enrolled member (OfflineMembers.ServerSync = false) up to the
        /// server one at a time. On a successful upload for a member: deletes that member from
        /// every device it was pushed to (per its OfflineMemberLog rows), then deletes its
        /// OfflineMemberLog rows, then its saved face photo, then the OfflineMembers row itself.
        /// </summary>
        public async Task SyncPendingOfflineEnrollmentsAsync()
        {
            if (_enrollmentSyncRunning) return; // reentrancy guard — previous run still in flight
            _enrollmentSyncRunning = true;

            try
            {
                if (!ConnectionStatusChecker.Instance.IsInternetConnected)
                {
                    LogHelper.Log("[OfflineEnrollmentSync] Skipping — no internet connection.");
                    return;
                }

                List<OfflineMember> allOfflineMembers;
                try
                {
                    allOfflineMembers = _dbHelper.GetOfflineMembers() ?? new List<OfflineMember>();
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[OfflineEnrollmentSync] Failed to read offline members: {ex.Message}");
                    allOfflineMembers = new List<OfflineMember>();
                }

                // Pass 1: upload members never accepted by the server yet.
                var pendingUpload = allOfflineMembers.Where(m => !m.ServerSync).ToList();
                if (pendingUpload.Count > 0)
                {
                    LogHelper.Log($"[OfflineEnrollmentSync] {pendingUpload.Count} offline member(s) pending server sync.");

                    foreach (var member in pendingUpload)
                    {
                        try
                        {
                            await SyncSingleOfflineMemberAsync(member).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"[OfflineEnrollmentSync] Error syncing {member.ClientId} ({member.Name}): {ex.Message}");
                        }
                    }
                }

                // Pass 2: already accepted by the server — never re-uploaded, just retry
                // whatever devices still haven't confirmed deletion. Re-read so pass 1's
                // just-synced members are included here too.
                List<OfflineMember> pendingCleanup;
                try
                {
                    pendingCleanup = (_dbHelper.GetOfflineMembers() ?? new List<OfflineMember>())
                        .Where(m => m.ServerSync)
                        .ToList();
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[OfflineEnrollmentSync] Failed to read server-synced offline members: {ex.Message}");
                    pendingCleanup = new List<OfflineMember>();
                }

                if (pendingCleanup.Count > 0)
                {
                    LogHelper.Log($"[OfflineEnrollmentSync] {pendingCleanup.Count} offline member(s) pending device cleanup.");

                    foreach (var member in pendingCleanup)
                    {
                        try
                        {
                            await TryCleanupOfflineMemberAsync(member).ConfigureAwait(false);
                        }
                        catch (Exception ex)
                        {
                            LogHelper.Log($"[OfflineEnrollmentSync] Error cleaning up {member.ClientId} ({member.Name}): {ex.Message}");
                        }
                    }
                }

                await SyncPendingRenewalsAsync().ConfigureAwait(false);
            }
            finally
            {
                _enrollmentSyncRunning = false;
            }
        }

        /// <summary>
        /// Pushes every queued membership renewal (dbo.OfflineSubscriptions) up to
        /// /api/sync/enrollments one at a time. On success: writes the returned package_expiry
        /// back onto the member and deletes the queued row; on failure the row is left for the
        /// next timer tick to retry.
        /// </summary>
        private async Task SyncPendingRenewalsAsync()
        {
            await _renewalGate.WaitAsync().ConfigureAwait(false);
            try
            {
                List<OfflineSubscription> pendingRenewals;
                try
                {
                    pendingRenewals = _dbHelper.GetPendingOfflineSubscriptions() ?? new List<OfflineSubscription>();
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[OfflineEnrollmentSync] Failed to read pending renewals: {ex.Message}");
                    return;
                }

                if (pendingRenewals.Count == 0) return;

                LogHelper.Log($"[OfflineEnrollmentSync] {pendingRenewals.Count} renewal(s) pending server sync.");

                foreach (var renewal in pendingRenewals)
                {
                    try
                    {
                        await SyncSingleRenewalAsync(renewal).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"[OfflineEnrollmentSync] Error syncing renewal Id={renewal.Id} (MemberId={renewal.MemberId}): {ex.Message}");
                    }
                }
            }
            finally
            {
                _renewalGate.Release();
            }
        }

        /// <summary>
        /// Pushes queued renewals right away; if any are still pending afterwards, retries exactly
        /// once more after 3 seconds. The 1-minute enrollment timer keeps retrying after that.
        /// </summary>
        public async Task SyncPendingRenewalsWithQuickRetryAsync()
        {
            if (!ConnectionStatusChecker.Instance.IsInternetConnected)
            {
                LogHelper.Log("[OfflineEnrollmentSync] Renewal quick sync skipped — no internet; the 1-minute timer will retry.");
                return;
            }

            await SyncPendingRenewalsAsync().ConfigureAwait(false);

            if (HasPendingRenewals())
            {
                LogHelper.Log("[OfflineEnrollmentSync] Renewal(s) still pending — retrying in 3 seconds");
                await Task.Delay(3000).ConfigureAwait(false);
                await SyncPendingRenewalsAsync().ConfigureAwait(false);
            }
        }

        // True if any offline member log can still be pushed to devices, or any renewal awaits upload.
        public async Task<bool> HasPendingOfflineRetriesAsync()
        {
            if (HasPendingRenewals()) return true;
            return await HasRetryableOfflineDeviceEntriesAsync().ConfigureAwait(false);
        }

        private bool HasPendingRenewals()
        {
            try
            {
                var pending = _dbHelper.GetPendingOfflineSubscriptions();
                return pending != null && pending.Count > 0;
            }
            catch
            {
                return false;
            }
        }

        private async Task SyncSingleRenewalAsync(OfflineSubscription renewal)
        {
            JObject subscription;
            try
            {
                subscription = !string.IsNullOrWhiteSpace(renewal.SubscriptionJson)
                    ? JObject.Parse(renewal.SubscriptionJson)
                    : null;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Invalid renewal subscription JSON for Id={renewal.Id}: {ex.Message}");
                return;
            }

            if (subscription == null)
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Renewal Id={renewal.Id} has no subscription payload — dropping.");
                _dbHelper.DeleteOfflineSubscription(renewal.Id);
                return;
            }

            var record = new JObject
            {
                ["client_id"] = renewal.ClientId,
                ["member_id"] = renewal.MemberId,
                ["subscription"] = subscription
            };

            var response = await _httpHandler.PostEnrollmentsAsync(new JArray(record)).ConfigureAwait(false);
            if (response == null)
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Renewal upload failed for MemberId={renewal.MemberId} — no response. Will retry.");
                return;
            }

            // Same dual-shape handling as new-member enrollments: batch-wrapped or a flat record.
            JObject resultRecord = null;
            if (response["records"] is JArray recordsArray)
            {
                int errors = response["errors"]?.ToObject<int>() ?? 0;
                resultRecord = recordsArray.OfType<JObject>()
                    .FirstOrDefault(r => (int?)r["member_id"] == renewal.MemberId);
                if (errors > 0 || resultRecord == null)
                {
                    LogHelper.Log($"[OfflineEnrollmentSync] Server reported failure for renewal MemberId={renewal.MemberId}. Response: {response.ToString(Formatting.None)}");
                    return;
                }
            }
            else if ((int?)response["member_id"] == renewal.MemberId)
            {
                resultRecord = response;
            }
            else
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Unexpected renewal response shape for MemberId={renewal.MemberId}. Response: {response.ToString(Formatting.None)}");
                return;
            }

            string packageExpiry = resultRecord["package_expiry"]?.ToString();
            LogHelper.Log($"[OfflineEnrollmentSync] Renewal synced for MemberId={renewal.MemberId} -> SubscriptionId={resultRecord["subscription_id"]}, PackageExpiry={packageExpiry}");

            try
            {
                _dbHelper.UpdateMemberPackageExpiry(renewal.MemberId, packageExpiry);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Failed to write package_expiry for MemberId={renewal.MemberId}: {ex.Message}");
            }

            try
            {
                _dbHelper.DeleteOfflineSubscription(renewal.Id);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Failed to delete synced renewal Id={renewal.Id}: {ex.Message}");
            }
        }

        private async Task SyncSingleOfflineMemberAsync(OfflineMember member)
        {
            JObject subscription = null;
            try
            {
                subscription = !string.IsNullOrWhiteSpace(member.SubscriptionJson)
                    ? JObject.Parse(member.SubscriptionJson)
                    : null;
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Invalid subscription JSON for {member.ClientId}: {ex.Message}");
            }

            // Upload the photo first (if any) and abort this member entirely on failure — do NOT
            // send the enrollment without it. The next timer tick will retry from scratch since
            // nothing local gets deleted/marked when we return early here.
            string uploadedPictureUrl = null;
            if (!string.IsNullOrWhiteSpace(member.Picture))
            {
                uploadedPictureUrl = await UploadMemberPictureAsync(member).ConfigureAwait(false);
                if (uploadedPictureUrl == null)
                {
                    LogHelper.Log($"[OfflineEnrollmentSync] Picture upload failed for {member.Name} ({member.ClientId}) — skipping this cycle, will retry.");
                    return;
                }
            }

            bool hasRfid = !string.IsNullOrWhiteSpace(member.RFID);
            var rfidObj = new JObject { ["enabled"] = hasRfid };
            if (hasRfid) rfidObj["identifier"] = member.RFID; // schema wants a string, not null — omit when disabled

            var authMethods = new JObject
            {
                ["rfid"] = rfidObj,
                ["face"] = uploadedPictureUrl != null,
                ["fingerprint"] = member.Fingerprint
            };

            var memberObj = new JObject
            {
                ["first_name"] = member.FirstName,
                ["last_name"] = member.LastName,
                ["phone"] = member.Phone,
                ["authentication_methods"] = authMethods
            };
            if (!string.IsNullOrWhiteSpace(member.Gender)) memberObj["gender"] = member.Gender.ToLowerInvariant();
            if (!string.IsNullOrWhiteSpace(member.Email)) memberObj["email"] = member.Email.Trim();
            if (!string.IsNullOrWhiteSpace(member.Whatsapp)) memberObj["whatsapp"] = member.Whatsapp.Trim();
            if (uploadedPictureUrl != null) memberObj["picture"] = uploadedPictureUrl;

            var record = new JObject
            {
                ["client_id"] = member.ClientId,
                ["member"] = memberObj,
                ["subscription"] = subscription
            };

            var response = await _httpHandler.PostEnrollmentsAsync(new JArray(record)).ConfigureAwait(false);
            if (response == null)
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Upload failed for {member.Name} ({member.ClientId}) — no response.");
                return;
            }

            // Server may reply either batch-wrapped ({"errors":N,"records":[...]}) or as a single
            // flat record object — handle both shapes.
            JObject resultRecord = null;
            if (response["records"] is JArray recordsArray)
            {
                int errors = response["errors"]?.ToObject<int>() ?? 0;
                resultRecord = recordsArray.OfType<JObject>().FirstOrDefault(r => (string)r["client_id"] == member.ClientId);
                if (errors > 0 || resultRecord == null)
                {
                    LogHelper.Log($"[OfflineEnrollmentSync] Server reported failure for {member.Name} ({member.ClientId}). Response: {response.ToString(Formatting.None)}");
                    return;
                }
            }
            else if ((string)response["client_id"] == member.ClientId)
            {
                resultRecord = response;
            }
            else
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Unexpected enrollment response shape for {member.Name} ({member.ClientId}). Response: {response.ToString(Formatting.None)}");
                return;
            }

            LogHelper.Log($"[OfflineEnrollmentSync] {member.Name} synced -> MemberId={resultRecord["member_id"]}, EnrollmentId={resultRecord["enrollment_id"]}");

            // Server has accepted this member — never upload it again, no matter what happens
            // next. Seed per-device cleanup status from the devices it was actually pushed to,
            // then try cleaning up right away (the timer's cleanup pass will retry any leftovers).
            List<OfflineLogEntry> logs;
            try
            {
                logs = _dbHelper.GetOfflineMemberLogs(member.ClientId);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Failed to load device logs for {member.ClientId}: {ex.Message}");
                logs = new List<OfflineLogEntry>();
            }

            _dbHelper.MarkOfflineMemberServerSynced(member.ClientId, logs.Select(l => l.DeviceID));

            member.ServerSync = true;
            member.DeviceCleanupStatus = logs.Select(l => l.DeviceID).Distinct().ToDictionary(id => id, id => false);
            await TryCleanupOfflineMemberAsync(member).ConfigureAwait(false);
        }

        /// <summary>
        /// For a member already accepted by the server (ServerSync = true): retries deleteuser
        /// only for devices not yet marked cleaned in DeviceCleanupStatus. Once every device is
        /// confirmed clean, deletes the OfflineMemberLog rows, the saved photo, and finally the
        /// OfflineMembers row. If any device still fails, everything is left in place for the
        /// next timer tick — the member is never re-uploaded.
        /// </summary>
        private async Task TryCleanupOfflineMemberAsync(OfflineMember member)
        {
            var status = member.DeviceCleanupStatus ?? new Dictionary<int, bool>();
            var pendingDeviceIds = status.Where(kv => !kv.Value).Select(kv => kv.Key).ToList();

            if (pendingDeviceIds.Count > 0)
            {
                List<OfflineLogEntry> logs;
                try
                {
                    logs = _dbHelper.GetOfflineMemberLogs(member.ClientId);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[OfflineEnrollmentSync] Failed to load device logs for {member.ClientId}: {ex.Message}");
                    return;
                }

                var devices = _dbHelper.LoadDevices();

                foreach (var deviceId in pendingDeviceIds)
                {
                    var log = logs.FirstOrDefault(l => l.DeviceID == deviceId);
                    var device = devices.FirstOrDefault(d => d.DeviceId == deviceId);
                    if (log == null || device == null) continue;

                    try
                    {
                        // backupnum 12 = delete all templates (fingerprint/face/card/password) for this enrollid
                        var deleteParams = new { enrollid = log.EnrollmentID, backupnum = 12 };
                        bool ok = await WebsocketServer.ExecuteServiceCommand(
                            device.DeviceSerial, "deleteuser", JsonConvert.SerializeObject(deleteParams)).ConfigureAwait(false);

                        LogHelper.Log($"[OfflineEnrollmentSync] Delete-from-device {(ok ? "OK" : "FAILED")} for {member.Name} on DeviceId={device.DeviceId} ({device.DeviceSerial})");

                        if (ok)
                        {
                            _dbHelper.MarkOfflineMemberDeviceCleaned(member.ClientId, deviceId);
                            status[deviceId] = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        LogHelper.Log($"[OfflineEnrollmentSync] Error deleting {member.Name} from DeviceId={deviceId}: {ex.Message}");
                    }
                }
            }

            if (status.Count > 0 && status.Values.Any(v => !v))
            {
                int remaining = status.Count(kv => !kv.Value);
                LogHelper.Log($"[OfflineEnrollmentSync] {member.Name} ({member.ClientId}) still has {remaining} device(s) pending cleanup — will retry next cycle.");
                return; // leave everything local in place; nothing else gets deleted yet
            }

            // Every device confirmed clean (or there were none to begin with) — safe to remove locally.
            try
            {
                _dbHelper.DeleteOfflineMemberLogs(member.ClientId);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Failed to delete offline logs for {member.ClientId}: {ex.Message}");
            }

            if (!string.IsNullOrWhiteSpace(member.Picture))
            {
                try
                {
                    if (File.Exists(member.Picture)) File.Delete(member.Picture);
                }
                catch (Exception ex)
                {
                    LogHelper.Log($"[OfflineEnrollmentSync] Failed to delete photo '{member.Picture}': {ex.Message}");
                }
            }

            try
            {
                _dbHelper.DeleteOfflineMember(member.ClientId);
                LogHelper.Log($"[OfflineEnrollmentSync] Removed local offline record for {member.Name} ({member.ClientId})");
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Failed to delete offline member row {member.ClientId}: {ex.Message}");
            }
        }

        // Reads the locally-saved face photo, base64-encodes it as a data URL, and uploads it via
        // /api/sync/member-picture. Returns the CDN url on success, or null on any failure.
        private async Task<string> UploadMemberPictureAsync(OfflineMember member)
        {
            try
            {
                if (!File.Exists(member.Picture))
                {
                    LogHelper.Log($"[OfflineEnrollmentSync] Picture file missing for {member.ClientId}: {member.Picture}");
                    return null;
                }

                byte[] bytes = File.ReadAllBytes(member.Picture);
                string ext = Path.GetExtension(member.Picture).TrimStart('.').ToLowerInvariant();
                string mime = ext == "png" ? "image/png" : "image/jpeg";
                string dataUrl = $"data:{mime};base64,{Convert.ToBase64String(bytes)}";

                return await _httpHandler.PostMemberPictureAsync(dataUrl, member.FirstName, member.LastName).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                LogHelper.Log($"[OfflineEnrollmentSync] Picture upload error for {member.ClientId}: {ex.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            Stop();
            _cts?.Dispose();
            _enrollmentSyncTimer?.Dispose();
            _httpHandler?.Dispose();
            LogHelper.Log("Offline enrollment/subscription catalog sync service disposed");
        }
    }
}
