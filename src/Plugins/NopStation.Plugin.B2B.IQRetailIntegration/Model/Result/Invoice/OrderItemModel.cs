using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice
{
    public class OrderItemModel
    {
        [JsonProperty("stock_code")]
        public string StockCode { get; set; }

        [JsonProperty("stock_description")]
        public string StockDescription { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("quantity")]
        public decimal Quantity { get; set; }

        [JsonProperty("volumetrics")]
        public VolumetricModel Volumetric { get; set; }

        [JsonProperty("discount_percentage")]
        public decimal DiscountPercentage { get; set; }

        [JsonProperty("line_total_inclusive")]
        public decimal LineTotalInclusive { get; set; }

        [JsonProperty("line_total_exclusive")]
        public decimal LineTotalExclusive { get; set; }

        [JsonProperty("list_price")]
        public decimal ListPrice { get; set; }
    }
}
