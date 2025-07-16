using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Customers;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpAccountCreditSyncFunctionality;

public class ErpAccountCreditSyncFunctionality : IErpAccountCreditSyncFunctionality
{
    #region Fields

    private readonly IErpAccountService _erpAccountService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpSalesOrgService _erpSalesOrgService;

    #endregion

    #region Ctor

    public ErpAccountCreditSyncFunctionality(IErpAccountService erpAccountService,        
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,        
        IErpLogsService erpLogsService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,        
        IErpSalesOrgService erpSalesOrgService)
    {
        _erpAccountService = erpAccountService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpLogsService = erpLogsService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpSalesOrgService = erpSalesOrgService;
    }

    #endregion

    #region Methods

    public async Task LiveErpAccountCreditCheckAsync(ErpAccount erpAccount, Customer customer)
    {
        if (_b2BB2CFeaturesSettings.EnableLiveCreditChecks && erpAccount != null)
        {
            var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

            if (erpIntegrationPlugin is not null)
            {
                var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdWithActiveAsync(erpAccount.ErpSalesOrgId);
                var response = await erpIntegrationPlugin.GetAllAccountCreditFromErpAsync(
                    new ErpGetRequestModel() 
                    { 
                        AccountNumber = erpAccount.AccountNumber,
                        Location = erpSalesOrg?.Code ?? string.Empty
                    }
                );

                if (!response.ErpResponseModel.IsError)
                {
                    if (response.Data is not null)
                    {
                        var data = response.Data?.FirstOrDefault();
                        erpAccount.CreditLimit = data?.CreditLimit ?? erpAccount.CreditLimit;
                        erpAccount.CurrentBalance = data?.CurrentBalance ?? erpAccount.CurrentBalance;
                        erpAccount.CreditLimitAvailable = data?.CreditLimitAvailable ?? erpAccount.CreditLimitAvailable;
                        erpAccount.UpdatedById = 1;
                        erpAccount.UpdatedOnUtc = DateTime.UtcNow;
                        erpAccount.LastErpAccountSyncDate = DateTime.UtcNow;
                        await _erpAccountService.UpdateErpAccountAsync(erpAccount);
                        await _erpLogsService.InformationAsync($"Erp Account {erpAccount.AccountName} ({erpAccount.AccountNumber}) Live Credit synced.", ErpSyncLevel.Account, customer: customer);
                    }
                    else
                    {
                        await _erpLogsService.InformationAsync($"No credit data found for Erp Account {erpAccount.AccountName} ({erpAccount.AccountNumber})", ErpSyncLevel.Account, customer: customer);
                    }
                }
                else
                {
                    await _erpLogsService.ErrorAsync($"Erp Account {erpAccount.AccountName} ({erpAccount.AccountNumber}) Live Credit Check error: {response.ErpResponseModel.ErrorShortMessage}", ErpSyncLevel.Account, customer: customer);
                }
            }
            else
            {
                await _erpLogsService.ErrorAsync($"Erp Account {erpAccount.AccountName} ({erpAccount.AccountNumber}) Live Credit Check error: No integration method found.", ErpSyncLevel.Account, customer: customer);
            }
        }
    }

    #endregion
}