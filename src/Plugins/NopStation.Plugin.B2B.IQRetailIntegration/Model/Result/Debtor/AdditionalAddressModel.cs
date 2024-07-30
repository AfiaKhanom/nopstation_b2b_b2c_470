using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Debtor
{
    public class AdditionalAddressModel
    {
        [JsonProperty("branch_number")]
        public string BranchNumber { get; set; }

        [JsonProperty("address1")]
        public string Address1 { get; set; }

        [JsonProperty("address2")]
        public string Address2 { get; set; }

        [JsonProperty("address3")]
        public string Address3 { get; set; }

        [JsonProperty("address4")]
        public string Address4 { get; set; }

        [JsonProperty("email_address")]
        public string EmailAddress { get; set; }
    }
}
