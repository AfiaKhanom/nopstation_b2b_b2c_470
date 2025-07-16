using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;
public class NopStationTriggerListener : ITriggerListener
{
    public string Name => "B2bSchedulerTriggerListener";

    public Task TriggerComplete(ITrigger trigger, IJobExecutionContext context,
        SchedulerInstruction triggerInstructionCode, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TriggerFired(ITrigger trigger, IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task TriggerMisfired(ITrigger trigger, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task<bool> VetoJobExecution(ITrigger trigger, IJobExecutionContext context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(false);
    }
}
