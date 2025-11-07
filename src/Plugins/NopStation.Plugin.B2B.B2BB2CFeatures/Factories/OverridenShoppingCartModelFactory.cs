using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Attributes;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Payments;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.ShoppingCart;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public partial class OverridenShoppingCartModelFactory : ShoppingCartModelFactory
{
    public OverridenShoppingCartModelFactory(AddressSettings addressSettings, CaptchaSettings captchaSettings, CatalogSettings catalogSettings, CommonSettings commonSettings, CustomerSettings customerSettings, IAddressModelFactory addressModelFactory, IAddressService addressService, IAttributeParser<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeParser, IAttributeService<CheckoutAttribute, CheckoutAttributeValue> checkoutAttributeService, ICheckoutAttributeFormatter checkoutAttributeFormatter, ICountryService countryService, ICurrencyService currencyService, ICustomerService customerService, IDateTimeHelper dateTimeHelper, IDiscountService discountService, IDownloadService downloadService, IGenericAttributeService genericAttributeService, IGiftCardService giftCardService, IHttpContextAccessor httpContextAccessor, ILocalizationService localizationService, IOrderProcessingService orderProcessingService, IOrderTotalCalculationService orderTotalCalculationService, IPaymentPluginManager paymentPluginManager, IPaymentService paymentService, IPermissionService permissionService, IPictureService pictureService, IPriceFormatter priceFormatter, IProductAttributeFormatter productAttributeFormatter, IProductService productService, IShippingService shippingService, IShoppingCartService shoppingCartService, IShortTermCacheManager shortTermCacheManager, IStateProvinceService stateProvinceService, IStaticCacheManager staticCacheManager, IStoreContext storeContext, IStoreMappingService storeMappingService, ITaxService taxService, IUrlRecordService urlRecordService, IVendorService vendorService, IWebHelper webHelper, IWorkContext workContext, MediaSettings mediaSettings, OrderSettings orderSettings, RewardPointsSettings rewardPointsSettings, ShippingSettings shippingSettings, ShoppingCartSettings shoppingCartSettings, TaxSettings taxSettings, VendorSettings vendorSettings) : base(addressSettings, captchaSettings, catalogSettings, commonSettings, customerSettings, addressModelFactory, addressService, checkoutAttributeParser, checkoutAttributeService, checkoutAttributeFormatter, countryService, currencyService, customerService, dateTimeHelper, discountService, downloadService, genericAttributeService, giftCardService, httpContextAccessor, localizationService, orderProcessingService, orderTotalCalculationService, paymentPluginManager, paymentService, permissionService, pictureService, priceFormatter, productAttributeFormatter, productService, shippingService, shoppingCartService, shortTermCacheManager, stateProvinceService, staticCacheManager, storeContext, storeMappingService, taxService, urlRecordService, vendorService, webHelper, workContext, mediaSettings, orderSettings, rewardPointsSettings, shippingSettings, shoppingCartSettings, taxSettings, vendorSettings)
    {
    }

    public override async Task<ShoppingCartModel> PrepareShoppingCartModelAsync(ShoppingCartModel model,
    IList<ShoppingCartItem> cart, bool isEditable = true,
    bool validateCheckoutAttributes = false,
    bool prepareAndDisplayOrderReviewData = false)
    {
        ArgumentNullException.ThrowIfNull(cart);

        ArgumentNullException.ThrowIfNull(model);

        model.OnePageCheckoutEnabled = _orderSettings.OnePageCheckoutEnabled;

        if (!cart.Any())
            return model;

        model.IsEditable = isEditable;
        model.ShowProductImages = _shoppingCartSettings.ShowProductImagesOnShoppingCart;
        model.ShowSku = _catalogSettings.ShowSkuOnProductDetailsPage;
        model.ShowVendorName = _vendorSettings.ShowVendorOnOrderDetailsPage;
        var customer = await _workContext.GetCurrentCustomerAsync();
        var store = await _storeContext.GetCurrentStoreAsync();
        var checkoutAttributesXml = await _genericAttributeService.GetAttributeAsync<string>(customer,
            NopCustomerDefaults.CheckoutAttributes, store.Id);
        var minOrderSubtotalAmountOk = await _orderProcessingService.ValidateMinOrderSubtotalAmountAsync(cart);
        if (!minOrderSubtotalAmountOk)
        {
            var minOrderSubtotalAmount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(_orderSettings.MinOrderSubtotalAmount, await _workContext.GetWorkingCurrencyAsync());
            model.MinOrderSubtotalWarning = string.Format(await _localizationService.GetResourceAsync("Checkout.MinOrderSubtotalAmount"), await _priceFormatter.FormatPriceAsync(minOrderSubtotalAmount, true, false));
        }

        model.TermsOfServiceOnShoppingCartPage = _orderSettings.TermsOfServiceOnShoppingCartPage;
        model.TermsOfServiceOnOrderConfirmPage = _orderSettings.TermsOfServiceOnOrderConfirmPage;
        model.TermsOfServicePopup = _commonSettings.PopupForTermsOfServiceLinks;
        model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoShoppingCart;

        model.DiscountBox.Display = _shoppingCartSettings.ShowDiscountBox;
        var discountCouponCodes = await _customerService.ParseAppliedDiscountCouponCodesAsync(customer);

        foreach (var couponCode in discountCouponCodes)
        {
            var discount = await (await _discountService.GetAllDiscountsAsync(couponCode: couponCode))
                .FirstOrDefaultAwaitAsync(async d => d.RequiresCouponCode && (await _discountService.ValidateDiscountAsync(d, customer, discountCouponCodes)).IsValid);

            if (discount != null)
            {
                model.DiscountBox.AppliedDiscountsWithCodes.Add(new ShoppingCartModel.DiscountBoxModel.DiscountInfoModel
                {
                    Id = discount.Id,
                    CouponCode = discount.CouponCode
                });
            }
        }

        model.GiftCardBox.Display = _shoppingCartSettings.ShowGiftCardBox;

        var cartWarnings = await _shoppingCartService.GetShoppingCartWarningsAsync(cart, checkoutAttributesXml, validateCheckoutAttributes);
        foreach (var warning in cartWarnings)
            model.Warnings.Add(warning);

        model.CheckoutAttributes = await PrepareCheckoutAttributeModelsAsync(cart);

        foreach (var sci in cart)
        {
            var cartItemModel = await PrepareShoppingCartItemModelAsync(cart, sci);
            var product = await _productService.GetProductByIdAsync(sci.ProductId);
            if (product == null || product.Deleted || !product.VisibleIndividually)
            {
                model.Warnings.Add($"{product.Name} does not available in this Account,It is removed from the cart");
                await _shoppingCartService.DeleteShoppingCartItemAsync(sci);
            }
            else
                model.Items.Add(cartItemModel);
        }

        var paymentMethods = await (await _paymentPluginManager
                .LoadActivePluginsAsync(customer, store.Id))
            .WhereAwait(async pm => !await pm.HidePaymentMethodAsync(cart)).ToListAsync();
        var nonButtonPaymentMethods = paymentMethods
            .Where(pm => pm.PaymentMethodType != PaymentMethodType.Button)
            .ToList();
        var buttonPaymentMethods = paymentMethods
            .Where(pm => pm.PaymentMethodType == PaymentMethodType.Button)
            .ToList();
        foreach (var pm in buttonPaymentMethods)
        {
            if (await _shoppingCartService.ShoppingCartIsRecurringAsync(cart) && pm.RecurringPaymentType == RecurringPaymentType.NotSupported)
                continue;

            var viewComponent = pm.GetPublicViewComponent();
            model.ButtonPaymentMethodViewComponents.Add(viewComponent);
        }
        model.HideCheckoutButton = !nonButtonPaymentMethods.Any() && model.ButtonPaymentMethodViewComponents.Any();

        if (prepareAndDisplayOrderReviewData)
        {
            model.OrderReviewData = await PrepareOrderReviewDataModelAsync(cart);
        }

        return model;
    }
}