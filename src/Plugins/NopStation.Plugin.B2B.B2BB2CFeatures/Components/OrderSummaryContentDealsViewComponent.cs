using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Components;

public class OrderSummaryContentDealsViewComponent : NopViewComponent
{
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly IErpAccountService _erpAccountService;
    private readonly IPriceFormatter _priceFormatter;

    public OrderSummaryContentDealsViewComponent(IErpCustomerFunctionalityService erpCustomerFunctionalityService,
        IStoreContext storeContext,
        ISettingService settingService,
        IErpAccountService erpAccountService,
        IPriceFormatter priceFormatter)
    {
        _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
        _storeContext = storeContext;
        _settingService = settingService;
        _erpAccountService = erpAccountService;
        _priceFormatter = priceFormatter;
    }

    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var erpAccount = await _erpCustomerFunctionalityService.GetActiveErpAccountOfCurrentCustomer();

        if (erpAccount == null)
            return Content("");

        var store = await _storeContext.GetCurrentStoreAsync();
        var settings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);
        var accountPaymentEnabled = settings.EnableAccountPayment;

        if (!accountPaymentEnabled)
            return Content("");

        var erpAccountModel = new ErpAccountDataModel()
        {
            AccountNumber = erpAccount.AccountNumber,
            AccountName = erpAccount.AccountName,
            PaymentTypeCode = erpAccount.PaymentTypeCode,
            IsActive = erpAccount.IsActive,
            CreditLimit = erpAccount.CreditLimit,
            CreditLimitUsed = erpAccount.CreditLimit - erpAccount.CreditLimitAvailable,
            CreditLimitAvailable = erpAccount.CreditLimitAvailable,
            CurrentBalance = erpAccount.CurrentBalance
        };

        erpAccountModel.CurrentBalanceStr = await _priceFormatter.FormatPriceAsync(erpAccount.CurrentBalance);
        erpAccountModel.CreditLimitAvailableStr = await _priceFormatter.FormatPriceAsync(erpAccount.CreditLimitAvailable);
        erpAccountModel.CreditLimitUsedStr = await _priceFormatter.FormatPriceAsync(erpAccount.CreditLimit - erpAccount.CreditLimitAvailable);
        erpAccountModel.CreditLimitStr = await _priceFormatter.FormatPriceAsync(erpAccount.CreditLimit);

        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/Shared/Components/OrderSummaryContentDeals/Default.cshtml", erpAccountModel);
    }
}