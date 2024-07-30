using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public partial class ErpInvoiceSyncTask : ISyncTask
    {
        #region Fields

        private readonly IErpInvoiceSyncService _erpInvoiceSyncService;

        #endregion

        #region Ctor

        public ErpInvoiceSyncTask(IErpInvoiceSyncService erpInvoiceSyncService)
        {
            _erpInvoiceSyncService = erpInvoiceSyncService;
        }

        #endregion

        #region Methods

        public virtual async Task ExecuteAsync()
        {
            await _erpInvoiceSyncService.IsErpInvoiceSyncSuccessfulAsync();
        }

        #endregion
    }
}