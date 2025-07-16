using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

[DisallowConcurrentExecution]
public partial class ErpShipToAddressSyncTask : IJob
{
    #region Fields

    private readonly IErpShipToAddressSyncService _erpShipToAddressSyncService;

    #endregion

    #region Ctor

    public ErpShipToAddressSyncTask(IErpShipToAddressSyncService erpShipToAddressSyncService)
    {
        _erpShipToAddressSyncService = erpShipToAddressSyncService;
    }

    #endregion

    #region Methods

    public async Task Execute(IJobExecutionContext context)
    {
        if (context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.JobShouldExecute, out var shouldExecute) && shouldExecute)
        {
            context.MergedJobDataMap.TryGetBoolean(ErpDataSchedulerDefaults.IsManualTrigger, out var isManualTrigger);
            context.MergedJobDataMap.TryGetString(nameof(ErpShipToAddressPartialSyncModel.ErpAccountNumber), out var erpAccountNumber);

            var isIncrementalSync = false;

            if (string.IsNullOrWhiteSpace(erpAccountNumber))
            {
                context.JobDetail.JobDataMap.TryGetBooleanValue(ErpDataSchedulerDefaults.IsIncrementalSync, out isIncrementalSync);
            }

            await _erpShipToAddressSyncService.IsErpShipToAddressSyncSuccessfulAsync(erpAccountNumber, isManualTrigger, isIncrementalSync, context.CancellationToken);
        }
    }

    #endregion
}