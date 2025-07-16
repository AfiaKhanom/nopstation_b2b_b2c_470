using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public class ErpProductSyncTask : IJob
{
    #region Fields

    private readonly IErpProductSyncService _erpProductSyncService;

    #endregion

    #region Ctor

    public ErpProductSyncTask(IErpProductSyncService erpProductSyncService)
    {
        _erpProductSyncService = erpProductSyncService;
    }

    #endregion

    #region Methods

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.JobShouldExecute, out var shouldExecute) && shouldExecute)
        {
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsManualTrigger, out var isManualTrigger);
            context.MergedJobDataMap.TryGetString(nameof(ErpProductPartialSyncModel.StockCode), out var stockCode);

            var isIncrementalSync = false;

            if (string.IsNullOrWhiteSpace(stockCode))
            {
                context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.IsIncrementalSync, out isIncrementalSync);
            }

            await _erpProductSyncService.IsErpProductSyncSuccessfulAsync(stockCode, isManualTrigger, isIncrementalSync, context.CancellationToken);
        }
    }

    #endregion
}
