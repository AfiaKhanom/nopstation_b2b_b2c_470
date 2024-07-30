using Newtonsoft.Json;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice
{
    public class IQApiResultInvoicePdfModel
    {
        [JsonProperty("iq_api_error")]
        public IList<IQApiErrorModel> IQApiErrors { get; set; } = new List<IQApiErrorModel>();

        [JsonProperty("iq_page_data")]
        public IQApiPageDataModel IQApiPageData { get; set; } = new IQApiPageDataModel();

        [JsonProperty("iq_api_result_data")]
        public IQApiResultDataInvoicePdfModel InvoicePdfModel { get; set; } = new IQApiResultDataInvoicePdfModel();
    }
}
