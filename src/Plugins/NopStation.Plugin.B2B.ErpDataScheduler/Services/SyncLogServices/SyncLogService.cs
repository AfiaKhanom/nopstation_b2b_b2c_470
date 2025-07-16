using Nop.Core.Infrastructure;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;

public class SyncLogService : ISyncLogService
{
    private readonly INopFileProvider _fileProvider;
    private readonly ISyncTaskService _syncTaskService;

    public SyncLogService(INopFileProvider fileProvider, ISyncTaskService syncTaskService)
    {
        _fileProvider = fileProvider;
        _syncTaskService = syncTaskService;
    }

    private string GetSyncLogDirectoryPath()
    {
        var path = _fileProvider.GetAbsolutePath(ErpDataSchedulerDefaults.SyncLogFileSaveDefaultPath);

        if (!_fileProvider.DirectoryExists(path))
            _fileProvider.CreateDirectory(path);

        return path;
    }

    public async Task SyncLogSaveOnFileAsync(string syncTaskName = "", ErpSyncLevel syncLavel = 0, string shortMessage = "", string fullMessage = "")
    {
        var path = GetSyncLogDirectoryPath();

        var fileName = $"{DateTime.UtcNow:yyyy-MM-dd}-{syncTaskName}-Log.txt";
        var filePath = _fileProvider.Combine(path, fileName);

        var logEntry = $"Created On Utc: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss}\nSync Type: {syncLavel}\nShort Message: {shortMessage}\nFull Message: {fullMessage}\n\n" +
                            $"-----------------------------------------------------------------------------------------------------------------------------------------\n\n";

        using var writer = File.AppendText(filePath);
        writer.WriteLine(logEntry);
    }

    public async Task<IList<string>> GetAllSyncLogFiles(string syncTaskName = "", int syncTaskId = 0)
    {
        var path = GetSyncLogDirectoryPath();

        if (syncTaskId > 0 && string.IsNullOrEmpty(syncTaskName))
            syncTaskName = (await _syncTaskService.GetTaskByIdAsync(syncTaskId)).Name;

        return _fileProvider.GetFiles(path, $"*{syncTaskName}-Log.{ErpDataSchedulerDefaults.SyncLogFileExtension}").OrderByDescending(p => _fileProvider.GetLastWriteTime(p)).ToList();
    }

    public string GetSyncLogFilePath(string fileName)
    {
        return _fileProvider.Combine(GetSyncLogDirectoryPath(), fileName);
    }

    public async Task DeleteAllLogFiles(string syncTaskName = "", int syncTaskId = 0)
    {
        var allFiles = (await GetAllSyncLogFiles(syncTaskName, syncTaskId)).ToList();

        foreach (var file in allFiles)
            _fileProvider.DeleteFile(file);
    }
}