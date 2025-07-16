namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpStockSyncService
{
    Task<bool> IsErpStockSyncSuccessfulAsync(string? stockCode, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default);
}