using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace smart_attendance_middleware.Classes
{
    /// <summary>
    /// Manages a semaphore to coordinate database write access, ensuring that only one write operation occurs at a time.
    /// Tasks waiting for the lock are queued and processed sequentially based on priority.
    /// </summary>
    public static class DatabaseQueueManager
    {
        // A wrapper for a waiting task, holding its priority and a TaskCompletionSource.
        private class Waiter
        {
            public DbTaskPriority Priority { get; }
            public TaskCompletionSource<bool> Tcs { get; }
            public string TaskName { get; }

            public Waiter(DbTaskPriority priority, string taskName)
            {
                Priority = priority;
                TaskName = taskName;
                Tcs = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            }
        }

        private static readonly object _queueLock = new object();
        private static readonly List<Waiter> _waiters = new List<Waiter>();
        private static int _currentHolderCount = 0; // 0 or 1

        /// <summary>
        /// Asynchronously waits to acquire the database write lock based on priority.
        /// </summary>
        /// <param name="taskName">The name of the task acquiring the lock, for logging purposes.</param>
        /// <param name="priority">The priority of the task.</param>
        public static async Task EnterDbWriteLockAsync(string taskName, DbTaskPriority priority = DbTaskPriority.Low)
        {
            Waiter waiter;
            var stopwatch = Stopwatch.StartNew();

            lock (_queueLock)
            {
                // If the lock is free, acquire it immediately.
                if (_currentHolderCount == 0)
                {
                    _currentHolderCount = 1;
                    stopwatch.Stop();
                    LogHelper.Log($"🔒 Acquired DB write lock for {taskName} (Priority: {priority}) immediately.");
                    return; // Lock acquired, no need to wait.
                }

                // Otherwise, create a waiter and add it to the queue.
                LogHelper.Log($"⏳ {taskName} (Priority: {priority}) is waiting to acquire the DB write lock.");
                waiter = new Waiter(priority, taskName);
                _waiters.Add(waiter);
                _waiters.Sort((a, b) => a.Priority.CompareTo(b.Priority)); // Sort by priority
            }

            // Asynchronously wait for the TaskCompletionSource to be signaled.
            await waiter.Tcs.Task;

            stopwatch.Stop();
            LogHelper.Log($"🔒 Acquired DB write lock for {taskName} (Priority: {priority}) after {stopwatch.ElapsedMilliseconds}ms.");
        }

        /// <summary>
        /// Releases the database write lock and grants it to the next highest-priority waiting task.
        /// </summary>
        /// <param name="taskName">The name of the task releasing the lock, for logging purposes.</param>
        public static void ReleaseDbWriteLock(string taskName)
        {
            Waiter nextWaiter = null;

            lock (_queueLock)
            {
                // Find the highest-priority waiter (first in the sorted list).
                if (_waiters.Count > 0)
                {
                    nextWaiter = _waiters[0];
                    _waiters.RemoveAt(0);
                }
                else
                {
                    // No one is waiting, so mark the lock as free.
                    _currentHolderCount = 0;
                }
            }

            LogHelper.Log($"🔓 Released DB write lock for {taskName}.");

            // If there was a waiting task, signal it to proceed.
            // This "passes" the lock to the next waiter without making it free.
            if (nextWaiter != null)
            {
                nextWaiter.Tcs.SetResult(true);
            }
        }
    }
}