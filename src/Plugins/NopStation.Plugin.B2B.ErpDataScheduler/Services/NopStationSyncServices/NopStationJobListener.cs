using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;

public class NopStationJobListener : IJobListener
{
    #region Fields

    public string Name => "B2BErpJobExecutionListener";
    private readonly ISyncTaskService _syncTaskService;
    public static event Action<string>? OnJobStatusChanged;
    private readonly INopStationScheduler _nopStationScheduler;

    #endregion

    #region Ctor

    public NopStationJobListener(ISyncTaskService syncTaskService, INopStationScheduler nopStationScheduler)
    {
        _syncTaskService = syncTaskService;
        _nopStationScheduler = nopStationScheduler;
    }

    #endregion

    #region Methods

    public Task JobExecutionVetoed(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task JobToBeExecuted(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        var syncTask = await _syncTaskService.GetTaskByQuartzJobNameAsync(context.JobDetail.Key.Name);

        if (syncTask is null)
            return;

        if (syncTask.Enabled)
        {
            context.JobDetail.JobDataMap[ErpDataSchedulerDefaults.JobShouldExecute] = true;
            context.JobDetail.JobDataMap[ErpDataSchedulerDefaults.IsIncrementalSync] = syncTask.IsIncremental;
            syncTask.LastStartUtc = context.FireTimeUtc.UtcDateTime;
            syncTask.IsRunning = true;
            await _syncTaskService.UpdateTaskAsync(syncTask);
        }

        // Notify subscribers
        OnJobStatusChanged?.Invoke($"{context.JobDetail.Key.Name}-1");
    }

    public async Task JobWasExecuted(IJobExecutionContext context, JobExecutionException? jobException, CancellationToken cancellationToken = default)
    {
        var syncTask = await _syncTaskService.GetTaskByQuartzJobNameAsync(context.JobDetail.Key.Name);

        if (syncTask is null)
            return;

        var jobExecutionTime = context.JobRunTime;
        syncTask.LastStartUtc = context.FireTimeUtc.UtcDateTime;
        syncTask.LastEndUtc = context.FireTimeUtc.UtcDateTime.Add(jobExecutionTime);
        syncTask.IsRunning = false;

        if (jobException is null)
            syncTask.LastSuccessUtc = context.FireTimeUtc.UtcDateTime.Add(jobExecutionTime);

        if (jobException is not null)
            await _nopStationScheduler.TriggerJobFailedErrorNotificationEmailAsync(syncTask.Name, jobException.Message);

        await _syncTaskService.UpdateTaskAsync(syncTask);

        // Notify subscribers
        OnJobStatusChanged?.Invoke($"{context.JobDetail.Key.Name}-0");
    }

    #endregion
}
