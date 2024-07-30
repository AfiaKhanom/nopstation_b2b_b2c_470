using Newtonsoft.Json;
using NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Submit.SalesOrder
{
    public class IQApiSubmitDocumentSalesOrderData : IQApiSubmitModel
    {
        [JsonProperty("IQ_Submit_Data")]
        public IQApiResultDataModel IQSubmitData { get; set; } = new IQApiResultDataModel();
    }

    public class IQApiSubmitDocumentSalesOrder
    {
        [JsonProperty("IQ_API_Submit_Document_Sales_Order")]
        public IQApiSubmitDocumentSalesOrderData IQApiSubmitDocumentSalesOrderData { get; set; }
    }

    public class IQApiSubmitDocumentSalesOrderModel
    {
        [JsonProperty("IQ_API")]
        public IQApiSubmitDocumentSalesOrder IQApi { get; set; }
    }
}