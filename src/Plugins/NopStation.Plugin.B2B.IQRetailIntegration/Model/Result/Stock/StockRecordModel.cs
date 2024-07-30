using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Stock
{
    public class StockRecordModel
    {
        [JsonProperty("records")]
        public IList<ErpStockRecordModel> ErpStockRecords { get; set; } = new List<ErpStockRecordModel>();
    }
}
