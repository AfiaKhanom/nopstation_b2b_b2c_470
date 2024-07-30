using Newtonsoft.Json;

namespace NopStation.Plugin.B2B.IQRetailIntegration.Model.Result.Common
{
    public class IQApiErrorModel
    {
        [JsonProperty("iq_error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("iq_error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty("iq_error_data")]
        public IQApiErrorDataModel IQApiErrorData { get; set; } = new IQApiErrorDataModel();
    }

    public class IQApiErrorDataModel
    {
        [JsonProperty("iq_error_data_items")]
        public IList<IQApiErrorDataItemModel> IQApiErrorDataItems { get; set; } = new List<IQApiErrorDataItemModel>();
    }

    public class IQApiErrorDataItemModel
    {
        [JsonProperty("iq_error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("iq_error_description")]
        public string ErrorDescription { get; set; }

        [JsonProperty("iq_error_extended_data")]
        public IQApiErrorExtendedDataModel ErrorExtendedData { get; set; }
    }

    public class IQApiErrorExtendedDataModel
    {
        [JsonProperty("iq_root_json")]
        public IQRootJsonErrorModel IQRootJsonErrorModel { get; set; }
    }

    public class IQRootJsonErrorModel
    {
        [JsonProperty("error_data")]
        public IList<IQApiAllErrorData> IQApiAllErrorDatas { get; set; } = new List<IQApiAllErrorData>();
    }

    public class IQApiAllErrorData
    {
        [JsonProperty("errors")]
        public IList<IQApiPerErrorData> IQApiPerErrorDatas { get; set; } = new List<IQApiPerErrorData>();

        [JsonProperty("items")]
        public IList<SalesOrderItem> SalesOrderItems { get; set; } = new List<SalesOrderItem>();
    }

    public class SalesOrderItem
    {
        [JsonProperty("errors")]
        public IList<IQApiPerErrorData> IQApiPerErrorDatas { get; set; } = new List<IQApiPerErrorData>();
    }

    public class IQApiPerErrorData
    {
        [JsonProperty("error_code")]
        public int ErrorCode { get; set; }

        [JsonProperty("error_description")]
        public string ErrorDescription { get; set; }
    }
}