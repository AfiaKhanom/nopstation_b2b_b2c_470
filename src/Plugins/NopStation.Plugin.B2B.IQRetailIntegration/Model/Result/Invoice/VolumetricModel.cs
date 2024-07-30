using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice
{
    public class VolumetricModel
    {
        [JsonProperty("units")]
        public decimal Units { get; set; }
    }
}
