using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
public record ErpSpecialPricePartialSyncModel
{
    #region Properties

    public int SyncTaskId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.ErpAccountNumber")]
    public string ErpAccountNumber { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.StockCode")]
    public string StockCode { get; set; }

    #endregion
}
