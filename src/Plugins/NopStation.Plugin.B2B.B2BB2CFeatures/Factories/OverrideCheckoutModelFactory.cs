using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Domain;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Payments;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Shipping;
using Nop.Services.Shipping.Pickup;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Factories;
using Nop.Web.Models.Checkout;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public partial class OverrideCheckoutModelFactory : CheckoutModelFactory
{
    #region Fields

    private readonly ISettingService _settingService;
    private readonly IPictureService _pictureService;
    public static string B2BPaymentMethodSystemName => "NopStation.Plugin.Payments.B2BAccount";

    #endregion

    #region Ctor

    public OverrideCheckoutModelFactory(AddressSettings addressSettings,
        CaptchaSettings captchaSettings,
        CommonSettings commonSettings,
        IAddressModelFactory addressModelFactory,
        IAddressService addressService,
        ICountryService countryService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IGenericAttributeService genericAttributeService,
        ILocalizationService localizationService,
        IOrderProcessingService orderProcessingService,
        IOrderTotalCalculationService orderTotalCalculationService,
        IPaymentPluginManager paymentPluginManager,
        IPaymentService paymentService,
        IPickupPluginManager pickupPluginManager,
        IPriceFormatter priceFormatter,
        IRewardPointService rewardPointService,
        IShippingPluginManager shippingPluginManager,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        IStateProvinceService stateProvinceService,
        IStoreContext storeContext,
        IStoreMappingService storeMappingService,
        ITaxService taxService,
        IWorkContext workContext,
        OrderSettings orderSettings,
        PaymentSettings paymentSettings,
        RewardPointsSettings rewardPointsSettings,
        ShippingSettings shippingSettings,
        TaxSettings taxSettings,
        ISettingService settingService,
        IPictureService pictureService) : base(addressSettings,
            captchaSettings,
            commonSettings,
            addressModelFactory,
            addressService,
            countryService,
            currencyService,
            customerService,
            genericAttributeService,
            localizationService,
            orderProcessingService,
            orderTotalCalculationService,
            paymentPluginManager,
            paymentService,
            pickupPluginManager,
            priceFormatter,
            rewardPointService,
            shippingPluginManager,
            shippingService,
            shoppingCartService,
            stateProvinceService,
            storeContext,
            storeMappingService,
            taxService,
            workContext,
            orderSettings,
            paymentSettings,
            rewardPointsSettings,
            shippingSettings,
            taxSettings)
    {
        _settingService = settingService;
        _pictureService = pictureService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Prepare payment method model
    /// </summary>
    /// <param name="cart">Cart</param>
    /// <param name="filterByCountryId">Filter by country identifier</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the payment method model
    /// </returns>
    public override async Task<CheckoutPaymentMethodModel> PreparePaymentMethodModelAsync(IList<ShoppingCartItem> cart, int filterByCountryId)
    {
        var model = new CheckoutPaymentMethodModel();

        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();

        var storeInformationSettings = await _settingService.LoadSettingAsync<StoreInformationSettings>(store.Id);
        var logoUrl = await _pictureService.GetPictureUrlAsync(storeInformationSettings.LogoPictureId);

        //reward points
        if (_rewardPointsSettings.Enabled && !await _shoppingCartService.ShoppingCartIsRecurringAsync(cart))
        {
            var shoppingCartTotal = await _orderTotalCalculationService.GetShoppingCartTotalAsync(cart, true, false);
            if (shoppingCartTotal.redeemedRewardPoints > 0)
            {
                model.DisplayRewardPoints = true;
                model.RewardPointsToUseAmount = await _priceFormatter.FormatPriceAsync(shoppingCartTotal.redeemedRewardPointsAmount, true, false);
                model.RewardPointsToUse = shoppingCartTotal.redeemedRewardPoints;
                model.RewardPointsBalance = await _rewardPointService.GetRewardPointsBalanceAsync(customer.Id, store.Id);

                //are points enough to pay for entire order? like if this option (to use them) was selected
                model.RewardPointsEnoughToPayForOrder = !await _orderProcessingService.IsPaymentWorkflowRequiredAsync(cart, true);
            }
        }

        //filter by country
        var paymentMethods = await (await _paymentPluginManager
                .LoadActivePluginsAsync(customer, store.Id, filterByCountryId))
            .Where(pm => pm.PaymentMethodType == PaymentMethodType.Standard || pm.PaymentMethodType == PaymentMethodType.Redirection)
            .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart))
            .ToListAsync();

        foreach (var pm in paymentMethods)
        {
            if (await _shoppingCartService.ShoppingCartIsRecurringAsync(cart) && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                continue;

            var pmModel = new CheckoutPaymentMethodModel.PaymentMethodModel
            {
                Name = await _localizationService.GetLocalizedFriendlyNameAsync(pm, (await _workContext.GetWorkingLanguageAsync()).Id),
                Description = _paymentSettings.ShowPaymentMethodDescriptions ? await pm.GetPaymentMethodDescriptionAsync() : string.Empty,
                PaymentMethodSystemName = pm.PluginDescriptor.SystemName,
                LogoUrl = await _paymentPluginManager.GetPluginLogoUrlAsync(pm)
            };

            if (pmModel.PaymentMethodSystemName.Equals(B2BPaymentMethodSystemName))
            {
                pmModel.LogoUrl = logoUrl;
            }

            //payment method additional fee
            var paymentMethodAdditionalFee = await _paymentService.GetAdditionalHandlingFeeAsync(cart, pm.PluginDescriptor.SystemName);
            var (rateBase, _) = await _taxService.GetPaymentMethodAdditionalFeeAsync(paymentMethodAdditionalFee, customer);
            var rate = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(rateBase, await _workContext.GetWorkingCurrencyAsync());

            if (rate > decimal.Zero)
                pmModel.Fee = await _priceFormatter.FormatPaymentMethodAdditionalFeeAsync(rate, true);

            model.PaymentMethods.Add(pmModel);
        }

        //find a selected (previously) payment method
        var selectedPaymentMethodSystemName = await _genericAttributeService.GetAttributeAsync<string>(customer,
            NopCustomerDefaults.SelectedPaymentMethodAttribute, store.Id);
        if (!string.IsNullOrEmpty(selectedPaymentMethodSystemName))
        {
            var paymentMethodToSelect = model.PaymentMethods.ToList()
                .Find(pm => pm.PaymentMethodSystemName.Equals(selectedPaymentMethodSystemName, StringComparison.InvariantCultureIgnoreCase));
            if (paymentMethodToSelect != null)
                paymentMethodToSelect.Selected = true;
        }
        //if no option has been selected, let's do it for the first one
        if (model.PaymentMethods.FirstOrDefault(so => so.Selected) == null)
        {
            var paymentMethodToSelect = model.PaymentMethods.FirstOrDefault();
            if (paymentMethodToSelect != null)
                paymentMethodToSelect.Selected = true;
        }

        return model;
    }

    #endregion
}