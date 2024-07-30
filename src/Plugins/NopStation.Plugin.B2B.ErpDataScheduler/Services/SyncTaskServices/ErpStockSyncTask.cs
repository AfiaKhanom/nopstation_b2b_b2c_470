using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public partial class ErpStockSyncTask : ISyncTask
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

        public virtual async Task ExecuteAsync()
        {
            await _erpStockSyncService.IsErpStockSyncSuccessfulAsync();
        }

        #endregion
    }
}