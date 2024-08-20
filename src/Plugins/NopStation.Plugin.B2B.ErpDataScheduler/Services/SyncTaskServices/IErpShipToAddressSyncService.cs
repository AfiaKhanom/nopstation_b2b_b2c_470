namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpShipToAddressSyncService
{
    Task<bool> IsErpShipToAddressSyncSuccessfulAsync();
}