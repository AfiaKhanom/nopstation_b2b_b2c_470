namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpGroupPriceSyncService
{
    Task<bool> IsErpGroupPriceSyncSuccessfulAsync(string? priceCode, string? stockCode, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default);
}
