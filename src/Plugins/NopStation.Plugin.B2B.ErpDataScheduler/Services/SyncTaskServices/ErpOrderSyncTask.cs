using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpOrderSyncTask : IJob
{
    #region Fields

    private readonly IErpOrderSyncService _erpOrderSyncService;

    #endregion

    #region Ctor

    public ErpOrderSyncTask(IErpOrderSyncService erpOrderSyncService)
    {
        _erpOrderSyncService = erpOrderSyncService;
    }

    #endregion

    #region Methods

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.JobShouldExecute, out var shouldExecute) && shouldExecute)
        {
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsManualTrigger, out var isManualTrigger);
            context.MergedJobDataMap.TryGetString(nameof(ErpOrderPartialSyncModel.ErpAccountNumber), out var erpAccountNumber);
            context.MergedJobDataMap.TryGetString(nameof(ErpOrderPartialSyncModel.OrderNumber), out var orderNumber);

            var isIncrementalSync = false;

            if (string.IsNullOrWhiteSpace(erpAccountNumber) && string.IsNullOrWhiteSpace(orderNumber))
            {
                context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.IsIncrementalSync, out isIncrementalSync);
            }

            await _erpOrderSyncService.IsErpOrderSyncSuccessfulAsync(erpAccountNumber, orderNumber, isManualTrigger, isIncrementalSync, context.CancellationToken);
        }
    }

    #endregion
}