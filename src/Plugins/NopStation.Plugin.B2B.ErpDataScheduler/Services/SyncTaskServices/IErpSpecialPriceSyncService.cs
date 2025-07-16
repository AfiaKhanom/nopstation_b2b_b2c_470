namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpSpecialPriceSyncService
{
    Task<bool> IsErpSpecialPriceSyncSuccessfulAsync(string? erpAccountNumber, string? stockCode, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default);
}