using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice
{
    public class ProcessingDocumentModel
    {
        [JsonProperty("export_class")]
        public string ExportClass { get; set; }

        [JsonProperty("document")]
        public DocumentModel Document { get; set; }

        [JsonProperty("items")]
        public List<OrderItemModel> Items { get; set; }
    }
}
