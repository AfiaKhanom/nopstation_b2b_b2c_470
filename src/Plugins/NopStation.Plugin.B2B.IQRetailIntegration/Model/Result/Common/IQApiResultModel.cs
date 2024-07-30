using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common
{
    public class IQApiResultModel
    {
        [JsonProperty("iq_api_error")]
        public IList<IQApiErrorModel> IQApiErrors { get; set; } = new List<IQApiErrorModel>();

        [JsonProperty("iq_page_data")]
        public IQApiPageDataModel IQApiPageData { get; set; } = new IQApiPageDataModel();

        [JsonProperty("iq_api_result_data")]
        public IQApiResultDataModel IQApiResultDataModel { get; set; } = new IQApiResultDataModel();

        [JsonProperty("iq_api_success")]
        public IQApiSuccessModel IQApiSuccess { get; set; } = new IQApiSuccessModel();
    }
}