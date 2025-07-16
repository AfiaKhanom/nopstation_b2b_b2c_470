using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;

public record ErpOrderPartialSyncModel
{
    #region Properties

    public int SyncTaskId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.ErpAccountNumber")]
    public string ErpAccountNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.OrderNumber")]
    public string OrderNumber { get; set; }

    #endregion
}
