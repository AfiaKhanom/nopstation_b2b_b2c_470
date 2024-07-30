namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public interface IErpInvoiceSyncService
    {
        Task<bool> IsErpInvoiceSyncSuccessfulAsync();
    }
}
