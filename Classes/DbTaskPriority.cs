namespace smart_attendance_middleware.Classes
{
    /// <summary>
    /// Defines the priority levels for tasks waiting for database access.
    /// </summary>
    public enum DbTaskPriority
    {
        /// <summary>
        /// Lowest priority, for background tasks like attendance logging.
        /// </summary>
        Low = 2,
        /// <summary>
        /// Medium priority, for tasks like changing a user's status.
        /// </summary>
        Medium = 1,
        /// <summary>
        /// Highest priority, for critical user-facing operations like creating or updating a user.
        /// </summary>
        High = 0
    }
}