using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpStockSyncTask : IJob
{
    #region Fields

    private readonly IErpStockSyncService _erpStockSyncService;

    #endregion

    #region Ctor

    public ErpStockSyncTask(IErpStockSyncService erpStockSyncService)
    {
        _erpStockSyncService = erpStockSyncService;
    }

    #endregion

    #region Methods

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.JobShouldExecute, out var shouldExecute) && shouldExecute)
        {
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsManualTrigger, out var isManualTrigger);
            context.MergedJobDataMap.TryGetString(nameof(ErpStockPartialSyncModel.StockCode), out var stockCode);

            var isIncrementalSync = false;

            if (string.IsNullOrWhiteSpace(stockCode))
            {
                context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.IsIncrementalSync, out isIncrementalSync);
            }

            await _erpStockSyncService.IsErpStockSyncSuccessfulAsync(stockCode, isManualTrigger, isIncrementalSync, context.CancellationToken);
        }
    }

    #endregion
}