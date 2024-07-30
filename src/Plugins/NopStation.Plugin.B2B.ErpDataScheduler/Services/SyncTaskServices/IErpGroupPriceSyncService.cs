namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public interface IErpGroupPriceSyncService
    {
        Task<bool> IsErpGroupPriceSyncSuccessfulAsync();
    }
}
