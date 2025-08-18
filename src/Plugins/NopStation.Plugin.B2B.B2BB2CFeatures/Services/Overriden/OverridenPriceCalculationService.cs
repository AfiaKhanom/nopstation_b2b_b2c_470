using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Directory;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Stores;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.Orders;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;

public class OverridenPriceCalculationService : PriceCalculationService
{
    #region Fields

    private readonly IOrderService _orderService;
    private readonly IErpCustomerFunctionalityService _erpCustomerFunctionality;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IErpGroupPriceService _erpGroupPriceService;
    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IErpProductService _erpProductService;
    protected readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpAccountService _erpAccountService;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public OverridenPriceCalculationService(CatalogSettings catalogSettings,
        CurrencySettings currencySettings,
        ICategoryService categoryService,
        ICurrencyService currencyService,
        ICustomerService customerService,
        IDiscountService discountService,
        IManufacturerService manufacturerService,
        IProductAttributeParser productAttributeParser,
        IProductService productService,
        IStaticCacheManager staticCacheManager,
        IOrderService orderService,
        IErpCustomerFunctionalityService erpCustomerFunctionality,
        IGenericAttributeService genericAttributeService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IErpGroupPriceService erpGroupPriceService,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpProductService erpProductService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpAccountService erpAccountService,
        IStoreContext storeContext,
        ISettingService settingService) :
        base(catalogSettings,
            currencySettings,
            categoryService,
            currencyService,
            customerService,
            discountService,
            manufacturerService,
            productAttributeParser,
            productService,
            staticCacheManager)
    {
        _orderService = orderService;
        _erpCustomerFunctionality = erpCustomerFunctionality;
        _genericAttributeService = genericAttributeService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _erpGroupPriceService = erpGroupPriceService;
        _erpSpecialPriceService = erpSpecialPriceService;
        _erpProductService = erpProductService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpAccountService = erpAccountService;
        _storeContext = storeContext;
        _settingService = settingService;
    }

    #endregion

    #region Utilities

    private async Task<(decimal, decimal)> GetGroupPriceAsync(ErpAccount erpAccount, int productId)
    {
        var priceGroupProductPricing = await _erpGroupPriceService
            .GetErpGroupPriceByErpPriceGroupCodeAndProductId(erpAccount.B2BPriceGroupCodeId ?? 0, productId);

        if (priceGroupProductPricing != null && priceGroupProductPricing.Id > 0)
        {
            return (priceGroupProductPricing.Price, decimal.Zero);
        }

        return (decimal.Zero, decimal.Zero);
    }

    private async Task<(decimal, decimal)> GetSpecialPriceAsync(ErpAccount erpAccount, int productId)
    {
        var productSpecialPricing = await _erpSpecialPriceService
            .GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(erpAccount.Id, productId);

        if (productSpecialPricing != null && productSpecialPricing.Id > 0)
        {
            return (productSpecialPricing.Price, productSpecialPricing.DiscountPerc);            
        }

        return (decimal.Zero, decimal.Zero);
    }

    public async Task<(decimal, decimal)> GetErpProductPriceAndDiscountPercByErpAccountAndProduct(ErpAccount erpAccount, int productId, B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        if (b2BB2CFeaturesSettings.UseProductCombinedPrice)
        {
            var specialPrice = await GetSpecialPriceAsync(erpAccount, productId);
            if (specialPrice.Item1 > 0)
                return specialPrice;

            return await GetGroupPriceAsync(erpAccount, productId);
        }

        if (b2BB2CFeaturesSettings.UseProductGroupPrice)
        {
            return await GetGroupPriceAsync(erpAccount, productId);
        }

        return await GetSpecialPriceAsync(erpAccount, productId);
    }

    #endregion

    #region Methods

    public override async Task<(decimal priceWithoutDiscounts, decimal finalPrice, decimal appliedDiscountAmount, List<Discount> appliedDiscounts)>
        GetFinalPriceAsync(Product product, Customer customer, Store store, decimal? overriddenProductPrice, decimal additionalCharge, bool includeDiscounts, int quantity, DateTime? rentalStartDate, DateTime? rentalEndDate)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ProductPriceCacheKey,
            product,
            overriddenProductPrice,
            additionalCharge,
            includeDiscounts,
            quantity,
            await _customerService.GetCustomerRoleIdsAsync(customer),
            store);

        //we do not cache price if this not allowed by settings or if the product is rental product
        //otherwise, it can cause memory leaks (to store all possible date period combinations)
        if (!_catalogSettings.CacheProductPrices || product.IsRental)
            cacheKey.CacheTime = 0;

        decimal rezPrice;
        decimal rezPriceWithoutDiscount;
        decimal discountAmount;
        List<Discount> appliedDiscounts;

        (rezPriceWithoutDiscount, rezPrice, discountAmount, appliedDiscounts) = await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var discounts = new List<Discount>();
            var appliedDiscountAmount = decimal.Zero;

            //initial price
            var price = overriddenProductPrice ?? product.Price;

            //ToDo: get the initial price according to customer identity from ERP

            #region ERP B2B

            var priceTakenFromQuoteOrderItem = false;
            var erpUser = await _erpCustomerFunctionality.GetActiveErpNopUserByCustomerAsync(customer);
            var erpAccount = await _erpAccountService.GetErpAccountByIdWithActiveAsync(erpUser?.ErpAccountId ?? 0);

            if (erpAccount != null)
            {
                var b2bOrderId = await _genericAttributeService.GetAttributeAsync<int>(customer, B2BB2CFeaturesDefaults.B2BConvertedQuoteB2BOrderId, store.Id);
                var b2COrderId = await _genericAttributeService.GetAttributeAsync<int>(customer, B2BB2CFeaturesDefaults.B2CConvertedQuoteB2COrderId, store.Id);

                if (b2bOrderId > 0)
                {
                    var b2BOrderPerAccount = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(b2bOrderId);
                    if (await _erpOrderAdditionalDataService.CheckQuoteOrderStatusAsync(b2BOrderPerAccount))
                    {
                        var quoteOrderItems = await _orderService.GetOrderItemsAsync(b2BOrderPerAccount.NopOrderId);
                        var quoteOrderItem = quoteOrderItems?.Where(x => x.ProductId == product.Id)?.FirstOrDefault();

                        if (quoteOrderItem != null && quantity >= quoteOrderItem.Quantity)
                        {
                            priceTakenFromQuoteOrderItem = true;

                            if (b2BOrderPerAccount.ErpOrderOriginType == ErpOrderOriginType.OnlineOrder
                            && quoteOrderItem.DiscountAmountExclTax > 0 && quoteOrderItem.Quantity > 0)
                            {
                                var discountPerUnitProduct = quoteOrderItem.DiscountAmountExclTax / quoteOrderItem.Quantity;
                                price = quoteOrderItem.UnitPriceExclTax + discountPerUnitProduct;
                            }
                            else
                            {
                                price = quoteOrderItem.UnitPriceExclTax;
                            }
                        }
                    }
                }
                else if (b2COrderId > 0)
                {
                    var b2COrderPerUser = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(b2COrderId);
                    if (await _erpOrderAdditionalDataService.CheckQuoteOrderStatusAsync(b2COrderPerUser))
                    {
                        var quoteOrderItems = await _orderService.GetOrderItemsAsync(b2COrderPerUser.NopOrderId);
                        var quoteOrderItem = quoteOrderItems?.Where(x => x.ProductId == product.Id)?.FirstOrDefault();
                        if (quoteOrderItem != null && quantity >= quoteOrderItem.Quantity)
                        {
                            priceTakenFromQuoteOrderItem = true;

                            if (b2COrderPerUser.ErpOrderOriginType == ErpOrderOriginType.OnlineOrder
                            && quoteOrderItem.DiscountAmountExclTax > 0 && quoteOrderItem.Quantity > 0)
                            {
                                var discountPerUnitProduct = quoteOrderItem.DiscountAmountExclTax / quoteOrderItem.Quantity;
                                price = quoteOrderItem.UnitPriceExclTax + discountPerUnitProduct;
                            }
                            else
                            {
                                price = quoteOrderItem.UnitPriceExclTax;
                            }
                        }
                    }
                }

                    if (!priceTakenFromQuoteOrderItem && !_b2BB2CFeaturesSettings.UseNopProductPrice
                    && (_b2BB2CFeaturesSettings.UseProductGroupPrice || _b2BB2CFeaturesSettings.UseProductSpecialPrice || _b2BB2CFeaturesSettings.UseProductCombinedPrice))
                    {
                        var discountPerc = decimal.Zero;
                        (price, discountPerc) = await GetErpProductPriceAndDiscountPercByErpAccountAndProduct(erpAccount, product.Id, _b2BB2CFeaturesSettings);
                    }
                }

            #endregion

            //tier prices
            var tierPrice = await _productService.GetPreferredTierPriceAsync(product, customer, store, quantity);

            if (tierPrice != null)
                price = tierPrice.Price;

            //additional charge
            price += additionalCharge;

            //rental products
            if (product.IsRental && rentalStartDate.HasValue && rentalEndDate.HasValue)
                price *= _productService.GetRentalPeriods(product, rentalStartDate.Value, rentalEndDate.Value);

            var priceWithoutDiscount = price;

            if (includeDiscounts)
            {
                //discount
                var (tmpDiscountAmount, tmpAppliedDiscounts) = await GetDiscountAmountAsync(product, customer, price);
                price -= tmpDiscountAmount;

                if (tmpAppliedDiscounts?.Any() ?? false)
                {
                    discounts.AddRange(tmpAppliedDiscounts);
                    appliedDiscountAmount = tmpDiscountAmount;
                }
            }

            if (price < decimal.Zero)
                price = decimal.Zero;

            if (priceWithoutDiscount < decimal.Zero)
                priceWithoutDiscount = decimal.Zero;

            return (priceWithoutDiscount, price, appliedDiscountAmount, discounts);
        });

        return (rezPriceWithoutDiscount, rezPrice, discountAmount, appliedDiscounts);
    }

    #endregion
}