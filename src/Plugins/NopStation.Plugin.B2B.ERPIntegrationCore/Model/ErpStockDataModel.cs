using System;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpStockDataModel
{
    public string WarehouseNameOrCode { get; set; }
    public string Sku { get; set; }
    public string SalesOrgCode { get; set; }
    public decimal? QuantityOnHand { get; set; }
    public DateTime? LastChangedDate { get; set; }
}
