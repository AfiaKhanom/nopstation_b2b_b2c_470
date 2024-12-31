namespace NopStation.Plugin.B2B.ERPIntegrationCore.Model;

public class ErpResponseData<T> 
{
    public ErpResponseData() 
    {
        ErpResponseModel = new ErpResponseModel();
    }

    public ErpResponseModel ErpResponseModel { get; set; }
    public T Data { get; set; }
}

public class ErpResponseModel
{
    public string AccountNumber { get; set; }
    public string OrderNumber { get; set; }
    public bool IsError { get; set; } = false;
    public string ErrorShortMessage { get; set; } = string.Empty;
    public string ErrorFullMessage { get; set; } = string.Empty;
    public string Next { get; set; }
    public string StatusCode { get; set; }
}
