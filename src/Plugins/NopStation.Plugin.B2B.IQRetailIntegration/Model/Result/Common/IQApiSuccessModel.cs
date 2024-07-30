using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common
{
    public class IQApiSuccessModel
    {
        [JsonProperty("iq_api_success_items")]
        public IList<List<IQApiSuccessItemModel>> IQApiSuccessItems { get; set; } = new List<List<IQApiSuccessItemModel>>();
    }
}
