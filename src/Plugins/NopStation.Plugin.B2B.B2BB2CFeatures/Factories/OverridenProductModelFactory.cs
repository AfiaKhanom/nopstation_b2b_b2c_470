using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Media;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Security;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using Nop.Web.Factories;
using Nop.Web.Models.Catalog;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories;

public class OverridenProductModelFactory : ProductModelFactory
{
    #region fields

    private readonly CaptchaSettings _captchaSettings;
    private readonly CatalogSettings _catalogSettings;
    private readonly CustomerSettings _customerSettings;
    private readonly ICategoryService _categoryService;
    private readonly ICurrencyService _currencyService;
    private readonly ICustomerService _customerService;
    private readonly IDateRangeService _dateRangeService;
    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly IDownloadService _downloadService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IJsonLdModelFactory _jsonLdModelFactory;
    private readonly ILocalizationService _localizationService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IPermissionService _permissionService;
    private readonly IPictureService _pictureService;
    private readonly IPriceCalculationService _priceCalculationService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly IProductAttributeService _productAttributeService;
    private readonly IProductService _productService;
    private readonly IProductTagService _productTagService;
    private readonly IProductTemplateService _productTemplateService;
    private readonly IReviewTypeService _reviewTypeService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly IStoreContext _storeContext;
    private readonly IStoreService _storeService;
    private readonly IShoppingCartModelFactory _shoppingCartModelFactory;
    private readonly ITaxService _taxService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly IVendorService _vendorService;
    private readonly IVideoService _videoService;
    private readonly IWebHelper _webHelper;
    private readonly IWorkContext _workContext;
    private readonly MediaSettings _mediaSettings;
    private readonly OrderSettings _orderSettings;
    private readonly SeoSettings _seoSettings;
    private readonly ShippingSettings _shippingSettings;
    private readonly VendorSettings _vendorSettings;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionality;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region ctor

    public OverridenProductModelFactory(CaptchaSettings captchaSettings,
        CatalogSettings catalogSettings,
        CustomerSettings customerSettings,
        ICategoryService categoryService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IDateRangeService dateRangeService,
        IDateTimeHelper dateTimeHelper,
        IDownloadService downloadService,
        IGenericAttributeService genericAttributeService,
        IJsonLdModelFactory jsonLdModelFactory,
        ILocalizationService localizationService,
        IManufacturerService manufacturerService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IPriceCalculationService priceCalculationService,
        IPriceFormatter priceFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductService productService,
        IProductTagService productTagService,
        IProductTemplateService productTemplateService,
        IReviewTypeService reviewTypeService,
        IShoppingCartService shoppingCartService,
        ISpecificationAttributeService specificationAttributeService,
        IStaticCacheManager staticCacheManager,
        IStoreContext storeContext,
        IStoreService storeService,
        IShoppingCartModelFactory shoppingCartModelFactory,
        ITaxService taxService,
        IUrlRecordService urlRecordService,
        IVendorService vendorService,
        IVideoService videoService,
        IWebHelper webHelper,
        IWorkContext workContext,
        MediaSettings mediaSettings,
        OrderSettings orderSettings,
        SeoSettings seoSettings,
        ShippingSettings shippingSettings,
        VendorSettings vendorSettings,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpCustomerFunctionalityService erpCustomerFunctionality,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings) : base(captchaSettings,
            catalogSettings,
            customerSettings,
            categoryService,
            currencyService,
            customerService,
            dateRangeService,
            dateTimeHelper,
            downloadService,
            genericAttributeService,
            jsonLdModelFactory,
            localizationService,
            manufacturerService,
            permissionService,
            pictureService,
            priceCalculationService,
            priceFormatter,
            productAttributeParser,
            productAttributeService,
            productService,
            productTagService,
            productTemplateService,
            reviewTypeService,
            shoppingCartService,
            specificationAttributeService,
            staticCacheManager,
            storeContext,
            storeService,
            shoppingCartModelFactory,
            taxService,
            urlRecordService,
            vendorService,
            videoService,
            webHelper,
            workContext,
            mediaSettings,
            orderSettings,
            seoSettings,
            shippingSettings,
            vendorSettings)
    {
        _captchaSettings = captchaSettings;
        _catalogSettings = catalogSettings;
        _customerSettings = customerSettings;
        _categoryService = categoryService;
        _currencyService = currencyService;
        _customerService = customerService;
        _dateRangeService = dateRangeService;
        _dateTimeHelper = dateTimeHelper;
        _downloadService = downloadService;
        _genericAttributeService = genericAttributeService;
        _jsonLdModelFactory = jsonLdModelFactory;
        _localizationService = localizationService;
        _manufacturerService = manufacturerService;
        _permissionService = permissionService;
        _pictureService = pictureService;
        _priceCalculationService = priceCalculationService;
        _priceFormatter = priceFormatter;
        _productAttributeParser = productAttributeParser;
        _productAttributeService = productAttributeService;
        _productService = productService;
        _productTagService = productTagService;
        _productTemplateService = productTemplateService;
        _reviewTypeService = reviewTypeService;
        _shoppingCartService = shoppingCartService;
        _specificationAttributeService = specificationAttributeService;
        _staticCacheManager = staticCacheManager;
        _storeContext = storeContext;
        _storeService = storeService;
        _shoppingCartModelFactory = shoppingCartModelFactory;
        _taxService = taxService;
        _urlRecordService = urlRecordService;
        _vendorService = vendorService;
        _videoService = videoService;
        _webHelper = webHelper;
        _workContext = workContext;
        _mediaSettings = mediaSettings;
        _orderSettings = orderSettings;
        _seoSettings = seoSettings;
        _shippingSettings = shippingSettings;
        _vendorSettings = vendorSettings;
        _erpSpecialPriceService = erpSpecialPriceService;
        _erpCustomerFunctionality = erpCustomerFunctionality;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Utilities

    protected override async Task<ProductDetailsModel.AddToCartModel> PrepareProductAddToCartModelAsync(Product product, ShoppingCartItem updatecartitem)
    {
        ArgumentNullException.ThrowIfNull(product);

        var model = new ProductDetailsModel.AddToCartModel
        {
            ProductId = product.Id
        };

        if (updatecartitem != null)
        {
            model.UpdatedShoppingCartItemId = updatecartitem.Id;
            model.UpdateShoppingCartItemType = updatecartitem.ShoppingCartType;
        }

        //quantity
        model.EnteredQuantity = updatecartitem != null ? updatecartitem.Quantity : product.OrderMinimumQuantity;
        //allowed quantities
        var allowedQuantities = _productService.ParseAllowedQuantities(product);
        foreach (var qty in allowedQuantities)
        {
            model.AllowedQuantities.Add(new SelectListItem
            {
                Text = qty.ToString(),
                Value = qty.ToString(),
                Selected = updatecartitem != null && updatecartitem.Quantity == qty
            });
        }
        //minimum quantity notification
        if (product.OrderMinimumQuantity > 1)
        {
            model.MinimumQuantityNotification = string.Format(await _localizationService.GetResourceAsync("Products.MinimumQuantityNotification"), product.OrderMinimumQuantity);
        }

        //'add to cart', 'add to wishlist' buttons
        model.DisableBuyButton = product.DisableBuyButton || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart);
        model.DisableWishlistButton = product.DisableWishlistButton || !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist);

        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices) ||
            !await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
        {
            model.DisableBuyButton = true;
            model.DisableWishlistButton = true;
        }

        //pre-order
        if (product.AvailableForPreOrder)
        {
            model.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                         product.PreOrderAvailabilityStartDateTimeUtc.Value >= DateTime.UtcNow;
            model.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;

            if (model.AvailableForPreOrder &&
                model.PreOrderAvailabilityStartDateTimeUtc.HasValue &&
                _catalogSettings.DisplayDatePreOrderAvailability)
            {
                model.PreOrderAvailabilityStartDateTimeUserTime =
                    (await _dateTimeHelper.ConvertToUserTimeAsync(model.PreOrderAvailabilityStartDateTimeUtc.Value)).ToString("D");
            }
        }
        //rental
        model.IsRental = product.IsRental;

        //customer entered price
        model.CustomerEntersPrice = product.CustomerEntersPrice;
        if (!model.CustomerEntersPrice)
            return model;

        var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
        var minimumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MinimumCustomerEnteredPrice, currentCurrency);
        var maximumCustomerEnteredPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(product.MaximumCustomerEnteredPrice, currentCurrency);

        model.CustomerEnteredPrice = updatecartitem != null ? updatecartitem.CustomerEnteredPrice : minimumCustomerEnteredPrice;
        model.CustomerEnteredPriceRange = string.Format(await _localizationService.GetResourceAsync("Products.EnterProductPrice.Range"),
            await _priceFormatter.FormatPriceAsync(minimumCustomerEnteredPrice, false, false),
            await _priceFormatter.FormatPriceAsync(maximumCustomerEnteredPrice, false, false));

        return model;
    }

    /// <summary>
    /// Prepare the simple product overview price model
    /// </summary>
    /// <param name="product">Product</param>
    /// <param name="priceModel">Price model</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected override async Task PrepareSimpleProductOverviewPriceModelAsync(Product product, ProductOverviewModel.ProductPriceModel priceModel)
    {
        //add to cart button
        priceModel.DisableBuyButton = product.DisableBuyButton ||
                                      !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart) ||
                                      !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

        //add to wishlist button
        priceModel.DisableWishlistButton = product.DisableWishlistButton ||
                                           !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) ||
                                           !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);
        //compare products
        priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;

        //rental
        priceModel.IsRental = product.IsRental;

        //pre-order
        if (product.AvailableForPreOrder)
        {
            priceModel.AvailableForPreOrder = !product.PreOrderAvailabilityStartDateTimeUtc.HasValue ||
                                              product.PreOrderAvailabilityStartDateTimeUtc.Value >=
                                              DateTime.UtcNow;
            priceModel.PreOrderAvailabilityStartDateTimeUtc = product.PreOrderAvailabilityStartDateTimeUtc;
        }

        //prices
        if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
        {
            if (product.CustomerEntersPrice)
                return;

            if (product.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                 _workContext.OriginalCustomerIfImpersonated == null))
            {
                //call for price
                priceModel.OldPrice = null;
                priceModel.OldPriceValue = null;
                priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                priceModel.PriceValue = null;
            }
            else
            {
                var store = await _storeContext.GetCurrentStoreAsync();
                var customer = await _workContext.GetCurrentCustomerAsync();

                //prices
                var (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount) = (decimal.Zero, decimal.Zero);
                var hasMultiplePrices = false;
                if (_catalogSettings.DisplayFromPrices)
                {
                    var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
                    var cacheKey = _staticCacheManager
                        .PrepareKeyForDefaultCache(NopCatalogDefaults.ProductMultiplePriceCacheKey, product, customerRoleIds, store);
                    if (!_catalogSettings.CacheProductPrices || product.IsRental)
                        cacheKey.CacheTime = 0;

                    var cachedPrice = await _staticCacheManager.GetAsync(cacheKey, async () =>
                    {
                        var prices = new List<(decimal PriceWithoutDiscount, decimal PriceWithDiscount)>();

                        // price when there are no required attributes
                        var attributesMappings = await _productAttributeService.GetProductAttributeMappingsByProductIdAsync(product.Id);
                        if (!attributesMappings.Any(am => !am.IsNonCombinable() && am.IsRequired))
                        {
                            (var priceWithoutDiscount, var priceWithDiscount, _, _) = await _priceCalculationService
                                .GetFinalPriceAsync(product, customer, store);
                            prices.Add((priceWithoutDiscount, priceWithDiscount));
                        }

                        var allAttributesXml = await _productAttributeParser.GenerateAllCombinationsAsync(product, true);
                        foreach (var attributesXml in allAttributesXml)
                        {
                            var warnings = new List<string>();
                            warnings.AddRange(await _shoppingCartService.GetShoppingCartItemAttributeWarningsAsync(customer,
                                ShoppingCartType.ShoppingCart, product, 1, attributesXml, true, true, true));
                            if (warnings.Any())
                                continue;

                            //get price with additional charge
                            var combination = await _productAttributeParser.FindProductAttributeCombinationAsync(product, attributesXml);
                            if (combination?.OverriddenPrice.HasValue ?? false)
                            {
                                (var priceWithoutDiscount, var priceWithDiscount, _, _) = await _priceCalculationService
                                    .GetFinalPriceAsync(product, customer, store, combination.OverriddenPrice.Value, decimal.Zero, true, 1, null, null);
                                prices.Add((priceWithoutDiscount, priceWithDiscount));
                            }
                            else
                            {
                                var additionalCharge = decimal.Zero;
                                var attributeValues = await _productAttributeParser.ParseProductAttributeValuesAsync(attributesXml);
                                foreach (var attributeValue in attributeValues)
                                {
                                    additionalCharge += await _priceCalculationService.
                                        GetProductAttributeValuePriceAdjustmentAsync(product, attributeValue, customer, store);
                                }
                                if (additionalCharge != decimal.Zero)
                                {
                                    (var priceWithoutDiscount, var priceWithDiscount, _, _) = await _priceCalculationService
                                        .GetFinalPriceAsync(product, customer, store, additionalCharge);
                                    prices.Add((priceWithoutDiscount, priceWithDiscount));
                                }
                            }
                        }

                        if (prices.Distinct().Count() > 1)
                        {
                            (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount) = prices.OrderBy(p => p.PriceWithDiscount).First();
                            return new
                            {
                                PriceWithoutDiscount = minPossiblePriceWithoutDiscount,
                                PriceWithDiscount = minPossiblePriceWithDiscount
                            };
                        }

                        // show default price when required attributes available but no values added
                        (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store);

                        //don't cache (return null) if there are no multiple prices
                        return null;
                    });

                    if (cachedPrice is not null)
                    {
                        hasMultiplePrices = true;
                        (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount) = (cachedPrice.PriceWithoutDiscount, cachedPrice.PriceWithDiscount);
                    }
                }
                else
                    (minPossiblePriceWithoutDiscount, minPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store);

                if (product.HasTierPrices)
                {
                    var (tierPriceMinPossiblePriceWithoutDiscount, tierPriceMinPossiblePriceWithDiscount, _, _) = await _priceCalculationService.GetFinalPriceAsync(product, customer, store, quantity: int.MaxValue);

                    //calculate price for the maximum quantity if we have tier prices, and choose minimal
                    minPossiblePriceWithoutDiscount = Math.Min(minPossiblePriceWithoutDiscount, tierPriceMinPossiblePriceWithoutDiscount);
                    minPossiblePriceWithDiscount = Math.Min(minPossiblePriceWithDiscount, tierPriceMinPossiblePriceWithDiscount);
                }

                #region B2B specific change for list price strike through as old price

                var strikeThroughPrice = decimal.Zero;
                var oldPriceBase = decimal.Zero;
                var erpAccount = await _erpCustomerFunctionality.GetActiveErpAccountByCustomerAsync(customer);
                if (erpAccount != null)
                {
                    var specialPrice = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(erpAccount.Id, product.Id);
                    if (specialPrice != null && specialPrice.ListPrice > 0)
                    {
                        oldPriceBase = specialPrice.ListPrice;
                    }

                }

                #endregion

                var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithoutDiscount);
                var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, minPossiblePriceWithDiscount);

                (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, oldPriceBase);
                var currentCurrency = await _workContext.GetWorkingCurrencyAsync();
                var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, currentCurrency);
                var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase, currentCurrency);
                var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, currentCurrency);
                if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                    strikeThroughPrice = oldPrice;

                if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                    strikeThroughPrice = finalPriceWithoutDiscount;

                if (strikeThroughPrice > decimal.Zero)
                {
                    priceModel.OldPrice = await _priceFormatter.FormatPriceAsync(strikeThroughPrice);
                    priceModel.OldPriceValue = strikeThroughPrice;
                }
                else
                {
                    priceModel.OldPrice = null;
                    priceModel.OldPriceValue = null;
                }

                //do we have tier prices configured?
                var tierPrices = product.HasTierPrices
                    ? await _productService.GetTierPricesAsync(product, customer, store)
                    : new List<TierPrice>();

                //When there is just one tier price (with  qty 1), there are no actual savings in the list.
                var hasTierPrices = tierPrices.Any() && !(tierPrices.Count == 1 && tierPrices[0].Quantity <= 1);

                var price = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                priceModel.Price = hasTierPrices || hasMultiplePrices
                    ? string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), price)
                    : price;
                priceModel.PriceValue = finalPriceWithDiscount;

                if (product.IsRental)
                {
                    //rental product
                    priceModel.OldPrice = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceModel.OldPrice);
                    priceModel.Price = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceModel.Price);
                }

                //property for German market
                //we display tax/shipping info only with "shipping enabled" for this product
                //we also ensure this it's not free shipping
                priceModel.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductBoxes && product.IsShipEnabled && !product.IsFreeShipping;

                //PAngV default baseprice (used in Germany)
                priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscount);
                priceModel.BasePricePAngVValue = finalPriceWithDiscount;

                #region Custom (B2B)
                if (minPossiblePriceWithoutDiscount == decimal.Zero)
                {
                    //call for price
                    priceModel.OldPrice = null;
                    priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                    priceModel.BasePricePAngV = null;
                }
                #endregion
            }
        }
        else
        {
            //hide prices
            priceModel.OldPrice = null;
            priceModel.OldPriceValue = null;
            priceModel.Price = null;
            priceModel.PriceValue = null;
        }
    }

    /// <summary>
    /// Prepare the product price model
    /// </summary>
    /// <param name="product">Product</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the product price model
    /// </returns>
    protected override async Task<ProductDetailsModel.ProductPriceModel> PrepareProductPriceModelAsync(Product product)
    {
        ArgumentNullException.ThrowIfNull(product);

        var model = new ProductDetailsModel.ProductPriceModel
        {
            ProductId = product.Id
        };

        if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
        {
            model.HidePrices = false;
            if (product.CustomerEntersPrice)
            {
                model.CustomerEntersPrice = true;
            }
            else
            {
                if (product.CallForPrice &&
                    //also check whether the current user is impersonated
                    (!_orderSettings.AllowAdminsToBuyCallForPriceProducts || _workContext.OriginalCustomerIfImpersonated == null))
                {
                    model.CallForPrice = true;
                }
                else
                {
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var currentCurrency = await _workContext.GetWorkingCurrencyAsync();

                    #region B2B specific change for list price strike through as old price

                    var oldPriceBase = decimal.Zero;
                    var erpAccount = await _erpCustomerFunctionality.GetActiveErpAccountByCustomerAsync(customer);
                    if (erpAccount != null)
                    {
                        var specialPrice = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(erpAccount.Id, product.Id);
                        if (specialPrice != null && specialPrice.ListPrice > 0)
                        {
                            oldPriceBase = specialPrice.ListPrice;
                        }

                    }

                    #endregion

                    var (finalPriceWithoutDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, customer, store, includeDiscounts: false)).finalPrice);
                    var (finalPriceWithDiscountBase, _) = await _taxService.GetProductPriceAsync(product, (await _priceCalculationService.GetFinalPriceAsync(product, customer, store)).finalPrice);
                    
                    (oldPriceBase, _) = await _taxService.GetProductPriceAsync(product, oldPriceBase);
                    var oldPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(oldPriceBase, currentCurrency);
                    var finalPriceWithoutDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithoutDiscountBase, currentCurrency);
                    var finalPriceWithDiscount = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceWithDiscountBase, currentCurrency);

                    if (finalPriceWithoutDiscountBase != oldPriceBase && oldPriceBase > decimal.Zero)
                    {
                        model.OldPrice = await _priceFormatter.FormatPriceAsync(oldPrice);
                        model.OldPriceValue = oldPrice;
                    }

                    model.Price = await _priceFormatter.FormatPriceAsync(finalPriceWithoutDiscount);

                    if (finalPriceWithoutDiscountBase != finalPriceWithDiscountBase)
                    {
                        model.PriceWithDiscount = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                        model.PriceWithDiscountValue = finalPriceWithDiscount;
                    }

                    model.PriceValue = finalPriceWithDiscount;

                    //property for German market
                    //we display tax/shipping info only with "shipping enabled" for this product
                    //we also ensure this it's not free shipping
                    model.DisplayTaxShippingInfo = _catalogSettings.DisplayTaxShippingInfoProductDetailsPage
                                                   && product.IsShipEnabled &&
                                                   !product.IsFreeShipping;

                    //PAngV baseprice (used in Germany)
                    model.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceWithDiscountBase);
                    model.BasePricePAngVValue = finalPriceWithDiscountBase;
                    //currency code
                    model.CurrencyCode = currentCurrency.CurrencyCode;

                    //rental
                    if (product.IsRental)
                    {
                        model.IsRental = true;
                        var priceStr = await _priceFormatter.FormatPriceAsync(finalPriceWithDiscount);
                        model.RentalPrice = await _priceFormatter.FormatRentalProductPeriodAsync(product, priceStr);
                        model.RentalPriceValue = finalPriceWithDiscount;
                    }

                    #region Custom (B2B)

                    if (finalPriceWithoutDiscount == decimal.Zero)
                    {
                        model.CallForPrice = true;
                        model.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                    }

                    #endregion
                }
            }
        }
        else
        {
            model.HidePrices = true;
            model.OldPrice = null;
            model.OldPriceValue = null;
            model.Price = null;
        }

        return model;
    }

    /// <summary>
    /// Prepare the grouped product overview price model
    /// </summary>
    /// <param name="product">Product</param>
    /// <param name="priceModel">Price model</param>
    /// <returns>A task that represents the asynchronous operation</returns>
    protected override async Task PrepareGroupedProductOverviewPriceModelAsync(Product product, ProductOverviewModel.ProductPriceModel priceModel)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var associatedProducts = await _productService.GetAssociatedProductsAsync(product.Id,
            store.Id);

        //add to cart button (ignore "DisableBuyButton" property for grouped products)
        priceModel.DisableBuyButton =
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart) ||
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

        //add to wishlist button (ignore "DisableWishlistButton" property for grouped products)
        priceModel.DisableWishlistButton =
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist) ||
            !await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices);

        //compare products
        priceModel.DisableAddToCompareListButton = !_catalogSettings.CompareProductsEnabled;
        if (!associatedProducts.Any())
            return;

        //we have at least one associated product
        if (await _permissionService.AuthorizeAsync(StandardPermissionProvider.DisplayPrices))
        {
            //find a minimum possible price
            decimal? minPossiblePrice = null;
            Product minPriceProduct = null;
            var customer = await _workContext.GetCurrentCustomerAsync();
            foreach (var associatedProduct in associatedProducts)
            {
                var (_, tmpMinPossiblePrice, _, _) = await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer, store);

                if (associatedProduct.HasTierPrices)
                {
                    //calculate price for the maximum quantity if we have tier prices, and choose minimal
                    tmpMinPossiblePrice = Math.Min(tmpMinPossiblePrice,
                        (await _priceCalculationService.GetFinalPriceAsync(associatedProduct, customer, store, quantity: int.MaxValue)).finalPrice);
                }

                if (minPossiblePrice.HasValue && tmpMinPossiblePrice >= minPossiblePrice.Value)
                    continue;
                minPriceProduct = associatedProduct;
                minPossiblePrice = tmpMinPossiblePrice;
            }

            if (minPriceProduct == null || minPriceProduct.CustomerEntersPrice)
                return;

            if (minPriceProduct.CallForPrice &&
                //also check whether the current user is impersonated
                (!_orderSettings.AllowAdminsToBuyCallForPriceProducts ||
                 _workContext.OriginalCustomerIfImpersonated == null))
            {
                priceModel.OldPrice = null;
                priceModel.OldPriceValue = null;
                priceModel.Price = await _localizationService.GetResourceAsync("Products.CallForPrice");
                priceModel.PriceValue = null;
            }
            else
            {
                //calculate prices
                var (finalPriceBase, _) = await _taxService.GetProductPriceAsync(minPriceProduct, minPossiblePrice.Value);
                var finalPrice = await _currencyService.ConvertFromPrimaryStoreCurrencyAsync(finalPriceBase, await _workContext.GetWorkingCurrencyAsync());

                priceModel.OldPrice = null;
                priceModel.OldPriceValue = null;
                priceModel.Price = finalPrice > decimal.Zero ? string.Format(await _localizationService.GetResourceAsync("Products.PriceRangeFrom"), await _priceFormatter.FormatPriceAsync(finalPrice)) : await _localizationService.GetResourceAsync("Products.CallForPrice");

                priceModel.PriceValue = finalPrice;

                //PAngV default baseprice (used in Germany)
                priceModel.BasePricePAngV = await _priceFormatter.FormatBasePriceAsync(product, finalPriceBase);
                priceModel.BasePricePAngVValue = finalPriceBase;
            }
        }
        else
        {
            //hide prices
            priceModel.OldPrice = null;
            priceModel.OldPriceValue = null;
            priceModel.Price = null;
            priceModel.PriceValue = null;
        }
    }

    #endregion
}
