namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public interface IErpAccountSyncService
{
    Task<bool> IsErpAccountSyncSuccessfulAsync(string? erpAccountNumber, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default);
}
