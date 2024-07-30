using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice
{
    public class OrderInformationModel
    {
        [JsonProperty("order_date")]
        public string OrderDate { get; set; }

        [JsonProperty("expected_date")]
        public string ExpectedDate { get; set; }
    }
}
