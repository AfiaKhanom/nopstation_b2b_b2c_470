using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice
{
    public class ErpInvoiceRecordModel
    {
        [JsonProperty("export_class")]
        public string? ExportClass { get; set; }

        [JsonProperty("document_number")]
        public string? DocumentNumber { get; set; }

        [JsonProperty("document")]
        public string Document { get; set; }
    }
}
