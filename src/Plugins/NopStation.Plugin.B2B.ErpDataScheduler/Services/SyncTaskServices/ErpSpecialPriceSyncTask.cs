using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public partial class ErpSpecialPriceSyncTask : ISyncTask
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

    public virtual async Task ExecuteAsync()
    {
        await _erpSpecialPriceSyncService.IsErpSpecialPriceSyncSuccessfulAsync();
    }

    #endregion
}