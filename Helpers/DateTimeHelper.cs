namespace LampStoreProjects.Helpers
{
    /// <summary>
    /// Centralized datetime helper — returns Vietnam time (UTC+7) for all user-facing timestamps.
    /// </summary>
    public static class DateTimeHelper
    {
        private static readonly TimeSpan VietnamOffset = TimeSpan.FromHours(7);

        /// <summary>
        /// Returns current Vietnam time (UTC+7)
        /// </summary>
        public static DateTime VietnamNow => DateTimeOffset.UtcNow.ToOffset(VietnamOffset).DateTime;
    }
}
