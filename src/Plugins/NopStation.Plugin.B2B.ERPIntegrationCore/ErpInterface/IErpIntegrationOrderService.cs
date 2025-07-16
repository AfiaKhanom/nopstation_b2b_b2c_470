using System.Collections.Generic;
using System.Threading.Tasks;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;

namespace NopStation.Plugin.B2B.ERPIntegrationCore.ErpInterface;

public interface IErpIntegrationOrderService
{
    Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByAccountFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByOrderNumberFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByAccountFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetQuoteByQuoteNumberFromErpAsync(ErpGetRequestModel erpRequest);

    Task<ErpResponseModel> CreateOrderOnErpAsync(ErpPlaceOrderDataModel erpRequest);
}
