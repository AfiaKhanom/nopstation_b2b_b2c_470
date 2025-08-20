using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using NopStation.Plugin.Widgets.AdvanceCart.Models;

namespace NopStation.Plugin.Widgets.AdvanceCart.Factories;

public class AdvanceCartModelFactory : IAdvanceCartModelFactory
{
    private readonly IProductService _productService;
    private readonly IPermissionService _permissionService;
    private readonly ILocalizationService _localizationService;
    private readonly AdvanceCartSettings _advanceCartSettings;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly IVendorService _vendorService;
    private readonly VendorSettings _vendorSettings;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IWorkContext _workContext;
    private readonly ITaxService _taxService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly ICurrencyService _currencyService;
    private readonly OrderSettings _orderSettings;
    private readonly ShoppingCartSettings _shoppingCartSettings;
    private readonly MediaSettings _mediaSettings;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;

    public AdvanceCartModelFactory(IProductService productService,
        IPermissionService permissionService,
        ILocalizationService localizationService,
        AdvanceCartSettings advanceCartSettings,
        IShoppingCartService shoppingCartService,
        IVendorService vendorService,
        VendorSettings vendorSettings,
        IUrlRecordService urlRecordService,
        IProductAttributeFormatter productAttributeFormatter,
        IWorkContext workContext,
        ITaxService taxService,
        IPriceFormatter priceFormatter,
        ICurrencyService currencyService,
        OrderSettings orderSettings,
        ShoppingCartSettings shoppingCartSettings,
        MediaSettings mediaSettings,
        IShoppingCartModelFactory shoppingCartModelFactory)
    {
        _productService = productService;
        _permissionService = permissionService;
        _localizationService = localizationService;
        _advanceCartSettings = advanceCartSettings;
        _shoppingCartService = shoppingCartService;
        _vendorService = vendorService;
        _vendorSettings = vendorSettings;
        _urlRecordService = urlRecordService;
        _productAttributeFormatter = productAttributeFormatter;
        _workContext = workContext;
        _taxService = taxService;
        _priceFormatter = priceFormatter;
        _currencyService = currencyService;
        _orderSettings = orderSettings;
        _shoppingCartSettings = shoppingCartSettings;
        _mediaSettings = mediaSettings;
        _shoppingCartModelFactory = shoppingCartModelFactory;
    }

    public async Task<AdvanceCartProductOverviewModel> PrepareProductOverviewQuantityModelAsync(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        var model = new AdvanceCartProductOverviewModel
        {
            Id = product.Id,
            AllowCustomersToSelectQuantityFromProductBox = _advanceCartSettings.AllowCustomersToSelectQuantityFromProductBox,
            EnableBuyNowButton = _advanceCartSettings.EnableBuyNowButton
        };

        if (_advanceCartSettings.AllowCustomersToSelectQuantityFromProductBox)
        {
            //quantity
            model.EnteredQuantity = product.OrderMinimumQuantity;
            //allowed quantities
            var allowedQuantities = _productService.ParseAllowedQuantities(product);
            foreach (var qty in allowedQuantities)
            {
                model.AllowedQuantities.Add(new SelectListItem
                {
                    Text = qty.ToString(),
                    Value = qty.ToString(),
                    Selected = qty == model.EnteredQuantity
                });
            }
            //minimum quantity notification
            if (product.OrderMinimumQuantity > 1)
            {
                model.MinimumQuantityNotification = string.Format(await _localizationService.GetResourceAsync("Products.MinimumQuantityNotification"), product.OrderMinimumQuantity);
            }
        }

        //'add to cart', 'add to wishlist' buttons
        model.DisableBuyButton = product.DisableBuyButton || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart);

        return model;
    }

    public async Task<AdvanceCartAddedToCartModel> PrepareAdvanceCartAddedToCartModelAsync(Product product, Customer customer,
        ShoppingCartType shoppingCartType,
        int storeId,
        string attributesXml = "",
        decimal customerEnteredPrice = decimal.Zero)
    {
        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, shoppingCartType, storeId);

        var sci = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart,
            shoppingCartType, product, attributesXml, customerEnteredPrice);

        if (sci == null)
            throw new ArgumentNullException(nameof(sci));

        var model = new AdvanceCartAddedToCartModel
        {
            Id = sci.Id,
            Sku = await _productService.FormatSkuAsync(product, sci.AttributesXml),
            VendorName = _vendorSettings.ShowVendorOnOrderDetailsPage ? (await _vendorService.GetVendorByProductIdAsync(product.Id))?.Name : string.Empty,
            ProductId = sci.ProductId,
            ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name),
            ProductSeName = await _urlRecordService.GetSeNameAsync(product),
            Quantity = sci.Quantity,
            AttributeInfo = await _productAttributeFormatter.FormatAttributesAsync(product, sci.AttributesXml),
        };

        //recurring info
        if (product.IsRecurring)
            model.RecurringInfo = string.Format(await _localizationService.GetResourceAsync("ShoppingCart.RecurringPeriod"),
                    product.RecurringCycleLength, await _localizationService.GetLocalizedEnumAsync(product.RecurringCyclePeriod));

        //rental info
        if (product.IsRental)
        {
            var rentalStartDate = sci.RentalStartDateUtc.HasValue
                ? _productService.FormatRentalDate(product, sci.RentalStartDateUtc.Value)
                : string.Empty;
            var rentalEndDate = sci.RentalEndDateUtc.HasValue
                ? _productService.FormatRentalDate(product, sci.RentalEndDateUtc.Value)
                : string.Empty;
            model.RentalInfo =
                string.Format(await _localizationService.GetResourceAsync("ShoppingCart.Rental.FormattedDate"),
                    rentalStartDate, rentalEndDate);
        }

        //unit prices
        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        if (product.CallForPrice &&
            //also check whether the current user is impersonated
            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
        {
            model.UnitPrice = await _localizationService.GetResourceAsync("Products.CallForPrice");
            model.UnitPriceValue = 0;
        }
        else
        {
            var (shoppingCartUnitPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _shoppingCartService.GetUnitPriceAsync(sci, true)).unitPrice);
            var shoppingCartUnitPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartUnitPriceWithDiscountBase, currentCurrency);
            model.UnitPrice = await _priceFormatter.FormatPriceAsync(shoppingCartUnitPriceWithDiscount);
            model.UnitPriceValue = shoppingCartUnitPriceWithDiscount;
        }
        //subtotal, discount
        if (product.CallForPrice &&
            //also check whether the current user is impersonated
            (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
        {
            model.SubTotal = await _localizationService.GetResourceAsync("Products.CallForPrice");
            model.SubTotalValue = 0;
        }
        else
        {
            //sub total
            var (subTotal, shoppingCartItemDiscountBase, _, maximumDiscountQty) = await _shoppingCartService.GetSubTotalAsync(sci, true);
            var (shoppingCartItemSubTotalWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, subTotal);
            var shoppingCartItemSubTotalWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemSubTotalWithDiscountBase, currentCurrency);
            model.SubTotal = await _priceFormatter.FormatPriceAsync(shoppingCartItemSubTotalWithDiscount);
            model.SubTotalValue = shoppingCartItemSubTotalWithDiscount;
            model.MaximumDiscountedQty = maximumDiscountQty;

            //display an applied discount amount
            if (shoppingCartItemDiscountBase > decimal.Zero)
            {
                (shoppingCartItemDiscountBase, _) = await _taxService.GetProductPriceAsync(product, shoppingCartItemDiscountBase);
                if (shoppingCartItemDiscountBase > decimal.Zero)
                {
                    var shoppingCartItemDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(shoppingCartItemDiscountBase, currentCurrency);
                    model.Discount = await _priceFormatter.FormatPriceAsync(shoppingCartItemDiscount);
                    model.DiscountValue = shoppingCartItemDiscount;
                }
            }
        }

        //picture
        model.Picture = await _shoppingCartModelFactory.PrepareCartItemPictureModelAsync(sci,
            _advanceCartSettings.NotificationPopupProductImageSize, true, model.ProductName);

        return model;
    }
}
