using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Stock
{
    public class ErpStockRecordModel
    {
        [JsonProperty("code")]
        public string Code { get; set; }

        [JsonProperty("descript")]
        public string Description { get; set; }

        [JsonProperty("alt_descript")]
        public string AlternativeDescription { get; set; }

        [JsonProperty("sellprice1")]
        public decimal SellPrice1 { get; set; }

        [JsonProperty("sellprice2")]
        public decimal SellPrice2 { get; set; }

        [JsonProperty("sellprice3")]
        public decimal SellPrice3 { get; set; }

        [JsonProperty("sellprice4")]
        public decimal SellPrice4 { get; set; }

        [JsonProperty("sellprice5")]
        public decimal SellPrice5 { get; set; }

        [JsonProperty("sellprice6")]
        public decimal SellPrice6 { get; set; }

        [JsonProperty("sellprice7")]
        public decimal SellPrice7 { get; set; }

        [JsonProperty("sellprice8")]
        public decimal SellPrice8 { get; set; }

        [JsonProperty("sellprice9")]
        public decimal SellPrice9 { get; set; }

        [JsonProperty("sellprice10")]
        public decimal SellPrice10 { get; set; }

        [JsonProperty("vatrate")]
        public string VatRate { get; set; }

        [JsonProperty("notes")]
        public string Notes { get; set; }

        [JsonProperty("maxdiscount")]
        public decimal MaximumDiscount { get; set; }

        [JsonProperty("onhand")]
        public decimal OnHand { get; set; }

        [JsonProperty("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonProperty("departmentname")]
        public string DepartmentName { get; set; }

        [JsonProperty("groupname")]
        public string GroupName { get; set; }

        [JsonProperty("categoryname")]
        public string CategoryName { get; set; }

        [JsonProperty("colour")]
        public string Colour { get; set; }

        [JsonProperty("size")]
        public string Size { get; set; }

        [JsonProperty("brandname")]
        public string BrandName { get; set; }
    }
}
