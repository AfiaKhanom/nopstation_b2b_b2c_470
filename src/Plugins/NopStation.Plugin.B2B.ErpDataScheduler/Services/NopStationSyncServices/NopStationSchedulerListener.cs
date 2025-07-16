using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;

public class NopStationSchedulerListener : ISchedulerListener
{
    #region Fields

    private readonly ISyncTaskService _syncTaskService;

    #endregion

    #region Ctor

    public NopStationSchedulerListener(ISyncTaskService syncTaskService)
    {
        _syncTaskService = syncTaskService;
    }

    #endregion

    #region Methods

    public Task JobAdded(IJobDetail jobDetail, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task JobDeleted(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task JobInterrupted(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task JobPaused(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        var syncTask = await _syncTaskService.GetTaskByQuartzJobNameAsync(jobKey.Name);

        if (syncTask is not null)
        {
            syncTask.Enabled = false;
            await _syncTaskService.UpdateTaskAsync(syncTask);
        }
    }

    public async Task JobResumed(JobKey jobKey, CancellationToken cancellationToken = default)
    {
        var syncTask = await _syncTaskService.GetTaskByQuartzJobNameAsync(jobKey.Name);

        if (syncTask is not null)
        {
            syncTask.Enabled = true;
            syncTask.LastEnabledUtc = DateTime.UtcNow;
            await _syncTaskService.UpdateTaskAsync(syncTask);
        }
    }

    public Task JobScheduled(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task JobsPaused(string jobGroup, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task JobsResumed(string jobGroup, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task JobUnscheduled(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public async Task SchedulerError(string msg, SchedulerException cause, CancellationToken cancellationToken = default)
    {
        //await _nopStationScheduler.TriggerJobFailedErrorNotificationEmailAsync(msg);
    }

    public Task SchedulerInStandbyMode(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SchedulerShutdown(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SchedulerShuttingdown(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SchedulerStarted(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SchedulerStarting(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task SchedulingDataCleared(CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TriggerFinalized(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TriggerPaused(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TriggerResumed(TriggerKey triggerKey, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TriggersPaused(string? triggerGroup, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TriggersResumed(string? triggerGroup, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    #endregion
}
