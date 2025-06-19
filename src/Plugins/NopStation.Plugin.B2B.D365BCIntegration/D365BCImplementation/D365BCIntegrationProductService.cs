using NopStation.Plugin.B2B.D365BCIntegration.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.D365BCIntegration.D365BCImplementation;
public class D365BCIntegrationProductService : ID365BCIntegrationProductService
{
    private readonly ID365BCService _d365Service;
    private readonly IErpLogsService _erpLogsService;
    private readonly D365BCIntegrationSettings _settings;

    public D365BCIntegrationProductService(
        ID365BCService d365Service,
        IErpLogsService erpLogsService,
        D365BCIntegrationSettings settings)
    {
        _d365Service = d365Service;
        _erpLogsService = erpLogsService;
        _settings = settings;
    }

    public async Task<ErpResponseData<ErpProductDataModel>> GetProductByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        if (!_d365Service.IsValidD365BCIntegrationSettings(_settings))
            return new ErpResponseData<ErpProductDataModel>
            {
                ErpResponseModel = new ErpResponseModel
                {
                    IsError = true,
                    ErrorShortMessage = "Dynamics 365 Integration Settings is not configured."
                }
            };
        return await _d365Service.GetProductByItemNoFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpProductDataModel>>> GetProductsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        if (!_d365Service.IsValidD365BCIntegrationSettings(_settings))
            return new ErpResponseData<IList<ErpProductDataModel>>
            {
                ErpResponseModel = new ErpResponseModel
                {
                    IsError = true,
                    ErrorShortMessage = "Dynamics 365 Integration Settings is not configured."
                }
            };
        return await _d365Service.GetProductsFromErpAsync(erpRequest);
    }
}
