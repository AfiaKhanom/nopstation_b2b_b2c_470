using Newtonsoft.Json;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice
{
    public class IQRootJsonInvoicePdfModel
    {
        [JsonProperty("iq_identification_info")]
        public IQIdentificationInfoModel IQIdentificationInfo { get; set; } = new IQIdentificationInfoModel();

        [JsonProperty("processing_documents")]
        public IList<ErpInvoiceRecordModel> ErpInvoicePdfByteCodes { get; set; } = new List<ErpInvoiceRecordModel>();
    }
}
