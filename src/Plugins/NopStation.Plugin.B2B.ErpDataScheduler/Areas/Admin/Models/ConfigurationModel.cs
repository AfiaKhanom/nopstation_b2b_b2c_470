using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;

public record ConfigurationModel : BaseNopModel, ISettingsModel
{
    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.NeedQuoteOrderCall")]
    public bool NeedQuoteOrderCall { get; set; }
    public bool NeedQuoteOrderCall_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.EnalbeSendingEmailNotificationToStoreOwnerOnSyncError")]
    public bool EnalbeSendingEmailNotificationToStoreOwnerOnSyncError { get; set; }
    public bool EnalbeSendingEmailNotificationToStoreOwnerOnSyncError_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.Admin.Configure.Fields.AdditionalEmailAddresses")]
    public string? AdditionalEmailAddresses { get; set; }
    public bool AdditionalEmailAddresses_OverrideForStore { get; set; }
}
