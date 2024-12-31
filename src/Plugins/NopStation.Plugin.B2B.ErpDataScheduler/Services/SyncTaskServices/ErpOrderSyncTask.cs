using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public partial class ErpOrderSyncTask : ISyncTask
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

    public virtual async Task ExecuteAsync()
    {
        await _erpOrderSyncService.IsErpOrderSyncSuccessfulAsync();
    }

    #endregion
}