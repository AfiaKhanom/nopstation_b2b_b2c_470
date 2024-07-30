using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler
{
    /// <summary>
    /// Erp Data Scheduler Task runner interface
    /// </summary>
    public interface ISyncTaskRunner
    {
        /// <summary>
        /// Executes the task
        /// </summary>
        /// <param name="task">SyncTask</param>
        /// <param name="forceRun">Force run</param>
        /// <param name="throwException">A value indicating whether exception should be thrown if some error happens</param>
        /// <param name="ensureRunOncePerPeriod">A value indicating whether we should ensure this task is run once per run period</param>
        Task ExecuteAsync(SyncTask task, bool forceRun = false, bool throwException = false, bool ensureRunOncePerPeriod = true);
    }
}
