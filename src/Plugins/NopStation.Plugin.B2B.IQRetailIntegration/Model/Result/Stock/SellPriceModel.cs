using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Stock
{
    public class SellPriceModel
    {
        [JsonProperty("inclusive")]
        public decimal Inclusive { get; set; }
    }
}
