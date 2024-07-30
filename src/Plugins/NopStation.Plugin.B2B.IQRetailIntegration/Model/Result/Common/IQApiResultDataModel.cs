using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common
{
    public class IQApiResultDataModel
    {
        [JsonProperty("iq_root_json")]
        public IQRootJsonModel IQRootJson { get; set; } = new IQRootJsonModel();
    }
}
