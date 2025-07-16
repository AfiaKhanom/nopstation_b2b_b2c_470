using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpGroupPriceSyncTask : IJob
{
    #region Fields

    private readonly IErpGroupPriceSyncService _erpGroupPriceSyncService;

    #endregion

    #region Ctor

    public ErpGroupPriceSyncTask(IErpGroupPriceSyncService erpGroupPriceSyncService)
    {
        _erpGroupPriceSyncService = erpGroupPriceSyncService;
    }

    #endregion

    #region Methods

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.JobShouldExecute, out var shouldExecute) && shouldExecute)
        {
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsManualTrigger, out var isManualTrigger);
            context.MergedJobDataMap.TryGetString(nameof(ErpGroupPricePartialSyncModel.PriceCode), out var priceCode);
            context.MergedJobDataMap.TryGetString(nameof(ErpGroupPricePartialSyncModel.StockCode), out var stockCode);

            var isIncrementalSync = false;

            if (string.IsNullOrWhiteSpace(stockCode) && string.IsNullOrWhiteSpace(priceCode))
            {
                context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.IsIncrementalSync, out isIncrementalSync);
            }

            await _erpGroupPriceSyncService.IsErpGroupPriceSyncSuccessfulAsync(priceCode, stockCode, isManualTrigger, isIncrementalSync, context.CancellationToken);
        }
    }

    #endregion
}