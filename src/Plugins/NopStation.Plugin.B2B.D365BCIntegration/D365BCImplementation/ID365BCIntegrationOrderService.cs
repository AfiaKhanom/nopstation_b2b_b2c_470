using NopStation.Plugin.B2B.D365BCIntegration.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.D365BCIntegration.D365BCImplementation;
public interface ID365BCIntegrationOrderService
{
    Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrdersByAccountFromErpAsync(ErpGetRequestModel erpRequest);
    Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByOrderNumberFromErpAsync(ErpGetRequestModel erpRequest);
    Task<ErpResponseData<D365BCOrderModel>> CreateOrderAsync(ErpPlaceOrderDataModel orderData);
}
