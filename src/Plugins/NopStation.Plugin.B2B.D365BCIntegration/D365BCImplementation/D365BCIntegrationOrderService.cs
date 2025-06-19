using NopStation.Plugin.B2B.D365BCIntegration.Models;
using NopStation.Plugin.B2B.D365BCIntegration.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.D365BCIntegration.D365BCImplementation;
public class D365BCIntegrationOrderService : ID365BCIntegrationOrderService
{
    private readonly ID365BCService _d365Service;
    private readonly IErpLogsService _erpLogsService;
    private readonly D365BCIntegrationSettings _settings;

    public D365BCIntegrationOrderService(
        ID365BCService d365Service,
        IErpLogsService erpLogsService,
        D365BCIntegrationSettings settings)
    {
        _d365Service = d365Service;
        _erpLogsService = erpLogsService;
        _settings = settings;
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrdersByAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        if (!_d365Service.IsValidD365BCIntegrationSettings(_settings))
            return new ErpResponseData<IList<ErpPlaceOrderDataModel>>
            {
                ErpResponseModel = new ErpResponseModel
                {
                    IsError = true,
                    ErrorShortMessage = "Dynamics 365 Integration Settings is not configured."
                }
            };
        return await _d365Service.GetOrdersByAccountFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpPlaceOrderDataModel>>> GetOrderByOrderNumberFromErpAsync(ErpGetRequestModel erpRequest)
    {
        if (!_d365Service.IsValidD365BCIntegrationSettings(_settings))
            return new ErpResponseData<IList<ErpPlaceOrderDataModel>>
            {
                ErpResponseModel = new ErpResponseModel
                {
                    IsError = true,
                    ErrorShortMessage = "Dynamics 365 Integration Settings is not configured."
                }
            };
        return await _d365Service.GetOrderByOrderNumberFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<D365BCOrderModel>> CreateOrderAsync(ErpPlaceOrderDataModel orderData)
    {
        if (!_d365Service.IsValidD365BCIntegrationSettings(_settings))
            return new ErpResponseData<D365BCOrderModel>
            {
                ErpResponseModel = new ErpResponseModel
                {
                    IsError = true,
                    ErrorShortMessage = "Dynamics 365 Integration Settings is not configured."
                }
            };

        return await _d365Service.CreateOrderAsync(orderData);
    }

}
