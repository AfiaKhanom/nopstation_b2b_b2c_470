using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Stock
{
    public class StockMasterModel
    {
        [JsonProperty("stock_code")]
        public string StockCode { get; set; }

        [JsonProperty("description")]
        public string Description { get; set; }

        [JsonProperty("alternative_description")]
        public string AlternativeDescription { get; set; }

        [JsonProperty("category")]
        public string Category { get; set; }

        [JsonProperty("sell_prices")]
        public List<SellPriceModel> SellPrices { get; set; }

        [JsonProperty("vat_rate")]
        public string VatRate { get; set; }

        [JsonProperty("new_prices")]
        public List<decimal> NewPrices { get; set; }

        [JsonProperty("extended_description")]
        public string ExtendedDescription { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("maximum_discount")]
        public decimal MaximumDiscount { get; set; }

        [JsonProperty("unit_of_measure")]
        public string UnitOfMeasure { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }
    }
}
