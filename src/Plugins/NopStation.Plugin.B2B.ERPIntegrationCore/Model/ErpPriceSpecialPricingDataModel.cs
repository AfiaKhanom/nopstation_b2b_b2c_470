namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model
{
    public class ErpPriceSpecialPricingDataModel
    {
        public string AccNo { get; set; }
        public string Branch { get; set; }
        public string ItemNo { get; set; }
        public decimal? AccountPrice { get; set; }
        public decimal? SellingPrice { get; set; }
        public decimal? PromoPrice { get; set; }
        public decimal? ListPrice { get; set; }
        public decimal? RetailPrice { get; set; }
        public decimal? DiscountPerc { get; set; }
        public string PricingNotes { get; set; }
        //public decimal InStockforLocNo { get; set; }
        //public decimal TotalOnHand { get; set; }
    }
}
