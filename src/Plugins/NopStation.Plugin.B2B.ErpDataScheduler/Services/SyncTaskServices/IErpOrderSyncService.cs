namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpOrderSyncService
{
    Task<bool> IsErpOrderSyncSuccessfulAsync(string? erpAccountNumber = null, string? orderNumber = null, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default);
}