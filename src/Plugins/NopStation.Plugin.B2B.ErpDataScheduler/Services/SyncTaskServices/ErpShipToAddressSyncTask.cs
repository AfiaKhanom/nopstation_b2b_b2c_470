using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public partial class ErpShipToAddressSyncTask : ISyncTask
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

    public virtual async Task ExecuteAsync()
    {
        await _erpShipToAddressSyncService.IsErpShipToAddressSyncSuccessfulAsync();
    }

    #endregion
}