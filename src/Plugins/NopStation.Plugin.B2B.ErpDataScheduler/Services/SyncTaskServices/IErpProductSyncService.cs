namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpProductSyncService
{
    Task<bool> IsErpProductSyncSuccessfulAsync(string? stockCode, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default);
}