using Newtonsoft.Json;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Debtor;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Invoice;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Stock;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common
{
    public class IQRootJsonModel
    {
        [JsonProperty("iq_identification_info")]
        public IQIdentificationInfoModel IQIdentificationInfo { get; set; } = new IQIdentificationInfoModel();

        [JsonProperty("debtors_master")]
        public IList<DebtorsMasterModel> DebtorsMasters { get; set; } = new List<DebtorsMasterModel>();

        [JsonProperty("processing_documents")]
        public IList<ProcessingDocumentModel> ProcessingDocuments { get; set; } = new List<ProcessingDocumentModel>();

        [JsonProperty("stock_master")]
        public IList<StockMasterModel> StockMasters { get; set; } = new List<StockMasterModel>();
    }
}
