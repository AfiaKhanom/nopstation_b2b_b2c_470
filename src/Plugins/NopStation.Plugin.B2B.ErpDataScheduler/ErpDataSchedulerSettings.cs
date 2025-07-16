using Nop.Core.Configuration;

namespace NopStation.Plugin.B2B.ErpDataScheduler;

public class ErpDataSchedulerSettings : ISettings
{
    public bool NeedQuoteOrderCall { get; set; }
    public bool EnalbeSendingEmailNotificationToStoreOwnerOnSyncError { get; set; }
    public string? AdditionalEmailAddresses { get; set; }
}
