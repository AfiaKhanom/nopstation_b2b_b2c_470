using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Payments.B2BAccount.Models;

namespace NopStation.Plugin.Payments.B2BAccount.Components;

public class B2BAccountPaymentViewComponent : NopViewComponent
{
    #region Fields

    private readonly IErpNopUserService _erpNopUserService;
    private readonly IWorkContext _workContext;
    private readonly IErpAccountService _erpAccountService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpSalesOrgService _erpSalesOrgService;

    #endregion

    #region Ctor

    public B2BAccountPaymentViewComponent(IErpNopUserService erpNopUserService,
        IWorkContext workContext,
        IErpAccountService erpAccountService,
        IPriceFormatter priceFormatter,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpLogsService erpLogsService,
        IErpSalesOrgService erpSalesOrgService)
    {
        _erpNopUserService = erpNopUserService;
        _workContext = workContext;
        _erpAccountService = erpAccountService;
        _priceFormatter = priceFormatter;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpLogsService = erpLogsService;
        _erpSalesOrgService = erpSalesOrgService;
    }

    #endregion

    #region Utilities

    private async Task LiveErpAccountCreditCheckAsync()
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        var nopErpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);
        var b2BAccount = await _erpAccountService.GetErpAccountByIdAsync(nopErpUser?.ErpAccountId ?? 0);

        if (nopErpUser != null && nopErpUser.ErpUserType == ErpUserType.B2BUser && _b2BB2CFeaturesSettings.EnableLiveCreditChecks && b2BAccount != null)
        {
            var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

            if (erpIntegrationPlugin is not null)
            {
                var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(b2BAccount.ErpSalesOrgId);
                var response = await erpIntegrationPlugin.GetAllAccountCreditFromErpAsync(
                    new ErpGetRequestModel()
                    {
                        Location = erpSalesOrg.Code,
                        AccountNumber = b2BAccount.AccountNumber
                    }
                );
                if (!response.ErpResponseModel.IsError)
                {
                    if (response.Data is not null)
                    {
                        var data = response.Data?.FirstOrDefault();
                        b2BAccount.CreditLimit = data?.CreditLimit ?? b2BAccount.CreditLimit;
                        b2BAccount.CurrentBalance = data?.CurrentBalance ?? b2BAccount.CurrentBalance;
                        b2BAccount.CreditLimitAvailable = data?.CreditLimitAvailable ?? b2BAccount.CreditLimitAvailable;
                        b2BAccount.UpdatedById = 1;
                        b2BAccount.UpdatedOnUtc = DateTime.UtcNow;
                        b2BAccount.LastErpAccountSyncDate = DateTime.UtcNow;
                        await _erpAccountService.UpdateErpAccountAsync(b2BAccount);
                        await _erpLogsService.InformationAsync($"Erp Account {b2BAccount.AccountName} ({b2BAccount.AccountNumber}) Live Credit synced.", ErpSyncLevel.Account, customer: customer);
                    }
                    else
                    {
                        await _erpLogsService.InformationAsync($"No credit data found for Erp Account {b2BAccount.AccountName} ({b2BAccount.AccountNumber})", ErpSyncLevel.Account, customer: customer);
                    }
                }
                else
                {
                    await _erpLogsService.ErrorAsync($"Erp Account {b2BAccount.AccountName} ({b2BAccount.AccountNumber}) Live Credit Check error: {response.ErpResponseModel.ErrorShortMessage}", ErpSyncLevel.Account, customer: customer);
                }
            }
            else
            {
                await _erpLogsService.ErrorAsync($"Erp Account {b2BAccount.AccountName} ({b2BAccount.AccountNumber}) Live Credit Check error: No integration method found.", ErpSyncLevel.Account, customer: customer);
            }
        }
    }

    #endregion

    #region Methods

    public async Task<IViewComponentResult> InvokeAsync()
    {
        var model = new B2BAccountPaymentInfoModel();

        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
            return Content(string.Empty);

        var nopErpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);
        if (nopErpUser == null || nopErpUser.ErpUserType != ErpUserType.B2BUser)
            return Content(string.Empty);

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(nopErpUser.ErpAccountId);
        if (erpAccount == null)
            return Content(string.Empty);

        await LiveErpAccountCreditCheckAsync();

        model.CreditLimitAvailableStr = await _priceFormatter.FormatPriceAsync(erpAccount.CreditLimitAvailable, true, false);
        model.CreditLimitStr = await _priceFormatter.FormatPriceAsync(erpAccount.CreditLimit, true, false);
        model.CurrentBalanceStr = await _priceFormatter.FormatPriceAsync(erpAccount.CurrentBalance, true, false);
        model.ErpAccountId = erpAccount.Id;

        return View("~/Plugins/NopStation.Plugin.Payments.B2BAccount/Views/Shared/Components/B2BAccountPayment/Default.cshtml", model);
    }

    #endregion
}
