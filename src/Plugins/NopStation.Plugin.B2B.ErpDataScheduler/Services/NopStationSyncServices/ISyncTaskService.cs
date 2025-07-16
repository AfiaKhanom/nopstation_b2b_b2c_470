using NopStation.Plugin.B2B.ErpDataScheduler.Domain;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;

public partial interface ISyncTaskService
{
    Task DeleteTaskAsync(SyncTask task);

    Task<SyncTask> GetTaskByIdAsync(int taskId);

    Task<SyncTask> GetTaskByTypeAsync(string type);

    Task<IList<SyncTask>> GetAllTasksAsync();

    Task InsertTaskAsync(SyncTask task);

    Task UpdateTaskAsync(SyncTask task);

    Task<SyncTask> GetTaskByQuartzJobNameAsync(string quartzJobName);
}