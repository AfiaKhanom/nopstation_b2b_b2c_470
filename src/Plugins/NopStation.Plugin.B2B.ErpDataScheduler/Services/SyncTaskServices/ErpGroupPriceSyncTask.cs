using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public partial class ErpGroupPriceSyncTask : ISyncTask
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

        public virtual async Task ExecuteAsync()
        {
            await _erpGroupPriceSyncService.IsErpGroupPriceSyncSuccessfulAsync();
        }

        #endregion
    }
}