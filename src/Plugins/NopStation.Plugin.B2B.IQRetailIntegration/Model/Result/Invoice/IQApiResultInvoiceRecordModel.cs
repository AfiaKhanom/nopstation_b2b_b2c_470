using Newtonsoft.Json;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice
{
    public class IQApiResultInvoiceRecordModel
    {
        [JsonProperty("iq_api_error")]
        public IList<IQApiErrorModel> IQApiErrors { get; set; } = new List<IQApiErrorModel>();

        [JsonProperty("iq_api_result_data")]
        public InvoiceRecordModel InvoiceRecordModel { get; set; } = new InvoiceRecordModel();
    }
}
