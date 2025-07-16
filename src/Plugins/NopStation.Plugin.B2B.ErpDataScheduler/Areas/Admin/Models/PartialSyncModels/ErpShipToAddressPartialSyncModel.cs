using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
public record ErpShipToAddressPartialSyncModel
{
    #region Properties

    public int SyncTaskId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.ErpAccountNumber")]
    public string ErpAccountNumber { get; set; }

    #endregion
}
