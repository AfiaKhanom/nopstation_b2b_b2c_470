using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice
{
    public class IQApiResultDataInvoicePdfModel
    {
        [JsonProperty("iq_root_json")]
        public IQRootJsonInvoicePdfModel IQRootJsonForInvoicePdf { get; set; } = new IQRootJsonInvoicePdfModel();
    }
}