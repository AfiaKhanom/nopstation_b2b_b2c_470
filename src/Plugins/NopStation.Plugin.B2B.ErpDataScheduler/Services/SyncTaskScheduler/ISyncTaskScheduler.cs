namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler
{
    /// <summary>
    /// Task manager interface
    /// </summary>
    public interface ISyncTaskScheduler
    {
        /// <summary>
        /// Initializes task scheduler
        /// </summary>
        Task<bool> InitializeAsync();

        /// <summary>
        /// Starts the task scheduler
        /// </summary>
        public void StartSyncTaskScheduler();

        /// <summary>
        /// Stops the task scheduler
        /// </summary>
        public void StopSyncTaskScheduler();
    }
}