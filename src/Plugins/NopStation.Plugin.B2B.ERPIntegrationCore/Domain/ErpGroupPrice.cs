namespace NopStation.Plugin.B2B.ERPIntegrationCore.Domain;

public partial class ErpGroupPrice : ErpBaseEntity
{
    public int ErpNopGroupPriceCodeId { get; set; }

    public int NopProductId { get; set; }

    public decimal Price { get; set; }
}
