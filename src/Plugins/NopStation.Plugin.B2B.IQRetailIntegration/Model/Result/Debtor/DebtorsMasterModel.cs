using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Debtor
{
    public class DebtorsMasterModel
    {
        [JsonProperty("postal_address_details")]
        public List<string> PostalAddressDetails { get; set; }

        [JsonProperty("delivery_address_details")]
        public List<string> DeliveryAddressDetails { get; set; }

        [JsonProperty("additional_addresses")]
        public List<AdditionalAddressModel>? AdditionalAddresses { get; set; }

        [JsonProperty("credit_limit")]
        public decimal CreditLimit { get; set; }

        [JsonProperty("telephone_numbers")]
        public List<string> TelephoneNumbers { get; set; }

        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }

        [JsonProperty("fax_number")]
        public string FaxNumber { get; set; }

        [JsonProperty("tax_number")]
        public string TaxNumber { get; set; }

        [JsonProperty("area")]
        public string Area { get; set; }

        [JsonProperty("balance_current")]
        public decimal BalanceCurrent { get; set; }

        [JsonProperty("company_registration_number")]
        public string CompanyRegistrationNumber { get; set; }

        [JsonProperty("debtor_account")]
        public string DebtorAccount { get; set; }

        [JsonProperty("debtor_name")]
        public string DebtorName { get; set; }

        [JsonProperty("debtor_sub_group")]
        public string DebtorSubGroup { get; set; }

        [JsonProperty("terms")]
        public string Terms { get; set; }

        [JsonProperty("delivery_route")]
        public string DeliveryRoute { get; set; }

        [JsonProperty("credit_limit_insured")]
        public decimal CreditLimitInsured { get; set; }

        [JsonProperty("credit_limit_reserved")]
        public decimal CreditLimitReserved { get; set; }

        [JsonProperty("preferred_sell_price")]
        public string PreferredSellPrice { get; set; }

        [JsonProperty("normal_representative")]
        public decimal NormalRepresentative { get; set; }
    }
}
