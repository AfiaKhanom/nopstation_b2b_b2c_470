using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;

public interface ISyncLogService
{
    Task SyncLogSaveOnFileAsync(string syncTaskName = "", ErpSyncLevel syncLavel = 0, string shortMessage = "", string fullMessage = "");
    Task<IList<string>> GetAllSyncLogFiles(string syncTaskName = "", int syncTaskId = 0);
    string GetSyncLogFilePath(string fileName);
    Task DeleteAllLogFiles(string syncTaskName = "", int syncTaskId = 0);
}