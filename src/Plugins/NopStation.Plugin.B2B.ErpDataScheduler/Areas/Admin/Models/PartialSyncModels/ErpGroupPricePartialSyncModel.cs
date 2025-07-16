using Nop.Web.Framework.Mvc.ModelBinding;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;
public record ErpGroupPricePartialSyncModel
{
    #region Properties

    public int SyncTaskId { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.PriceCode")]
    public string PriceCode { get; set; }

    [NopResourceDisplayName("Plugin.Misc.NopStation.ErpDataScheduler.PartialSync.StockCode")]
    public string StockCode { get; set; }

    #endregion
}
