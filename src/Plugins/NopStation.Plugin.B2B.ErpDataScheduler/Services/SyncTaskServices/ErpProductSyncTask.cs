using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public partial class ErpProductSyncTask : ISyncTask
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

        public virtual async Task ExecuteAsync()
        {
            await _erpProductSyncService.IsErpProductSyncSuccessfulAsync();
        }

        #endregion
    }
}