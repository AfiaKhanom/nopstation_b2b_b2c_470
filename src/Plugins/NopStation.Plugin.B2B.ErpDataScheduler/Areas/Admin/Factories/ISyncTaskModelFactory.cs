using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Factories;

public partial interface ISyncTaskModelFactory
{
    Task<SyncTaskSearchModel> PrepareTaskSearchModelAsync(SyncTaskSearchModel searchModel);
    Task<SyncTaskListModel> PrepareTaskListModelAsync(SyncTaskSearchModel searchModel);
    Task<SyncTaskModel> PrepareTaskModelByIdAsync(int taskId);
    Task<SyncTaskModel> PrepareTaskModelByIdAsync(string quartzJobName);
    Task<SyncLogListModel> PrepareSyncLogListModelAsync(SyncLogSearchModel searchModel);
    Task<QuartzJobListModel> PrepareJobListPagedModelAsync(SyncTaskSearchModel searchModel);
}
