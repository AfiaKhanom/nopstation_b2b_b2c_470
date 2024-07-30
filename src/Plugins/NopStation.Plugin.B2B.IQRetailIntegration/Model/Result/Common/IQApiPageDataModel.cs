using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common
{
    public class IQApiPageDataModel
    {
        [JsonProperty("next_offset")]
        public int NextOffset { get; set; }

        [JsonProperty("record_count")]
        public int RecordCount { get; set; }
    }
}
