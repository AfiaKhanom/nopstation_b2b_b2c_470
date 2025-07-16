namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpPlaceOrderItemDataModel
{
    public string Sku { get; set; }
    public string BatchCode { get; set; }
    public string Description { get; set; }
    public decimal? Quantity { get; set; }
    public string UnitOfMeasure { get; set; }
    public string SpecialInstruction { get; set; }
    public decimal? UnitPriceExclTax { get; set; }
    public decimal? UnitPriceInclTax { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public decimal? DiscountAmountInclTax { get; set; }
    public decimal? DiscountAmountExclTax { get; set; }
    public decimal? PriceExclTax { get; set; }
    public decimal? PriceInclTax { get; set; }
}
