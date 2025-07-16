using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpAccountSyncTask : IJob
{
    #region Fields

    private readonly IErpAccountSyncService _erpAccountSyncService;

    #endregion

    #region Ctor

    public ErpAccountSyncTask(IErpAccountSyncService erpAccountSyncService)
    {
        _erpAccountSyncService = erpAccountSyncService;
    }

    #endregion

    #region Methods

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.JobShouldExecute, out var shouldExecute) && shouldExecute)
        {
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsManualTrigger, out var isManualTrigger);
            context.MergedJobDataMap.TryGetString(nameof(ErpAccountPartialSyncModel.ErpAccountNumber), out var erpAccountNumber);

            var isIncrementalSync = false;

            if (string.IsNullOrWhiteSpace(erpAccountNumber))
            {
                context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.IsIncrementalSync, out isIncrementalSync);
            }

            await _erpAccountSyncService.IsErpAccountSyncSuccessfulAsync(erpAccountNumber, isManualTrigger, isIncrementalSync, context.CancellationToken);
        }
    }

    #endregion
}