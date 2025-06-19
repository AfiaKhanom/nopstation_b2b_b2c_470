using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.D365BCIntegration.Services;

namespace NopStation.Plugin.B2B.D365BCIntegration.D365BCImplementation;

public class D365BCIntegrationAccountService : ID365BCIntegrationAccountService
{
    #region Fields

    private readonly ID365BCService _d365Service;
    private readonly IErpLogsService _erpLogsService;
    private readonly D365BCIntegrationSettings _settings;

    #endregion

    #region Ctor

    public D365BCIntegrationAccountService(
        ID365BCService d365Service,
        IErpLogsService erpLogsService,
        D365BCIntegrationSettings settings)
    {
        _d365Service = d365Service;
        _erpLogsService = erpLogsService;
        _settings = settings;
    }

    #endregion

    #region Methods

    public async Task<ErpResponseModel> CreateAccountNoErpAsync(ErpCreateAccountModel erpCreateAccountModel)
    {
        // D365 doesn't support customer creation through API in this implementation
        var erpResponseModel = new ErpResponseModel
        {
            IsError = true,
            ErrorShortMessage = "Creating customers in Dynamics 365 is not supported"
        };
        return erpResponseModel;
    }

    public async Task<ErpResponseData<ErpAccountDataModel>> GetAccountFromErpAsync(ErpGetRequestModel erpRequest)
    {
        if (!_d365Service.IsValidD365BCIntegrationSettings(_settings))
            return new ErpResponseData<ErpAccountDataModel>
            {
                ErpResponseModel = new ErpResponseModel
                {
                    IsError = true,
                    ErrorShortMessage = "Dynamics 365 Integration Settings is not configured."
                }
            };

        return await _d365Service.GetCustomerByAccountNumberFromErpAsync(erpRequest);
    }

    public async Task<ErpResponseData<IList<ErpAccountDataModel>>> GetAccountsFromErpAsync(ErpGetRequestModel erpRequest)
    {
        if (!_d365Service.IsValidD365BCIntegrationSettings(_settings))
            return new ErpResponseData<IList<ErpAccountDataModel>>
            {
                ErpResponseModel = new ErpResponseModel
                {
                    IsError = true,
                    ErrorShortMessage = "Dynamics 365 Integration Settings is not configured."
                }
            };
        return await _d365Service.GetCustomersFromErpAsync(erpRequest);
    }

    #endregion
}