using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;

public interface INopStationScheduler
{
    /// <summary>
    /// Prepare trigger key
    /// </summary>
    /// <param name="jobIdentity"></param>
    /// <param name="dayOfWeek"></param>
    /// <param name="timeslot"></param>
    /// <returns></returns>
    TriggerKey PrepareTriggerKey(string jobIdentity, int dayOfWeek, string timeslot);

    /// <summary>
    /// Create Schedule Job
    /// </summary>
    /// <typeparam name="TJob"></typeparam>
    /// <param name="jobIdentity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateScheduleJobAsync<TJob>(string jobIdentity,
        bool isDurable = false,
        bool requestRecovery = false,
        CancellationToken cancellationToken = default) where TJob : IJob;

    /// <summary>
    /// Create Trigger For Scheduled Job
    /// </summary>
    /// <param name="jobIdentity"></param>
    /// <param name="dayOfWeek"></param>
    /// <param name="timeSlot"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task CreateTriggerAsync(string jobIdentity, int dayOfWeek, string timeSlot,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unschedule single trigger
    /// </summary>
    /// <param name="triggerIdentity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UnscheduleTriggerAsync(string triggerIdentity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Unschedule multiple triggers
    /// </summary>
    /// <param name="triggerKeys"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task UnscheduleTriggerAsync(IReadOnlyCollection<TriggerKey> triggerKeys,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="jobIdentity"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task<IJobDetail?> GetScheduleJobAsync(string jobIdentity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Execute a job immediately with additional data
    /// </summary>
    /// <param name="quartzJobName"></param>
    /// <param name="jobDataMap"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task ExecuteSchedulerAsync(string quartzJobName, JobDataMap jobDataMap, CancellationToken cancellationToken = default);

    /// <summary>
    /// Disable a scheduled job if it is enabled
    /// </summary>
    /// <param name="quartzJobName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task DisableScheduleJobAsync(string quartzJobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Enable a schedule job if it is disabled
    /// </summary>
    /// <param name="quartzJobName"></param>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task EnableScheduleJobAsync(string quartzJobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Terminate a running job immediately
    /// </summary>
    /// <param name="quartzJobName"></param>
    /// <returns></returns>
    Task TerminateSchedulerAsync(string quartzJobName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Prepares job data map
    /// </summary>
    /// <param name="jobDataMaps"></param>
    /// <returns></returns>
    JobDataMap PrepareJobDataMap(IList<KeyValuePair<string, object>> jobDataMaps);

    /// <summary>
    /// Triggers a job failed error notification email
    /// </summary>
    /// <param name="message"></param>
    /// <param name="syncTaskName"></param>
    /// <returns></returns>
    Task TriggerJobFailedErrorNotificationEmailAsync(string syncTaskName, string message = "");
}
