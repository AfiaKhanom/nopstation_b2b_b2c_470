using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common
{
    public class IQIdentificationInfoModel
    {
        [JsonProperty("company_store_id")]
        public string CompanyStoreId { get; set; }

        [JsonProperty("company_code")]
        public string CompanyCode { get; set; }

        [JsonProperty("company_name")]
        public string? CompanyName { get; set; }

        [JsonProperty("company_address1")]
        public string? CompanyAddress1 { get; set; }

        [JsonProperty("company_telephone1")]
        public string? CompanyTelephone1 { get; set; }

        [JsonProperty("company_fax")]
        public string? CompanyFax { get; set; }

        [JsonProperty("company_email")]
        public string? CompanyEmail { get; set; }

        [JsonProperty("company_tax")]
        public string? CompanyTax { get; set; }

        [JsonProperty("company_registration_Number")]
        public string? CompanyRegistrationNumber { get; set; }
    }
}
