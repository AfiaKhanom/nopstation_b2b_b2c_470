namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpProductSyncService
{
    Task<bool> IsErpProductSyncSuccessfulAsync();
}