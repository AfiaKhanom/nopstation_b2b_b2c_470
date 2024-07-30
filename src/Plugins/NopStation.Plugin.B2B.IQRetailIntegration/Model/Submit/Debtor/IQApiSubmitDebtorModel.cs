using Newtonsoft.Json;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Submit.Debtor
{
    public class IQApiSubmitDebtorData : IQApiSubmitModel
    {
        [JsonProperty("IQ_Submit_Data")]
        public IQApiResultDataModel IQSubmitData { get; set; } = new IQApiResultDataModel();
    }

    public class IQApiSubmitDebtor
    {
        [JsonProperty("IQ_API_Submit_Debtor")]
        public IQApiSubmitDebtorData IQApiSubmitDebtorData { get; set; }
    }

    public class IQApiSubmitDebtorModel
    {
        [JsonProperty("IQ_API")]
        public IQApiSubmitDebtor IQApi { get; set; }
    }
}
