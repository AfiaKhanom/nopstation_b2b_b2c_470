namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpSpecialPriceSyncService
{
    Task<bool> IsErpSpecialPriceSyncSuccessfulAsync();
}