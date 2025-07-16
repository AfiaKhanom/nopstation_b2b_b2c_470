namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;

public interface ISyncWorkflowMessageService
{
    Task SendSyncFailNotificationAsync(DateTime dateTime, string syncTaskName, string message = "");
}