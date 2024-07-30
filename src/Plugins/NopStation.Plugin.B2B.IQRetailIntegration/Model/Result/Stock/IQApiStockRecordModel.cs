using Newtonsoft.Json;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Stock
{
    public class IQApiStockRecordModel
    {
        [JsonProperty("iq_api_error")]
        public IList<IQApiErrorModel> IQApiErrors { get; set; } = new List<IQApiErrorModel>();

        [JsonProperty("iq_api_result_data")]
        public StockRecordModel StockRecordModel { get; set; } = new StockRecordModel();
    }
}