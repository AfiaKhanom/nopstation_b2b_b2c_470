using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.ErpProductList;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.OrderSummary;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories
{
    public class ErpProductModelFactory : IErpProductModelFactory
    {
        #region Fields

        private readonly IBaseAdminModelFactory _baseAdminModelFactory;
        private readonly IPriceFormatter _priceFormatter;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly IStoreContext _storeContext;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IErpAccountService _erpAccountService;
        private readonly ISettingService _settingService;
        private readonly IPermissionService _permissionService;
        private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IErpSpecialPriceService _erpSpecialPriceService;
        private readonly IShoppingCartService _shoppingCartService;
        private readonly IPriceCalculationService _priceCalculationService;
        private readonly IWorkContext _workContext;


        #endregion

        #region Ctor

        public ErpProductModelFactory(IBaseAdminModelFactory baseAdminModelFactory,
            IPriceFormatter priceFormatter,
            IProductService productService,
            IStoreContext storeContext,
            IStaticCacheManager staticCacheManager,
            IErpAccountService erpAccountService,
            ISettingService settingService,
            IPermissionService permissionService,
            IErpCustomerFunctionalityService erpCustomerFunctionalityService,
            IUrlRecordService urlRecordService,
            IErpSpecialPriceService erpSpecialPriceService,
            IShoppingCartService shoppingCartService,
            IPriceCalculationService priceCalculationService,
            IWorkContext workContext
            )
        {
            _baseAdminModelFactory = baseAdminModelFactory;
            _priceFormatter = priceFormatter;
            _productService = productService;
            _storeContext = storeContext;
            _staticCacheManager = staticCacheManager;
            _erpAccountService = erpAccountService;
            _settingService = settingService;
            _permissionService = permissionService;
            _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
            _urlRecordService = urlRecordService;
            _erpSpecialPriceService = erpSpecialPriceService;
            _shoppingCartService = shoppingCartService;
            _priceCalculationService = priceCalculationService;
            _workContext = workContext;
        }

        #endregion

        #region Method

        public async Task<ErpProductListSearchModel> PrepareErpProductListSearchModelAsync(ErpProductListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            await _baseAdminModelFactory.PrepareCategoriesAsync(searchModel.AvailableCategories);
            await _baseAdminModelFactory.PrepareManufacturersAsync(searchModel.AvailableManufacturers);
            searchModel.SetGridPageSize();
            return searchModel;
        }

        public async Task<ErpProductListModel> PrepareErpProductListModelAsync(ErpProductListSearchModel searchModel)
        {
            if (searchModel == null)
                throw new ArgumentNullException(nameof(searchModel));

            var products = await _productService.SearchProductsAsync(showHidden: true,
                categoryIds: new List<int> { searchModel.SearchCategoryId },
                manufacturerIds: new List<int> { searchModel.SearchManufacturerId },
                keywords: searchModel.SearchProductName,
                pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

            var model = await new ErpProductListModel().PrepareToGridAsync(searchModel, products, () =>
            {
                //fill in model values from the entity
                return products.SelectAwait(async product =>
                {
                    var b2bProductModel = new ErpProductDataModel
                    {
                        Name = product.Name,
                        Sku = product.Sku,
                        ManufacturerPartNumber = product.ManufacturerPartNumber,
                        SeName = await _urlRecordService.GetSeNameAsync(product)
                    };
                    return b2bProductModel;
                });
            });
            return model;
        }

        public async Task<IList<ErpProductDataModel>> PrepareErpProductDataListModelAsync(List<string> productIds, ErpAccount erpAccount, B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
        {
            if (erpAccount == null || productIds == null)
            {
                return null;
            }

            var productDataModels = new List<ErpProductDataModel>();

            if (erpAccount != null && productIds != null)
            {
                foreach (var id in productIds)
                {
                    if (int.TryParse(id, out var productId))
                    {
                        var product = await _productService.GetProductByIdAsync(productId);

                        if (product == null)
                        {
                            continue;
                        }

                        ErpSpecialPrice productPricing = null;
                        if (!b2BB2CFeaturesSettings.UseProductGroupPrice)
                        {
                            productPricing = await _erpSpecialPriceService.GetErpSpecialPricesByErpAccountIdAndNopProductIdAsync(erpAccount.Id, productId);
                        }

                        var shoppingCartItemQuantity = (await _workContext.GetCurrentCustomerAsync()).HasShoppingCartItems ?
                            (await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id, productId))?.FirstOrDefault()?.Quantity ?? 0 : 0;
                        var displayBackInStockSubscription = false;
                        if (await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BStock) &&
                            product.ManageInventoryMethod == ManageInventoryMethod.ManageStock &&
                            product.BackorderMode == BackorderMode.NoBackorders &&
                            product.AllowBackInStockSubscriptions &&
                            await _productService.GetTotalStockQuantityAsync(product) <= 0)
                        {
                            displayBackInStockSubscription = true;
                        }



                        var stockAvailability = await _productService.FormatStockMessageAsync(product, "");
                        //_localizationService.GetResource("Products.Availability.OutOfStock")
                        var isOutOfStock = stockAvailability == await _localizationService.GetResourceAsync("Products.Availability.OutOfStock") ? true : false;

                        var b2bProductData = new ErpProductDataModel()
                        {
                            Id = productId,
                            ManufacturerPartNumber = product.ManufacturerPartNumber ?? string.Empty,
                            IsOutOfStock = isOutOfStock,
                            StockAvailability = await _productService.FormatStockMessageAsync(product, ""),
                            UOM = string.Empty, //ToDo
                            Quantity = shoppingCartItemQuantity,
                            DisplayBackInStockSubscription = displayBackInStockSubscription
                        };

                        if (await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BPrices))
                        {
                            b2bProductData.PricingNotes = b2BB2CFeaturesSettings.UseProductGroupPrice || productPricing == null ? string.Empty : productPricing.Price == 0 ? await _localizationService.GetResourceAsync("Products.CallForPrice") : (await _erpSpecialPriceService.GetErpSpecialPriceByIdAsync(productPricing.Id))?.PricingNote ?? string.Empty;
                        }
                        else
                        {
                            b2bProductData.PricingNotes = string.Empty;
                        }

                        productDataModels.Add(b2bProductData);

                    }
                }
            }
            return productDataModels;
        }

        public async Task<ProductInCartQuantityModel> PrepareProductInCartQuantityModelAsync(int productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return new ProductInCartQuantityModel();

            var currCustomer = await _workContext.GetCurrentCustomerAsync();

            var shoppingCartItemQuantity = currCustomer.HasShoppingCartItems ?
                            (await _shoppingCartService.GetShoppingCartAsync(currCustomer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id, product.Id))?.FirstOrDefault()?.Quantity ?? 0 : 0;
            return new ProductInCartQuantityModel
            {
                Id = product.Id,
                Quantity = shoppingCartItemQuantity
            };
        }

        public async Task<decimal> GetErpProductPriceAsync(int productId)
        {
            var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(B2BB2CFeaturesDefaults.ErpProductModelProductPriceCacheKey, productId);
            return await _staticCacheManager.GetAsync(cacheKey, async () =>
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                    return decimal.Zero;

                (decimal priceWithoutDiscounts, decimal finalPrice, decimal appliedDiscountAmount, List<Discount> appliedDiscounts) = await _priceCalculationService.GetFinalPriceAsync(product, await _workContext.GetCurrentCustomerAsync(), await _storeContext.GetCurrentStoreAsync(), includeDiscounts: true);
                return finalPrice;
            });
        }

        public async Task<IList<ProductInCartQuantityModel>> PrepareProductInCartQuantityModelAsync(List<string> productIds)
        {
            var productInCartQuantityModels = new List<ProductInCartQuantityModel>();

            if (productIds != null)
            {
                foreach (var id in productIds)
                {
                    if (int.TryParse(id, out var productId))
                    {
                        var product = await _productService.GetProductByIdAsync(productId);

                        if (product == null)
                        {
                            continue;
                        }
                        var currentCustomer = await _workContext.GetCurrentCustomerAsync();
                        var shoppingCartItemQuantity = currentCustomer.HasShoppingCartItems ?
                            (await _shoppingCartService.GetShoppingCartAsync(currentCustomer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id, productId))?.FirstOrDefault()?.Quantity ?? 0 : 0;
                        productInCartQuantityModels.Add(new ProductInCartQuantityModel
                        {
                            Id = productId,
                            Quantity = shoppingCartItemQuantity
                        });
                    }
                }
            }
            return productInCartQuantityModels;
        }

        public async Task<string> UpdateLiveStockAndGetProductAvailabilityAsync(int productId, ErpAccount erpAccount)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
                return null;
            //var b2BB2CFeaturesSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);
            //var categoryIds = b2BB2CFeaturesSettings.SkipLiveStockCheckCategoryIds.Split(',').Select(int.Parse).ToList();
            //var productCategories = _catalogService.Cat product.ProductCategories;
            //foreach (var cat in productCategories)
            //{
            //    if (categoryIds.Any(a => a == cat.CategoryId))
            //        return await _productService.FormatStockMessageAsync(product, "");
            //}

            //if (b2BB2CFeaturesSettings.EnableLiveStockChecks)
            //{
            //    product = await _erpIntegrationProductService.GetProductsFromErpAsync();
            //}

            var stockAvailability = await _productService.FormatStockMessageAsync(product, "");
            return stockAvailability;
        }

        public async Task<ErpOrderSummaryModel> PrepareErpOrderSummaryModelAsync()
        {
            var orderSummeryModel = new ErpOrderSummaryModel();
            var totalPriceWithOutSavings = decimal.Zero;
            var b2bOnlineOrderDiscount = decimal.Zero;
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (currentCustomer.HasShoppingCartItems)
            {
                //var b2BB2CFeaturesSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);

                var shoppingCartItems = (await _shoppingCartService.GetShoppingCartAsync(currentCustomer))?.Where(x => x.ShoppingCartType == ShoppingCartType.ShoppingCart).ToList();

                var b2bAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currentCustomer.Id);

                if (b2bAccount == null)
                    return new ErpOrderSummaryModel();

                var isBackorderAllowed = b2bAccount.AllowAccountsBackOrdering;

                foreach (var cartItemProduct in shoppingCartItems)
                {
                    var product = await _productService.GetProductByIdAsync(cartItemProduct.ProductId);
                    if (product == null)
                    {
                        continue;
                    }

                    var productQuantity = cartItemProduct.Quantity;
                    if (isBackorderAllowed)
                        productQuantity = await _productService.GetTotalStockQuantityAsync(product);

                    (decimal unitPrice, decimal discountAmount, List<Discount> appliedDiscounts) = await _shoppingCartService.GetUnitPriceAsync(cartItemProduct, false);
                    var b2bProductPrice = unitPrice + discountAmount;
                    totalPriceWithOutSavings += b2bProductPrice * cartItemProduct.Quantity;
                    b2bOnlineOrderDiscount += discountAmount * cartItemProduct.Quantity;

                    orderSummeryModel.Items.Add(new ErpOrderSummaryModel.ErpOrderSummaryItemModel
                    {
                        Id = cartItemProduct.ProductId,
                        Weight = 0,
                        WeightValue = "",
                        StockAvailability = await _productService.FormatStockMessageAsync(product, ""),
                        Uom = string.Empty,
                        IsBackOrder = productQuantity < cartItemProduct.Quantity,
                        UnitPriceWithOutDiscount = await _priceFormatter.FormatPriceAsync(b2bProductPrice),
                        DiscountForPerUnitProduct = await _priceFormatter.FormatPriceAsync(discountAmount)
                    });
                }
                orderSummeryModel.TotalPriceWithOutSavings = await _priceFormatter.FormatPriceAsync(totalPriceWithOutSavings);
                orderSummeryModel.ErpOnlineOrderDiscount = await _priceFormatter.FormatPriceAsync(b2bOnlineOrderDiscount);
            }
            return orderSummeryModel;
        }

        #endregion

    }
}