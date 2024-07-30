using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice
{
    public class InvoiceRecordModel
    {
        [JsonProperty("records")]
        public IList<ErpInvoiceRecordModel> ErpInvoiceRecords { get; set; } = new List<ErpInvoiceRecordModel>();
    }
}
