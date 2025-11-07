using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Services;
using NopStation.Plugin.Payments.B2BAccount.Components;
using NopStation.Plugin.Payments.B2BAccount.Models;
using NopStation.Plugin.Payments.B2BAccount.Validators;

namespace NopStation.Plugin.Payments.B2BAccount;

public class B2BAccountPaymentProcessor : BasePlugin, IPaymentMethod, INopStationPlugin
{
    #region Fields

    private readonly IOrderTotalCalculationService _orderTotalCalculationService;
    private readonly ILocalizationService _localizationService;
    private readonly IWorkContext _workContext;
    private readonly IErpAccountService _erpAccountService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IStoreContext _storeContext;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpLogsService _erpLogsService;
    private readonly INotificationService _notificationService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IErpSalesOrgService _erpSalesOrgService;

    #endregion

    #region Ctor

    public B2BAccountPaymentProcessor(IOrderTotalCalculationService orderTotalCalculationService,
        ILocalizationService localizationService,
        IWorkContext workContext,
        IErpAccountService erpAccountService,
        IErpNopUserService erpNopUserService,
        IShoppingCartService shoppingCartService,
        IStoreContext storeContext,
        IPaymentService paymentService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpLogsService erpLogsService,
        INotificationService notificationService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IGenericAttributeService genericAttributeService,
        IErpSalesOrgService erpSalesOrgService)
    {
        _orderTotalCalculationService = orderTotalCalculationService;
        _localizationService = localizationService;
        _workContext = workContext;
        _erpAccountService = erpAccountService;
        _erpNopUserService = erpNopUserService;
        _shoppingCartService = shoppingCartService;
        _storeContext = storeContext;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
        _erpLogsService = erpLogsService;
        _notificationService = notificationService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _genericAttributeService = genericAttributeService;
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
            var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin(ErpSyncLevel.Account);

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

    public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
    {
        //always success
        return Task.FromResult(new CancelRecurringPaymentResult());
    }

    public Task<bool> CanRePostProcessPaymentAsync(Order order)
    {
        ArgumentNullException.ThrowIfNull(order);

        //it's not a redirection payment method. So we always return false
        return Task.FromResult(false);
    }

    public async Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
    {
        return new CapturePaymentResult { Errors = new[] { await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.CaptureResult.CaptureMethodNotSupported") } };
    }

    public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
    {
        return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart, 0, false);
    }

    public async Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
    {
        string customerReferenceAsPO = form["CustomerReferenceAsPO"];
        if (!string.IsNullOrEmpty(customerReferenceAsPO))
        {
            await _genericAttributeService.SaveAttributeAsync(await _workContext.GetCurrentCustomerAsync(), 
                B2BB2CFeaturesDefaults.ProvidedB2BCustomerReferenceAsPO, 
                customerReferenceAsPO, 
                (await _storeContext.GetCurrentStoreAsync()).Id);
        }

        return new ProcessPaymentRequest();
    }

    public async Task<string> GetPaymentMethodDescriptionAsync()
    {
        return await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.PaymentMethodDescription");
    }

    public Type GetPublicViewComponent()
    {
        return typeof(B2BAccountPaymentViewComponent);
    }

    public async Task<bool> HidePaymentMethodAsync(IList<ShoppingCartItem> cart)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
            return true;

        var nopErpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);
        if (nopErpUser == null || nopErpUser.ErpUserType != ErpUserType.B2BUser)
            return true;

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(nopErpUser.ErpAccountId);
        if (erpAccount == null)
            return true;

        return false;
    }

    public async Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
    {

    }

    public async Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        var paymentResult = new ProcessPaymentResult();

        await LiveErpAccountCreditCheckAsync();

        var customer = await _workContext.GetCurrentCustomerAsync();
        var b2BUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);
        var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(customer.Id);

        if (b2BUser != null && !b2BUser.IsDeleted && b2BUser.IsActive)
        {
            if (b2BAccount != null && !b2BAccount.IsDeleted && b2BAccount.IsActive)
            {
                var spendableAmount = b2BAccount.CreditLimitAvailable;

                if (spendableAmount >= processPaymentRequest.OrderTotal || b2BAccount.AllowOverspend)
                {
                    b2BAccount.CurrentBalance += processPaymentRequest.OrderTotal;
                    b2BAccount.CreditLimitAvailable -= processPaymentRequest.OrderTotal;
                    await _erpAccountService.UpdateErpAccountAsync(b2BAccount);

                    paymentResult.NewPaymentStatus = PaymentStatus.Paid;

                    if ((spendableAmount < processPaymentRequest.OrderTotal) && _b2BB2CFeaturesSettings.IsShowOverSpendWarningText)
                    {
                        _notificationService.WarningNotification(
                            await _localizationService.GetLocalizedSettingAsync(
                            _b2BB2CFeaturesSettings,
                            x => x.OverSpendWarningText,
                            (await _workContext.GetWorkingLanguageAsync()).Id,
                            (await _storeContext.GetCurrentStoreAsync()).Id)
                        );
                    }
                }
                else
                {
                    paymentResult.AddError(await _localizationService.GetLocalizedSettingAsync(
                        _b2BB2CFeaturesSettings,
                        x => x.OverSpendWarningText,
                        (await _workContext.GetWorkingLanguageAsync()).Id,
                        (await _storeContext.GetCurrentStoreAsync()).Id));
                }
            }
            else
            {
                paymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.InactiveB2BUser"));
            }
        }
        else
        {
            paymentResult.AddError(await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.InvalidB2BUser"));
        }

        return paymentResult;
    }

    public async Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
    {
        return new ProcessPaymentResult { Errors = new[] { await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.ProcessRecurringPaymentResult.RecurringPaymentNotSupported") } };
    }

    public async Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
    {
        return new RefundPaymentResult { Errors = new[] { await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.RefundResult.RefundMethodNotSupported") } };
    }

    public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
    {
        var warnings = new List<string>();

        var store = await _storeContext.GetCurrentStoreAsync();
        var customer = await _workContext.GetCurrentCustomerAsync();
        if (customer == null)
        {
            warnings.Add(await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.Validation.NoCustomerFound"));
            return warnings;
        }

        var nopErpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);
        if (nopErpUser == null)
        {
            warnings.Add(await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.Validation.NoErpUserFound"));
            return warnings;
        }

        if (nopErpUser.ErpUserType != ErpUserType.B2BUser)
        {
            warnings.Add(await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.Validation.OnlyB2BUserIsAllowedToProceed"));
            return warnings;
        }

        var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(nopErpUser.ErpAccountId);
        if (erpAccount == null)
        {
            warnings.Add(await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.Validation.NoErpAccountfound"));
            return warnings;
        }

        //validate
        var validator = new PaymentInfoValidator(_localizationService, _erpOrderAdditionalDataService, _b2BB2CFeaturesSettings);
        var model = new B2BAccountPaymentInfoModel
        {
            ErpAccountId = erpAccount.Id,
            CustomerReferenceAsPO = form["CustomerReferenceAsPO"]
        };

        var validationResult = validator.Validate(model);
        if (!validationResult.IsValid)
        {
            warnings.AddRange(validationResult.Errors.Select(error => error.ErrorMessage));
            return warnings;
        }

        var sci = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId: store.Id);
        var (shoppingCartTotalBase, _, _, _, _, _) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(sci);

        if (shoppingCartTotalBase > erpAccount.CreditLimitAvailable && !erpAccount.AllowOverspend)
        {
            warnings.Add(await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.Validation.InsufficientErpAccountBalance"));
        }
        return warnings;
    }

    public async Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
    {
        return new VoidPaymentResult { Errors = new[] { await _localizationService.GetResourceAsync("Plugins.Payments.NopStation.B2B.Account.VoidResult.VoidMethodNotSupported") } };
    }

    public override async Task InstallAsync()
    {
        await this.InstallPluginAsync();

        await base.InstallAsync();
    }

    public override async Task UninstallAsync()
    {
        await this.UninstallPluginAsync();

        await base.UninstallAsync();
    }

    public List<KeyValuePair<string, string>> PluginResouces()
    {
        return new List<KeyValuePair<string, string>>
        {
            new ("Plugins.Payments.NopStation.B2B.Account.PaymentMethodDescription", "This plugin allows to buy using credit amount for b2b customer"),
            new ("Plugins.Payments.NopStation.B2B.Account.Title.CreditLimit", "Credit Limit"),
            new ("Plugins.Payments.NopStation.B2B.Account.Title.CreditLimitAvailable", "Credit Limit Available"),
            new ("Plugins.Payments.NopStation.B2B.Account.Validation.NoCustomerFound", "No Customer found"),
            new ("Plugins.Payments.NopStation.B2B.Account.Validation.NoErpUserFound", "No Erp User found"),
            new ("Plugins.Payments.NopStation.B2B.Account.Validation.NoErpAccountfound", "No Erp Account found"),
            new ("Plugins.Payments.NopStation.B2B.Account.Validation.InsufficientErpAccountBalance", "Insufficient Erp Account balance"),
            new ("Plugins.Payments.NopStation.B2B.Account.Validation.OnlyB2BUserIsAllowedToProceed", "Only B2B User is allowed to proceed"),
            new ("Plugins.Payments.NopStation.B2B.Account.CaptureResult.CaptureMethodNotSupported", "Capture method not supported"),
            new ("Plugins.Payments.NopStation.B2B.Account.ProcessRecurringPaymentResult.RecurringPaymentNotSupported", "Recurring payment not supported"),
            new ("Plugins.Payments.NopStation.B2B.Account.RefundResult.RefundMethodNotSupported", "Refund method not supported"),
            new ("Plugins.Payments.NopStation.B2B.Account.VoidResult.VoidMethodNotSupported", "Void method not supported"),
            new ("Plugins.Payments.NopStation.B2B.Account.InactiveB2BUser", "Inactive B2B user"),
            new ("Plugins.Payments.NopStation.B2B.Account.InvalidB2BUser", "Invalid B2B user"),
            new ("Plugins.Payments.NopStation.B2B.Account.CustomerReferenceAsPO.CustomerReferenceAsPORequired", "Customer reference is required"),
            new ("Plugins.Payments.NopStation.B2B.Account.CustomerReferenceAsPO.AlreadyExist", "Your provided po reference already exist"),
            new ("Plugins.Payments.NopStation.B2B.Account.CustomerReferenceAsPO.Invalid", "Invalid Purchase Order Reference. Please ignore the following special characters: {0}"),
            new ("Plugins.Payments.NopStation.B2B.Account.PaymentInfo.CustomerReferenceAsPO", "Customer Reference As PO"),
        };
    }

    #endregion

    #region Properties

    public bool SupportCapture => false;

    public bool SupportPartiallyRefund => false;

    public bool SupportRefund => false;

    public bool SupportVoid => false;

    public RecurringPaymentType RecurringPaymentType => RecurringPaymentType.NotSupported;

    public PaymentMethodType PaymentMethodType => PaymentMethodType.Standard;

    public bool SkipPaymentInfo => false;

    #endregion
}