using Nop.Core;
using NopStation.Plugin.B2B.D365BCIntegration.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.D365BCIntegration.D365BCImplementation;

public class D365BCIntegrationStockService : ID365BCIntegrationStockService
{
    #region Fields

    private readonly ID365BCService _d365Service;
    private readonly D365BCIntegrationSettings _settings;
    private readonly IErpLogsService _erpLogsService;
    private readonly IWorkContext _workContext;

    #endregion

    #region Ctor

    public D365BCIntegrationStockService(
        ID365BCService d365Service,
        D365BCIntegrationSettings settings,
        IErpLogsService erpLogsService,
        IWorkContext workContext)
    {
        _d365Service = d365Service;
        _settings = settings;
        _erpLogsService = erpLogsService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public async Task<ErpResponseData<ErpStockDataModel>> GetStockByItemNoFromErpAsync(ErpGetRequestModel erpRequest)
    {
        if (!_d365Service.IsValidD365BCIntegrationSettings(_settings))
            return new ErpResponseData<ErpStockDataModel>
            {
                ErpResponseModel = new ErpResponseModel
                {
                    IsError = true,
                    ErrorShortMessage = "Dynamics 365 Integration Settings is not configured."
                }
            };

        return await _d365Service.GetStockByItemNoFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpStockDataModel>>> GetStocksFromErpAsync(ErpGetRequestModel erpRequest)
    {
        if (!_d365Service.IsValidD365BCIntegrationSettings(_settings))
            return new ErpResponseData<IList<ErpStockDataModel>>
            {
                ErpResponseModel = new ErpResponseModel
                {
                    IsError = true,
                    ErrorShortMessage = "Dynamics 365 Integration Settings is not configured."
                }
            };

        return await _d365Service.GetStocksFromErpAsync(erpRequest);
    }

    #endregion
}
