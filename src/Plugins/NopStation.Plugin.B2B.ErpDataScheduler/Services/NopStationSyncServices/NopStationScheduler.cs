using Quartz;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;

public class NopStationScheduler : INopStationScheduler
{
    #region Fields

    private readonly ISchedulerFactory _schedulerFactory;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;

    #endregion

    #region Ctor

    public NopStationScheduler(ISchedulerFactory schedulerFactory,
        ISyncWorkflowMessageService syncWorkflowMessageService)
    {
        _schedulerFactory = schedulerFactory;
        _syncWorkflowMessageService = syncWorkflowMessageService;
    }

    #endregion

    #region Utilities

    private static string PrepareCronExpression(string timeSlot, int dayOfWeek = 1)
    {
        var second = 0;
        var minutes = 0;
        var hours = 0;
        var dom = "?";
        var month = "*";
        var dow = dayOfWeek;
        var year = "*";

        if (timeSlot is not null)
        {
            var timeParts = timeSlot.Split(':');
            hours = int.Parse(timeParts[0]);
            minutes = int.Parse(timeParts[1]);
        }

        return $"{second} {minutes} {hours} {dom} {month} {dow} {year}";
    }

    #endregion

    #region Methods

    public async Task CreateScheduleJobAsync<TJob>(string jobIdentity,
        bool isDurable = false,
        bool requestRecovery = false,
        CancellationToken cancellationToken = default) where TJob : IJob
    {
        ArgumentException.ThrowIfNullOrEmpty(jobIdentity, nameof(jobIdentity));

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var jobKey = new JobKey(jobIdentity);

        var job = await scheduler.GetJobDetail(jobKey, cancellationToken);

        if (job is not null)
            return;

        job = JobBuilder.Create<TJob>()
            .WithIdentity(jobKey)
            .StoreDurably(isDurable)
            .RequestRecovery(requestRecovery)
            .Build();

        await scheduler.AddJob(job, true, true, cancellationToken);
    }

    public async Task CreateTriggerAsync(string jobIdentity, int dayOfWeek, string timeSlot, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(jobIdentity, nameof(jobIdentity));
        ArgumentException.ThrowIfNullOrEmpty(timeSlot, nameof(timeSlot));

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var jobKey = new JobKey(jobIdentity);
        var job = await scheduler.GetJobDetail(jobKey, cancellationToken) ??
            throw new ArgumentException($"The requested job with identity {jobIdentity} was not found.");

        var triggerKey = PrepareTriggerKey(jobIdentity, dayOfWeek, timeSlot);

        var trigger = await scheduler.GetTrigger(triggerKey, cancellationToken);

        if (trigger is not null)
            return;

        var cronExpression = PrepareCronExpression(timeSlot, dayOfWeek);

        trigger = TriggerBuilder.Create()
            .WithIdentity(triggerKey)
            .WithCronSchedule(cronExpression, x => x.InTimeZone(TimeZoneInfo.Utc))
            .ForJob(job)
            .StartAt(DateBuilder.FutureDate(5, IntervalUnit.Minute))
            .Build();

        await scheduler.ScheduleJob(trigger, cancellationToken);
    }

    public async Task UnscheduleTriggerAsync(string triggerIdentity, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(triggerIdentity, nameof(triggerIdentity));

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);
        var triggerKey = new TriggerKey(triggerIdentity);

        await scheduler.UnscheduleJob(triggerKey, cancellationToken);
    }

    public async Task UnscheduleTriggerAsync(IReadOnlyCollection<TriggerKey> triggerKeys, CancellationToken cancellationToken = default)
    {
        if (triggerKeys is null || triggerKeys.Count == 0)
            return;

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        await scheduler.UnscheduleJobs(triggerKeys, cancellationToken);
    }

    public TriggerKey PrepareTriggerKey(string jobIdentity, int dayOfWeek, string timeslot)
    {
        ArgumentException.ThrowIfNullOrEmpty(jobIdentity, nameof(jobIdentity));
        ArgumentException.ThrowIfNullOrWhiteSpace(timeslot, nameof(timeslot));

        var triggerTime = timeslot.Replace(':', '-');

        return new TriggerKey($"trigger-{jobIdentity}-{dayOfWeek}-{triggerTime}");
    }

    public async Task<IJobDetail?> GetScheduleJobAsync(string jobIdentity, CancellationToken cancellationToken = default)
    {
        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var jobKey = new JobKey(jobIdentity);

        return await scheduler.GetJobDetail(jobKey, default);
    }

    public async Task ExecuteSchedulerAsync(string quartzJobName, JobDataMap jobDataMap, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(quartzJobName, nameof(quartzJobName));

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var jobKey = new JobKey(quartzJobName);

        _ = await scheduler.GetJobDetail(jobKey, cancellationToken) ??
            throw new ArgumentException("Schedule job not found by the specified job identity key");

        await scheduler.TriggerJob(jobKey, jobDataMap, default);
    }

    public async Task DisableScheduleJobAsync(string quartzJobName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(quartzJobName, nameof(quartzJobName));

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var jobKey = new JobKey(quartzJobName);

        await scheduler.PauseJob(jobKey, cancellationToken);
    }

    public async Task EnableScheduleJobAsync(string quartzJobName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(quartzJobName, nameof(quartzJobName));

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var jobKey = new JobKey(quartzJobName);

        await scheduler.ResumeJob(jobKey, cancellationToken);
    }

    public async Task TerminateSchedulerAsync(string quartzJobName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrEmpty(quartzJobName, nameof(quartzJobName));

        var scheduler = await _schedulerFactory.GetScheduler(cancellationToken);

        var jobKey = new JobKey(quartzJobName);

        _ = await scheduler.GetJobDetail(jobKey, cancellationToken) ??
            throw new ArgumentException("Schedule job not found by the specified job identity key");

        await scheduler.Interrupt(jobKey, cancellationToken);
    }

    public JobDataMap PrepareJobDataMap(IList<KeyValuePair<string, object>> jobDataMaps)
    {
        var jobDataMap = new JobDataMap()
        {
            { ErpDataSchedulerDefaults.IsManualTrigger, "true"}
        };

        foreach (var jobData in jobDataMaps)
            jobDataMap.Add(jobData);

        return jobDataMap;
    }

    public async Task TriggerJobFailedErrorNotificationEmailAsync(string syncTaskName, string message = "")
    {
        await _syncWorkflowMessageService.SendSyncFailNotificationAsync(DateTime.UtcNow, syncTaskName, message);
    }

    #endregion
}
