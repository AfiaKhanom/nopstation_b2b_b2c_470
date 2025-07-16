using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpSpecialPriceSyncTask : IJob
{
    #region Fields

    private readonly IErpSpecialPriceSyncService _erpSpecialPriceSyncService;

    #endregion

    #region Ctor

    public ErpSpecialPriceSyncTask(IErpSpecialPriceSyncService erpSpecialPriceSyncService)
    {
        _erpSpecialPriceSyncService = erpSpecialPriceSyncService;
    }

    #endregion

    #region Methods

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.JobShouldExecute, out var shouldExecute) && shouldExecute)
        {
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsManualTrigger, out var isManualTrigger);
            context.MergedJobDataMap.TryGetString(nameof(ErpSpecialPricePartialSyncModel.ErpAccountNumber), out var erpAccountNumber);
            context.MergedJobDataMap.TryGetString(nameof(ErpSpecialPricePartialSyncModel.StockCode), out var stockCode);

            var isIncrementalSync = false;

            if (string.IsNullOrWhiteSpace(erpAccountNumber) && string.IsNullOrWhiteSpace(stockCode))
            {
                context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.IsIncrementalSync, out isIncrementalSync);
            }

            await _erpSpecialPriceSyncService.IsErpSpecialPriceSyncSuccessfulAsync(erpAccountNumber, stockCode, isManualTrigger, isIncrementalSync, context.CancellationToken);
        }
    }

    #endregion
}