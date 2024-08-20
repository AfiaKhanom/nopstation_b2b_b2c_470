namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpPriceSpecialPricingDataModel
{
    public string AccountNumber { get; set; }
    public string Branch { get; set; }
    public string Sku { get; set; }
    public decimal? SpecialPrice { get; set; }
    public decimal? SellingPrice { get; set; }
    public decimal? PromoPrice { get; set; }
    public decimal? ListPrice { get; set; }
    public decimal? RetailPrice { get; set; }
    public decimal? DiscountPercentage { get; set; }
    public string PricingNotes { get; set; }
}
