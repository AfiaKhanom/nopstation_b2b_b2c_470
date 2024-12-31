using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Domain.Orders;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Payments.B2B.Account.Components;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Plugins;
using NopStation.Plugin.Misc.Core.Services;

namespace NopStation.Plugin.Payments.B2B.Account
{
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

        #endregion

        #region Ctor

        public B2BAccountPaymentProcessor(IOrderTotalCalculationService orderTotalCalculationService,
                                          ILocalizationService localizationService,
                                          IWorkContext workContext,
                                          IErpAccountService erpAccountService,
                                          IErpNopUserService erpNopUserService,
                                          IShoppingCartService shoppingCartService,
                                          IStoreContext storeContext
            )
        {
            _orderTotalCalculationService = orderTotalCalculationService;
            _localizationService = localizationService;
            _workContext = workContext;
            _erpAccountService = erpAccountService;
            _erpNopUserService = erpNopUserService;
            _shoppingCartService = shoppingCartService;
            _storeContext = storeContext;
        }

        #endregion

        #region Utilitis

        #endregion

        #region Methods


        public Task<CancelRecurringPaymentResult> CancelRecurringPaymentAsync(CancelRecurringPaymentRequest cancelPaymentRequest)
        {
            //always success
            return Task.FromResult(new CancelRecurringPaymentResult());
        }

        public Task<bool> CanRePostProcessPaymentAsync(Order order)
        {
            if (order == null)
                throw new ArgumentNullException(nameof(order));

            //it's not a redirection payment method. So we always return false
            return Task.FromResult(false);
        }

        public Task<CapturePaymentResult> CaptureAsync(CapturePaymentRequest capturePaymentRequest)
        {
            return Task.FromResult(new CapturePaymentResult { Errors = new[] { "Capture method not supported" } });
        }

        public async Task<decimal> GetAdditionalHandlingFeeAsync(IList<ShoppingCartItem> cart)
        {
            return await _orderTotalCalculationService.CalculatePaymentAdditionalFeeAsync(cart,
                0, false);
        }

        public Task<ProcessPaymentRequest> GetPaymentInfoAsync(IFormCollection form)
        {
            return Task.FromResult(new ProcessPaymentRequest());
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

        public  Task PostProcessPaymentAsync(PostProcessPaymentRequest postProcessPaymentRequest)
        {
            var warnings = new List<string>();

            var store =  _storeContext.GetCurrentStoreAsync();
            var customer =  _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                warnings.Add("No customer found");

            var nopErpUser =  _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id).Result;
            if (nopErpUser == null)
            {
                warnings.Add("No Erp User found");
                if (nopErpUser.ErpUserType != ErpUserType.B2BUser)
                {
                    warnings.Add("Only for B2B User");
                }
            }
            var erpAccount =  _erpAccountService.GetErpAccountByIdAsync(nopErpUser.ErpAccountId).Result;
            if (erpAccount == null)
            {
                warnings.Add("No found erp account");
            }
            erpAccount.CurrentBalance += postProcessPaymentRequest.Order.OrderTotal;
            //nothing
            return  Task.CompletedTask;
        }

        public Task<ProcessPaymentResult> ProcessPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult());
        }

        public Task<ProcessPaymentResult> ProcessRecurringPaymentAsync(ProcessPaymentRequest processPaymentRequest)
        {
            return Task.FromResult(new ProcessPaymentResult { Errors = new[] { "Recurring payment not supported" } });
        }

        public Task<RefundPaymentResult> RefundAsync(RefundPaymentRequest refundPaymentRequest)
        {
            return Task.FromResult(new RefundPaymentResult { Errors = new[] { "Refund method not supported" } });
        }

        public async Task<IList<string>> ValidatePaymentFormAsync(IFormCollection form)
        {
            var warnings = new List<string>();

            var store = await _storeContext.GetCurrentStoreAsync();
            var customer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
                warnings.Add("No customer found");

            var nopErpUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(customer.Id);
            if (nopErpUser == null)
            {
                warnings.Add("No Erp User found");
                if (nopErpUser.ErpUserType != ErpUserType.B2BUser)
                {
                    warnings.Add("Only for B2B User");
                }
            }
            var erpAccount = await _erpAccountService.GetErpAccountByIdAsync(nopErpUser.ErpAccountId);
            if (erpAccount == null)
            {
                warnings.Add("No found erp account");
            }

            var sci = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, storeId: store.Id);
            //total
            var (shoppingCartTotalBase, orderTotalDiscountAmountBase, _, appliedGiftCards, redeemedRewardPoints, redeemedRewardPointsAmount) = await _orderTotalCalculationService.GetShoppingCartTotalAsync(sci);

            if (shoppingCartTotalBase > erpAccount.CreditLimitAvailable && !erpAccount.AllowOverspend)
            {
                warnings.Add("Insufficient ERP Account Balance");
            }

            return await Task.FromResult<IList<string>>(warnings);
        }

        public Task<VoidPaymentResult> VoidAsync(VoidPaymentRequest voidPaymentRequest)
        {
            return Task.FromResult(new VoidPaymentResult { Errors = new[] { "Void method not supported" } });
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
            var list = new List<KeyValuePair<string, string>>
            {
                new KeyValuePair<string, string>("Plugins.Payments.NopStation.B2B.Account.PaymentMethodDescription", "This plugin allows to buy using credit amount for b2b customer"),
                new KeyValuePair<string, string>("Plugins.Payments.NopStation.B2B.Account.Title.CreditLimit", "Credit Limit"),
                new KeyValuePair<string, string>("Plugins.Payments.NopStation.B2B.Account.Title.CreditLimitAvailable", "Credit Limit Available"),
                new KeyValuePair<string, string>("Plugins.Payments.NopStation.B2B.Account.Title.CurrentBalance", "Current Balance"),
            };

            return list;
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
}