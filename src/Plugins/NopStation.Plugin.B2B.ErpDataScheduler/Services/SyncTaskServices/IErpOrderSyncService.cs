namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public interface IErpOrderSyncService
    {
        Task<bool> IsErpOrderSyncSuccessfulAsync();
    }
}