using NopStation.Plugin.B2B.D365BCIntegration.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.D365BCIntegration.Services;

public interface ID365BCService
{
    /// <summary>
    /// Validates if the D365 integration settings are properly configured
    /// </summary>
    bool IsValidD365BCIntegrationSettings(D365BCIntegrationSettings settings);

    /// <summary>
    /// Gets customer data from Dynamic 365
    /// </summary>
    Task<ErpResponseData<IList<ErpAccountDataModel>>> GetCustomersFromErpAsync(ErpGetRequestModel erpRequest);

    /// <summary>
    /// Gets a single customer by account number from Dynamic 365
    /// </summary>
    Task<ErpResponseData<ErpAccountDataModel>> GetCustomerByAccountNumberFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<ErpProductDataModel>> GetProductByItemNoFromErpAsync(ErpGetRequestModel erpRequest);
    Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<ErpStockDataModel>> GetStockByItemNoFromErpAsync(ErpGetRequestModel erpRequest);
    Task<ErpResponseData<IList<ErpStockDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrdersByAccountFromErpAsync(ErpGetRequestModel erpRequest);
    Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByOrderNumberFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<D365BCOrderModel>> CreateOrderAsync(ErpPlaceOrderDataModel orderData);


}