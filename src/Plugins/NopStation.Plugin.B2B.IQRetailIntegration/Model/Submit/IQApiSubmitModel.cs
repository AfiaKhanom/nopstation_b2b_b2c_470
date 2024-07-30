using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Submit
{
    public class IQApiSubmitModel
    {
        [JsonProperty("IQ_Company_Number")]
        public string IQCompanyNumber { get; set; }

        [JsonProperty("IQ_Terminal_Number")]
        public int IQTerminalNumber { get; set; }

        [JsonProperty("IQ_User_Number")]
        public int IQUserNumber { get; set; }

        [JsonProperty("IQ_User_Password")]
        public string IQUserPassword { get; set; }

        [JsonProperty("IQ_Partner_Passphrase")]
        public string? IQPartnerPassphrase { get; set; }

        [JsonProperty("IQ_SQL_Text")]
        public string? IQSqlText { get; set; }

        [JsonProperty("Filter_Type")]
        public string? FilterType { get; set; }

        [JsonProperty("Text_Filter")]
        public string? TextFilter { get; set; }

        [JsonProperty("record_limit")]
        public int? RecordLimit { get; set; }

        [JsonProperty("record_Offset")]
        public int? RecordOffset { get; set; }
    }
}
