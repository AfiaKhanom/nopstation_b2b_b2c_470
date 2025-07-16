using Nop.Core.Infrastructure;
using Nop.Services.ScheduleTasks;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;

public partial class SyncLogFileDeleteTask : IScheduleTask
{
    #region Fields

    private readonly ISyncLogService _syncLogService;
    private readonly ISyncTaskService _syncTaskService;
    private readonly INopFileProvider _fileProvider;
    private const int DELETE_FILES_OLDER_THAN_N_DAYS_THRESHOLD = 15;

    #endregion

    #region Ctor

    public SyncLogFileDeleteTask(ISyncLogService syncLogService,
        ISyncTaskService syncTaskService,
        INopFileProvider nopFileProvider)
    {
        _syncLogService = syncLogService;
        _syncTaskService = syncTaskService;
        _fileProvider = nopFileProvider;
    }

    #endregion

    #region Methods

    public virtual async Task ExecuteAsync()
    {
        var syncTasks = (await _syncTaskService.GetAllTasksAsync()).ToList();

        foreach (var syncTask in syncTasks)
        {
            try
            {
                var syncLogs = await _syncLogService.GetAllSyncLogFiles(syncTask.Name);

                foreach (var logFile in syncLogs)
                {
                    var logFileName = _fileProvider.GetFileName(logFile).Split('-');
                    var logFileDate = new DateTime(int.Parse(logFileName[0]), int.Parse(logFileName[1]), int.Parse(logFileName[2]));

                    var difference = DateTime.UtcNow.Date - logFileDate;

                    if (difference.Days > DELETE_FILES_OLDER_THAN_N_DAYS_THRESHOLD)
                    {
                        _fileProvider.DeleteFile(logFile);
                    }
                }
            }
            catch (Exception ex)
            {
                await _syncLogService.SyncLogSaveOnFileAsync(syncTask.Name, 0, ex.Message, ex.StackTrace ?? string.Empty);
            }
        }

    }

    #endregion
}