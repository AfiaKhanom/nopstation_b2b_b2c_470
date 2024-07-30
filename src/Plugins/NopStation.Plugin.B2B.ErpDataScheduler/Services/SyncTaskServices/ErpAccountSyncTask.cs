using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public partial class ErpAccountSyncTask : ISyncTask
    {
        #region Fields

        private readonly IErpAccountSyncService _erpAccountSyncService;

        #endregion

        #region Ctor

        public ErpAccountSyncTask(IErpAccountSyncService erpAccountSyncService)
        {
            _erpAccountSyncService = erpAccountSyncService;
        }

        #endregion

        #region Methods

        public virtual async Task ExecuteAsync()
        {
            await _erpAccountSyncService.IsErpAccountSyncSuccessfulAsync();
        }

        #endregion
    }
}