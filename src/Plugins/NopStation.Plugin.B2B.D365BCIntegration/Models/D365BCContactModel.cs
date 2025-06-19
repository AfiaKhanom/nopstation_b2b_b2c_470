using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.D365BCIntegration.Models
{
    public class D365CustomersResponse
    {
        [JsonProperty("@odata.context")]
        public string Context { get; set; }

        [JsonProperty("value")]
        public List<D365Customer> Value { get; set; }
    }

    public class D365Customer
    {
        [JsonProperty("No")]
        public string No { get; set; }

        [JsonProperty("Name")]
        public string Name { get; set; }

        [JsonProperty("Address")]
        public string Address { get; set; }

        [JsonProperty("Address_2")]
        public string Address_2 { get; set; }

        [JsonProperty("City")]
        public string City { get; set; }

        [JsonProperty("County")]
        public string County { get; set; }

        [JsonProperty("Post_Code")]
        public string Post_Code { get; set; }

        [JsonProperty("Country_Region_Code")]
        public string Country_Region_Code { get; set; }

        [JsonProperty("Phone_No")]
        public string Phone_No { get; set; }

        [JsonProperty("E_Mail")]
        public string E_Mail { get; set; }

        [JsonProperty("Registration_Number")]
        public string Registration_Number { get; set; }

        [JsonProperty("VAT_Registration_No")]
        public string VAT_Registration_No { get; set; }

        [JsonProperty("Credit_Limit_LCY")]
        public decimal Credit_Limit_LCY { get; set; }
    }

    public class D365CountResponse
    {
        [JsonProperty("@odata.context")]
        public string Context { get; set; }

        [JsonProperty("@odata.count")]
        public int Count { get; set; }

        [JsonProperty("value")]
        public object[] Value { get; set; }
    }
}