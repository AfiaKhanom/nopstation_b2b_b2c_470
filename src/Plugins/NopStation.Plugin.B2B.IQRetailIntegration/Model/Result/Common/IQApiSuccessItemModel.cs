using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common
{
    public class IQApiSuccessItemModel
    {
        [JsonProperty("fieldname")]
        public string FieldName { get; set; }

        [JsonProperty("data")]
        public string Data { get; set; }
    }
}
