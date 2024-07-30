using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Factories
{
    /// <summary>
    /// Represents the schedule task model factory
    /// </summary>
    public partial interface ISyncTaskModelFactory
    {
        /// <summary>
        /// Prepare task search model
        /// </summary>
        /// <param name="searchModel">SyncTask search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the task search model
        /// </returns>
        Task<SyncTaskSearchModel> PrepareTaskSearchModelAsync(SyncTaskSearchModel searchModel);

        /// <summary>
        /// Prepare paged task list model
        /// </summary>
        /// <param name="searchModel">SyncTask search model</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the task list model
        /// </returns>
        Task<SyncTaskListModel> PrepareTaskListModelAsync(SyncTaskSearchModel searchModel);

        /// <summary>
        /// Prepare a task model
        /// </summary>
        /// <param name="taskId">SyncTask id</param>
        /// <returns>
        /// A task that represents the asynchronous operation
        /// The task result contains the task model
        /// </returns>
        Task<SyncTaskModel> PrepareTaskModelByIdAsync(int taskId);

        Task<SyncLogListModel> PrepareSyncLogListModelAsync(SyncLogSearchModel searchModel);
    }
}
