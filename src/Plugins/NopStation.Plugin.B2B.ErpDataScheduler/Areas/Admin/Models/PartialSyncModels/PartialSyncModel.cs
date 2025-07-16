namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models.PartialSyncModels;

public record PartialSyncModel
{
    #region Properties
    
    public ErpAccountPartialSyncModel AccountPartialSyncModel { get; set; } = new();
    public ErpGroupPricePartialSyncModel GroupPricePartialSyncModel { get; set; } = new();
    public ErpInvoicePartialSyncModel InvoicePartialSyncModel { get; set; } = new();
    public ErpProductPartialSyncModel ProductPartialSyncModel { get; set; } = new();
    public ErpShipToAddressPartialSyncModel ShipToAddressPartialSyncModel { get; set; } = new();
    public ErpSpecialPricePartialSyncModel SpecialPricePartialSyncModel { get; set; } = new();
    public ErpStockPartialSyncModel StockPartialSyncModel { get; set; } = new();
    public ErpOrderPartialSyncModel OrderPartialSyncModel { get; set; } = new();
    
    #endregion
}
