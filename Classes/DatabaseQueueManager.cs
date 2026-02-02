using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace smart_attendance_middleware.Classes
{
    /// <summary>
    /// Manages a semaphore-like write lock for DB writes. Only one writer at a time.
    /// Waiters are queued and processed sequentially based on priority.
    /// Queue ordering is persisted to a JSON file to survive crashes.
    /// </summary>
    public static class DatabaseQueueManager
    {
        private sealed class Waiter
        {
            public Guid TicketId { get; }
            public DbTaskPriority Priority { get; }
            public TaskCompletionSource<bool> Tcs { get; }
            public string TaskName { get; }

            public Waiter(Guid ticketId, DbTaskPriority priority, string taskName)
            {
                TicketId = ticketId;
                Priority = priority;
                TaskName = taskName;
                Tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        private sealed class PersistentQueueItem
        {
            public Guid Id { get; set; }
            public string TaskName { get; set; }
            public int Priority { get; set; } // DbTaskPriority numeric value
            public long EnqueuedTicks { get; set; } // DateTime.UtcNow.Ticks
            public int ProcessId { get; set; } // Owner process at enqueue time
        }

        public sealed class DbWriteLockReleaser : IDisposable
        {
            private readonly string _taskName;
            private int _disposed; // 0 = not disposed, 1 = disposed

            internal DbWriteLockReleaser(string taskName)
            {
                _taskName = taskName;
            }

            public void Dispose()
            {
                if (Interlocked.Exchange(ref _disposed, 1) != 0)
                    return;

                ReleaseDbWriteLock(_taskName);
            }
        }

        private static readonly object _queueLock = new object();

        // JSON-backed queue state (order matters).
        private static readonly List<PersistentQueueItem> _persistentQueue = new List<PersistentQueueItem>();
        private static readonly JavaScriptSerializer _json = new JavaScriptSerializer();
        private static readonly string _queueFilePath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "db_write_queue.json");

        // In-memory waiters keyed by their ticket.
        private static readonly Dictionary<Guid, Waiter> _waitersById = new Dictionary<Guid, Waiter>();

        // Lock ownership tracking.
        private static bool _isHeld;
        private static string _currentHolderName;
        private static Guid _currentHolderTicket;
        private static DateTime _heldSinceUtc;

        // Watchdog to prevent stuck lock holders.
        private static readonly TimeSpan MaxHoldDuration = TimeSpan.FromSeconds(15);
        private static readonly Timer _watchdogTimer = new Timer(_ =>
        {
            try
            {
                lock (_queueLock)
                {
                    if (_isHeld && _heldSinceUtc != default && (DateTime.UtcNow - _heldSinceUtc) > MaxHoldDuration)
                    {
                        LogHelper.Log($"⛑️ Watchdog: Forcing DB write lock release. Holder='{_currentHolderName}', HeldForMs={(DateTime.UtcNow - _heldSinceUtc).TotalMilliseconds:0}");

                        var prevTicket = _currentHolderTicket;

                        // Remove head if it matches current holder ticket; else remove defensively.
                        if (_persistentQueue.Count > 0 && _persistentQueue[0].Id == prevTicket)
                        {
                            _persistentQueue.RemoveAt(0);
                            SaveQueueToDisk_NoThrow();
                        }
                        else
                        {
                            RemoveTicketFromPersistentQueue(prevTicket);
                            SaveQueueToDisk_NoThrow();
                        }

                        // Identify next waiter for the new head (if any).
                        Waiter nextWaiter = null;
                        if (_persistentQueue.Count > 0)
                        {
                            var nextId = _persistentQueue[0].Id;
                            _waitersById.TryGetValue(nextId, out nextWaiter);
                        }

                        // Clear ownership; mark lock as free until the next waiter acquires it.
                        _isHeld = false;
                        _currentHolderName = null;
                        _currentHolderTicket = Guid.Empty;
                        _heldSinceUtc = default;

                        // Wake the next waiter outside of the lock to avoid reentrancy deadlocks.
                        if (nextWaiter != null)
                        {
                            _waitersById.Remove(nextWaiter.TicketId);
                            Task.Run(() => nextWaiter.Tcs.SetResult(true));
                        }
                    }
                }
            }
            catch
            {
                // Swallow watchdog issues to avoid crashing.
            }
        }, null, TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(5));

        // Startup: load queue and purge orphans from previous runs.
        static DatabaseQueueManager()
        {
            try
            {
                lock (_queueLock)
                {
                    LoadQueueFromDisk_NoThrow();
                    PurgeOrphanedEntries_NoThrow(); // Remove entries not belonging to this process
                    SaveQueueToDisk_NoThrow();
                }
            }
            catch
            {
                // Swallow any startup issues; the lock will still work in-memory.
            }
        }

        /// <summary>
        /// Acquire the DB write lock. Use in a using/try-finally to guarantee release.
        /// Every request is enqueued to a JSON-backed queue to preserve ordering across crashes.
        /// </summary>
        public static async Task<DbWriteLockReleaser> EnterDbWriteLockAsync(
            string taskName,
            DbTaskPriority priority = DbTaskPriority.Low,
            int timeoutMs = 60000)
        {
            if (string.IsNullOrWhiteSpace(taskName))
                taskName = "UnknownTask";

            var sw = Stopwatch.StartNew();
            Waiter waiter = null;
            Guid myTicket;

            lock (_queueLock)
            {
                // 1) Enqueue to persistent queue (source of truth for order).
                var item = new PersistentQueueItem
                {
                    Id = Guid.NewGuid(),
                    TaskName = taskName,
                    Priority = (int)priority,
                    EnqueuedTicks = DateTime.UtcNow.Ticks,
                    ProcessId = Process.GetCurrentProcess().Id
                };
                myTicket = item.Id;

                InsertSortedByPriorityAndFifo(item);
                SaveQueueToDisk_NoThrow();

                // 2) Fast-path: if lock not held and I'm at head-of-line, acquire immediately.
                if (!_isHeld && IsHeadOfLine(myTicket))
                {
                    _isHeld = true;
                    _currentHolderName = taskName;
                    _currentHolderTicket = myTicket;
                    _heldSinceUtc = DateTime.UtcNow;

                    sw.Stop();
                    LogHelper.Log($"🔒 Acquired DB write lock for {taskName} (Priority: {priority}) immediately. Queue size: {_persistentQueue.Count}");
                    return new DbWriteLockReleaser(taskName);
                }

                // 3) Otherwise, create a waiter and wait until signaled (when this ticket reaches head-of-line).
                LogHelper.Log($"⏳ {taskName} (Priority: {priority}) is waiting for DB write lock. Current holder: {_currentHolderName ?? "None"} (held {(_isHeld ? (DateTime.UtcNow - _heldSinceUtc).TotalMilliseconds : 0):0}ms), " +
                              $"Queue size: {_persistentQueue.Count}");
                waiter = new Waiter(myTicket, priority, taskName);
                _waitersById[myTicket] = waiter;
            }

            // 4) Wait with optional timeout. On timeout, remove from queues.
            Task waitTask;
            lock (_queueLock)
            {
                waitTask = waiter.Tcs.Task;
            }

            if (timeoutMs > 0)
            {
                var completed = await Task.WhenAny(waitTask, Task.Delay(timeoutMs)).ConfigureAwait(false);
                if (completed != waitTask)
                {
                    // Timeout: remove our ticket from persistent queue and waiter map.
                    lock (_queueLock)
                    {
                        _waitersById.Remove(myTicket);
                        RemoveTicketFromPersistentQueue(myTicket);
                        SaveQueueToDisk_NoThrow();
                    }

                    throw new TimeoutException(
                        $"Timed out waiting for DB write lock. Waiter='{taskName}', Priority={priority}, " +
                        $"CurrentHolder='{_currentHolderName}', HeldForMs={(DateTime.UtcNow - _heldSinceUtc).TotalMilliseconds:0}");
                }
            }
            else
            {
                await waitTask.ConfigureAwait(false);
            }

            // 5) We were granted the lock (we should be head-of-line now).
            lock (_queueLock)
            {
                _isHeld = true;
                _currentHolderName = taskName;
                _currentHolderTicket = myTicket;
                _heldSinceUtc = DateTime.UtcNow;
            }

            sw.Stop();
            LogHelper.Log($"🔒 Acquired DB write lock for {taskName} (Priority: {priority}) after {sw.ElapsedMilliseconds}ms.");
            return new DbWriteLockReleaser(taskName);
        }

        /// <summary>
        /// Release the DB write lock. Prefer disposing DbWriteLockReleaser instead of calling this directly.
        /// </summary>
        public static void ReleaseDbWriteLock(string taskName)
        {
            Waiter nextWaiter = null;
            string prevHolder;
            double heldMs;

            lock (_queueLock)
            {
                prevHolder = _currentHolderName;
                heldMs = _isHeld ? (DateTime.UtcNow - _heldSinceUtc).TotalMilliseconds : 0;

                if (!_isHeld)
                {
                    LogHelper.Log($"⚠️ ReleaseDbWriteLock called by {taskName} but lock is not held. Ignoring.");
                    return;
                }

                // Guard: mismatched release
                if (!string.Equals(_currentHolderName, taskName, StringComparison.Ordinal))
                {
                    LogHelper.Log($"⚠️ DB write lock released by '{taskName}', but current holder is '{_currentHolderName}'. Proceeding to avoid hang.");
                }

                // Remove the holder ticket from head of the persistent queue (if present).
                if (_persistentQueue.Count > 0 && _persistentQueue[0].Id == _currentHolderTicket)
                {
                    _persistentQueue.RemoveAt(0);
                    SaveQueueToDisk_NoThrow();
                }
                else
                {
                    // If not at head, try to find and remove it (defensive).
                    RemoveTicketFromPersistentQueue(_currentHolderTicket);
                    SaveQueueToDisk_NoThrow();
                }

                // Determine next head-of-line and whether we have a waiter for it.
                if (_persistentQueue.Count > 0)
                {
                    var nextId = _persistentQueue[0].Id;
                    Waiter w;
                    if (_waitersById.TryGetValue(nextId, out w))
                    {
                        nextWaiter = w;
                        // Do not mark the lock held here. Let the awakened waiter set ownership upon acquire.
                        _isHeld = false;
                        _currentHolderName = null;
                        _currentHolderTicket = Guid.Empty;
                        _heldSinceUtc = default;
                    }
                    else
                    {
                        // No waiter for new head (e.g., orphan or future request). Mark lock as free.
                        _isHeld = false;
                        _currentHolderName = null;
                        _currentHolderTicket = Guid.Empty;
                        _heldSinceUtc = default;
                    }
                }
                else
                {
                    // Queue empty: free the lock.
                    _isHeld = false;
                    _currentHolderName = null;
                    _currentHolderTicket = Guid.Empty;
                    _heldSinceUtc = default;
                }
            }

            LogHelper.Log($"🔓 Released DB write lock for {taskName}. Previous holder: {prevHolder}, held {heldMs:0}ms.");

            // Wake the next waiter (if any) outside lock to avoid reentrancy issues.
            if (nextWaiter != null)
            {
                lock (_queueLock)
                {
                    _waitersById.Remove(nextWaiter.TicketId);
                }
                nextWaiter.Tcs.SetResult(true);
            }
        }

        // Convenience wrappers to run work exclusively inside the lock.

        // Async action (no result)
        public static async Task RunExclusiveAsync(string taskName, DbTaskPriority priority, Func<Task> body, int timeoutMs = 60000)
        {
            using (await EnterDbWriteLockAsync(taskName, priority, timeoutMs).ConfigureAwait(false))
            {
                await body().ConfigureAwait(false);
            }
        }

        // Async function (with result)
        public static async Task<T> RunExclusiveAsync<T>(string taskName, DbTaskPriority priority, Func<Task<T>> body, int timeoutMs = 60000)
        {
            using (await EnterDbWriteLockAsync(taskName, priority, timeoutMs).ConfigureAwait(false))
            {
                return await body().ConfigureAwait(false);
            }
        }

        // Synchronous action (no result)
        public static void RunExclusive(string taskName, DbTaskPriority priority, Action body, int timeoutMs = 60000)
        {
            DbWriteLockReleaser releaser = null;
            try
            {
                releaser = EnterDbWriteLockAsync(taskName, priority, timeoutMs).GetAwaiter().GetResult();
                body();
            }
            finally
            {
                releaser?.Dispose();
            }
        }

        // Synchronous function (with result)
        public static T RunExclusive<T>(string taskName, DbTaskPriority priority, Func<T> body, int timeoutMs = 60000)
        {
            DbWriteLockReleaser releaser = null;
            try
            {
                releaser = EnterDbWriteLockAsync(taskName, priority, timeoutMs).GetAwaiter().GetResult();
                return body();
            }
            finally
            {
                releaser?.Dispose();
            }
        }

        // Insert by priority (ascending: High=0 is first), then FIFO by enqueue time.
        private static void InsertSortedByPriorityAndFifo(PersistentQueueItem item)
        {
            int idx = 0;
            while (idx < _persistentQueue.Count)
            {
                var curr = _persistentQueue[idx];
                if (item.Priority < curr.Priority) break;
                if (item.Priority == curr.Priority && item.EnqueuedTicks < curr.EnqueuedTicks) break;
                idx++;
            }
            _persistentQueue.Insert(idx, item);
        }

        private static bool IsHeadOfLine(Guid id)
        {
            return _persistentQueue.Count > 0 && _persistentQueue[0].Id == id;
        }

        private static void RemoveTicketFromPersistentQueue(Guid id)
        {
            var i = _persistentQueue.FindIndex(p => p.Id == id);
            if (i >= 0) _persistentQueue.RemoveAt(i);
        }

        private static void LoadQueueFromDisk_NoThrow()
        {
            try
            {
                if (!File.Exists(_queueFilePath)) return;
                var json = File.ReadAllText(_queueFilePath);
                var list = _json.Deserialize<List<PersistentQueueItem>>(json) ?? new List<PersistentQueueItem>();
                // Ensure deterministic order after load.
                _persistentQueue.Clear();
                _persistentQueue.AddRange(list.OrderBy(p => p.Priority).ThenBy(p => p.EnqueuedTicks));
            }
            catch (Exception ex)
            {
                LogHelper.Log($"⚠️ Failed to load DB queue file. Starting with empty queue. Error: {ex.Message}");
                _persistentQueue.Clear();
            }
        }

        private static void SaveQueueToDisk_NoThrow()
        {
            try
            {
                var dir = Path.GetDirectoryName(_queueFilePath);
                if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                var json = _json.Serialize(_persistentQueue);
                var tmp = _queueFilePath + ".tmp";
                File.WriteAllText(tmp, json);
                if (File.Exists(_queueFilePath))
                {
                    // Replace atomically if possible
                    try
                    {
                        File.Replace(tmp, _queueFilePath, null);
                    }
                    catch
                    {
                        // Fallback
                        File.Copy(tmp, _queueFilePath, overwrite: true);
                        File.Delete(tmp);
                    }
                }
                else
                {
                    File.Move(tmp, _queueFilePath);
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"⚠️ Failed to save DB queue file. Error: {ex.Message}");
            }
        }

        // Remove entries that clearly cannot be processed by this process (e.g., after crash).
        // We keep only entries enqueued by the current process to avoid head-of-line blocking by orphans.
        private static void PurgeOrphanedEntries_NoThrow()
        {
            try
            {
                int currentPid = Process.GetCurrentProcess().Id;
                int before = _persistentQueue.Count;
                _persistentQueue.RemoveAll(p => p.ProcessId != currentPid);
                int removed = before - _persistentQueue.Count;
                if (removed > 0)
                {
                    LogHelper.Log($"ℹ️ Purged {removed} orphaned DB queue entries from previous runs.");
                }
            }
            catch (Exception ex)
            {
                LogHelper.Log($"⚠️ Failed to purge orphaned DB queue entries. Error: {ex.Message}");
            }
        }
    }
}