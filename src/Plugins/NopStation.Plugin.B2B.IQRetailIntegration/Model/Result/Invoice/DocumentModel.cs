using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice
{
    public class DocumentModel
    {
        [JsonProperty("document_number")]
        public string DocumentNumber { get; set; }

        [JsonProperty("document_reference")]
        public string DocumentReference { get; set; }

        [JsonProperty("delivery_address_information")]
        public List<string> DeliveryAddressInformation { get; set; }

        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }

        [JsonProperty("order_number")]
        public string OrderNumber { get; set; }

        [JsonProperty("delivery_method")]
        public string DeliveryMethod { get; set; }

        [JsonProperty("delivery_note_number")]
        public string DeliveryNoteNumber { get; set; }

        [JsonProperty("total_vat")]
        public decimal TotalVat { get; set; }

        [JsonProperty("document_total")]
        public decimal DocumentTotal { get; set; }

        [JsonProperty("currency")]
        public string Currency { get; set; }

        [JsonProperty("store_department")]
        public string? StoreDepartment { get; set; }

        [JsonProperty("vat_number")]
        public string VatNumber { get; set; }

        [JsonProperty("delivery_route")]
        public string? DeliveryRoute { get; set; }

        [JsonProperty("debtor_account")]
        public string DebtorAccount { get; set; }

        [JsonProperty("sales_representative_number")]
        public decimal SalesRepresentativeNumber { get; set; }

        [JsonProperty("invoice_date")]
        public DateTime InvoiceDate { get; set; }

        [JsonProperty("date_picked_on")]
        public DateTime DatePickedOn { get; set; }

        [JsonProperty("order_information")]
        public OrderInformationModel OrderInformation { get; set; }
    }
}
