using System;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Spreadsheet;
using MailKit.Search;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Web.Framework;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Infrastructure;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;

public class OverridenProductService : ProductService
{
    #region Fields

    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IErpAccountService _erpAccountService;
    private readonly IPermissionService _permissionService;
    private B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly ICategoryService _categoryService;
    private readonly IErpNopUserService _erpNopUserService;
    private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    #endregion

    #region Ctor

    public OverridenProductService(CatalogSettings catalogSettings,
        CommonSettings commonSettings,
        IAclService aclService,
        ICustomerService customerService,
        IDateRangeService dateRangeService,
        ILanguageService languageService,
        ILocalizationService localizationService,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IRepository<Category> categoryRepository,
        IRepository<CrossSellProduct> crossSellProductRepository,
        IRepository<DiscountProductMapping> discountProductMappingRepository,
        IRepository<LocalizedProperty> localizedPropertyRepository,
        IRepository<Manufacturer> manufacturerRepository,
        IRepository<Product> productRepository,
        IRepository<ProductAttributeCombination> productAttributeCombinationRepository,
        IRepository<ProductAttributeMapping> productAttributeMappingRepository,
        IRepository<ProductCategory> productCategoryRepository,
        IRepository<ProductManufacturer> productManufacturerRepository,
        IRepository<ProductPicture> productPictureRepository,
        IRepository<ProductProductTagMapping> productTagMappingRepository,
        IRepository<ProductReview> productReviewRepository,
        IRepository<ProductReviewHelpfulness> productReviewHelpfulnessRepository,
        IRepository<ProductSpecificationAttribute> productSpecificationAttributeRepository,
        IRepository<ProductTag> productTagRepository,
        IRepository<ProductVideo> productVideoRepository,
        IRepository<ProductWarehouseInventory> productWarehouseInventoryRepository,
        IRepository<RelatedProduct> relatedProductRepository,
        IRepository<Shipment> shipmentRepository,
        IRepository<StockQuantityHistory> stockQuantityHistoryRepository,
        IRepository<TierPrice> tierPriceRepository,
        ISearchPluginManager searchPluginManager,
        IStaticCacheManager staticCacheManager,
        IStoreService storeService,
        IStoreMappingService storeMappingService,
        IWorkContext workContext,
        LocalizationSettings localizationSettings,
        IActionContextAccessor actionContextAccessor,
        IErpAccountService erpAccountService,
        IPermissionService permissionService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpSpecificationAttributeService erpSpecificationAttributeService,
        IStoreContext storeContext,
        ISpecificationAttributeService specificationAttributeService,
        ISettingService settingService,
        ICategoryService categoryService,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpNopUserService erpNopUserService,
        IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
        IGenericAttributeService genericAttributeService
        ) : base(catalogSettings,
            commonSettings,
            aclService,
            customerService,
            dateRangeService,
            languageService,
            localizationService,
            productAttributeParser,
            productAttributeService,
            categoryRepository,
            crossSellProductRepository,
            discountProductMappingRepository,
            localizedPropertyRepository,
            manufacturerRepository,
            productRepository,
            productAttributeCombinationRepository,
            productAttributeMappingRepository,
            productCategoryRepository,
            productManufacturerRepository,
            productPictureRepository,
            productTagMappingRepository,
            productReviewRepository,
            productReviewHelpfulnessRepository,
            productSpecificationAttributeRepository,
            productTagRepository,
            productVideoRepository,
            productWarehouseInventoryRepository,
            relatedProductRepository,
            shipmentRepository,
            stockQuantityHistoryRepository,
            tierPriceRepository,
            searchPluginManager,
            staticCacheManager,
            storeService,
            storeMappingService,
            workContext,
            localizationSettings)
    {
        _actionContextAccessor = actionContextAccessor;
        _erpAccountService = erpAccountService;
        _permissionService = permissionService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpSpecificationAttributeService = erpSpecificationAttributeService;
        _specificationAttributeService = specificationAttributeService;
        _categoryService = categoryService;
        _erpNopUserService = erpNopUserService;
        _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
        _genericAttributeService = genericAttributeService;
        _settingService = settingService;
        _storeContext = storeContext;
    }

    #endregion

    #region Utilites

    private bool IsAdminRoute()
    {
        var haveArea = _actionContextAccessor.ActionContext.RouteData.Values.TryGetValue("area", out var data);
        if (haveArea && data.ToString().Equals(AreaNames.ADMIN))
            return true;

        return false;
    }

    private async Task<ErpAccount> GetErpAccountByCurrentCustomerAsync()
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var impersonatedCustomerId = await _genericAttributeService.GetAttributeAsync<int?>(currCustomer, NopCustomerDefaults.ImpersonatedCustomerIdAttribute);
        return await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(impersonatedCustomerId ?? currCustomer.Id);
    }

    /// <summary>
    /// Get stock message
    /// </summary>
    /// <param name="product">Product</param>
    /// <param name="stockMessage">Message</param>
    /// <returns>Message</returns>
    protected override async Task<string> GetStockMessageAsync(Product product)
    {
        var currCustomer = await _workContext.GetCurrentCustomerAsync();
        var stockMessage = string.Empty;

        // seperate consideration for B2B and non B2B

        #region B2B

        var b2bAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);

        if (b2bAccount != null)
        {
            if (!await _permissionService.AuthorizeAsync(ErpPermissionProvider.DisplayB2BStock))
                return string.Empty;

            var considerableStockDisplayFormatType = _b2BB2CFeaturesSettings.StockDisplayFormat;
            if (b2bAccount.OverrideStockDisplayFormatConfigSetting)
            {
                considerableStockDisplayFormatType = StockDisplayFormat.ShowStockQuantities;
            }

            if (considerableStockDisplayFormatType == StockDisplayFormat.DoNotShowAnyStockAtAll)
                return string.Empty;

            var b2bstockQuantity = await GetTotalStockQuantityAsync(product);
            if (b2bstockQuantity > 0)
            {
                stockMessage = considerableStockDisplayFormatType == StockDisplayFormat.ShowStockQuantities
                    ?
                    //display "in stock" with stock quantity
                    string.Format(await _localizationService.GetResourceAsync("Products.Availability.InStockWithQuantity"), b2bstockQuantity)
                    :
                    //display "in stock" without stock quantity
                    await _localizationService.GetResourceAsync("Products.Availability.InStock");
            }
            else
            {
                //out of stock
                var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(product.ProductAvailabilityRangeId);
                stockMessage = productAvailabilityRange == null
                            ? await _localizationService.GetResourceAsync("Products.Availability.OutOfStock")
                            : string.Format(await _localizationService.GetResourceAsync("Products.Availability.AvailabilityRange"),
                                await _localizationService.GetLocalizedAsync(productAvailabilityRange, range => range.Name));
            }
            return stockMessage;
        }

        #endregion

        if (!product.DisplayStockAvailability)
            return string.Empty;

        var stockQuantity = await GetTotalStockQuantityAsync(product);

        if (stockQuantity > 0)
        {
            if (product.MinStockQuantity >= stockQuantity && product.LowStockActivity == LowStockActivity.Nothing)
            {
                stockMessage = product.DisplayStockQuantity
                    ?
                    //display "low stock" with stock quantity
                    string.Format(await _localizationService.GetResourceAsync("Products.Availability.LowStockWithQuantity"), stockQuantity)
                    :
                    //display "low stock" without stock quantity
                    await _localizationService.GetResourceAsync("Products.Availability.LowStock");
            }
            else
            {
                stockMessage = product.DisplayStockQuantity
                    ?
                    //display "in stock" with stock quantity
                    string.Format(await _localizationService.GetResourceAsync("Products.Availability.InStockWithQuantity"), stockQuantity)
                    :
                    //display "in stock" without stock quantity
                    await _localizationService.GetResourceAsync("Products.Availability.InStock");
            }
        }
        else
        {
            //out of stock
            var productAvailabilityRange = await _dateRangeService.GetProductAvailabilityRangeByIdAsync(product.ProductAvailabilityRangeId);
            switch (product.BackorderMode)
            {
                case BackorderMode.NoBackorders:
                    stockMessage = productAvailabilityRange == null
                        ? await _localizationService.GetResourceAsync("Products.Availability.OutOfStock")
                        : string.Format(await _localizationService.GetResourceAsync("Products.Availability.AvailabilityRange"),
                            await _localizationService.GetLocalizedAsync(productAvailabilityRange, range => range.Name));
                    break;
                case BackorderMode.AllowQtyBelow0:
                    stockMessage = await _localizationService.GetResourceAsync("Products.Availability.InStock");
                    break;
                case BackorderMode.AllowQtyBelow0AndNotifyCustomer:
                    stockMessage = productAvailabilityRange == null
                        ? await _localizationService.GetResourceAsync("Products.Availability.Backordering")
                        : string.Format(await _localizationService.GetResourceAsync("Products.Availability.BackorderingWithDate"),
                            await _localizationService.GetLocalizedAsync(productAvailabilityRange, range => range.Name));
                    break;
            }
        }

        return stockMessage;
    }

    #endregion

    #region Methods

    #region Product

    /// <summary>
    /// Search products
    /// </summary>
    /// <param name="filterableSpecificationAttributeOptionIds">The specification attribute option identifiers applied to loaded products (all pages)</param>
    /// <param name="loadFilterableSpecificationAttributeOptionIds">A value indicating whether we should load the specification attribute option identifiers applied to loaded products (all pages)</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <param name="categoryIds">Category identifiers</param>
    /// <param name="manufacturerId">Manufacturer identifier; 0 to load all records</param>
    /// <param name="storeId">Store identifier; 0 to load all records</param>
    /// <param name="vendorId">Vendor identifier; 0 to load all records</param>
    /// <param name="warehouseId">Warehouse identifier; 0 to load all records</param>
    /// <param name="productType">Product type; 0 to load all records</param>
    /// <param name="visibleIndividuallyOnly">A values indicating whether to load only products marked as "visible individually"; "false" to load all records; "true" to load "visible individually" only</param>
    /// <param name="markedAsNewOnly">A values indicating whether to load only products marked as "new"; "false" to load all records; "true" to load "marked as new" only</param>
    /// <param name="featuredProducts">A value indicating whether loaded products are marked as featured (relates only to categories and manufacturers). 0 to load featured products only, 1 to load not featured products only, null to load all products</param>
    /// <param name="priceMin">Minimum price; null to load all records</param>
    /// <param name="priceMax">Maximum price; null to load all records</param>
    /// <param name="productTagId">Product tag identifier; 0 to load all records</param>
    /// <param name="keywords">Keywords</param>
    /// <param name="searchDescriptions">A value indicating whether to search by a specified "keyword" in product descriptions</param>
    /// <param name="searchManufacturerPartNumber">A value indicating whether to search by a specified "keyword" in manufacturer part number</param>
    /// <param name="searchSku">A value indicating whether to search by a specified "keyword" in product SKU</param>
    /// <param name="searchProductTags">A value indicating whether to search by a specified "keyword" in product tags</param>
    /// <param name="languageId">Language identifier (search for text searching)</param>
    /// <param name="filteredSpecs">Filtered product specification identifiers</param>
    /// <param name="orderBy">Order by</param>
    /// <param name="showHidden">A value indicating whether to show hidden records</param>
    /// <param name="overridePublished">
    /// null - process "Published" property according to "showHidden" parameter
    /// true - load only "Published" products
    /// false - load only "Unpublished" products
    /// </param>
    /// <returns>Products</returns>
    public override async Task<IPagedList<Product>> SearchProductsAsync(
        int pageIndex = 0,
        int pageSize = int.MaxValue,
        IList<int> categoryIds = null,
        IList<int> manufacturerIds = null,
        int storeId = 0,
        int vendorId = 0,
        int warehouseId = 0,
        ProductType? productType = null,
        bool visibleIndividuallyOnly = false,
        bool excludeFeaturedProducts = false,
        decimal? priceMin = null,
        decimal? priceMax = null,
        int productTagId = 0,
        string keywords = null,
        bool searchDescriptions = false,
        bool searchManufacturerPartNumber = true,
        bool searchSku = true,
        bool searchProductTags = false,
        int languageId = 0,
        IList<SpecificationAttributeOption> filteredSpecOptions = null,
        ProductSortingEnum orderBy = ProductSortingEnum.Position,
        bool showHidden = false,
        bool? overridePublished = null)
    {
        #region Default Nop
        _b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeId);
        //some databases don't support int.MaxValue
        if (pageSize == int.MaxValue)
            pageSize = int.MaxValue - 1;

        var productsQuery = _productRepository.Table;

        if (!showHidden)
            productsQuery = productsQuery.Where(p => p.Published);
        else if (overridePublished.HasValue)
            productsQuery = productsQuery.Where(p => p.Published == overridePublished.Value);

        if (!showHidden)
        {
            //apply store mapping constraints
            productsQuery = await _storeMappingService.ApplyStoreMapping(productsQuery, storeId);

            //apply ACL constraints
            var customer = await _workContext.GetCurrentCustomerAsync();
            productsQuery = await _aclService.ApplyAcl(productsQuery, customer);
        }

        productsQuery =
            from p in productsQuery
            where !p.Deleted &&
                (!visibleIndividuallyOnly || p.VisibleIndividually) &&
                (vendorId == 0 || p.VendorId == vendorId) &&
                (
                    warehouseId == 0 ||
                    (
                        !p.UseMultipleWarehouses ? p.WarehouseId == warehouseId :
                            _productWarehouseInventoryRepository.Table.Any(pwi => pwi.WarehouseId == warehouseId && pwi.ProductId == p.Id)
                    )
                ) &&
                (productType == null || p.ProductTypeId == (int)productType) &&
                (showHidden ||
                        DateTime.UtcNow >= (p.AvailableStartDateTimeUtc ?? DateTime.MinValue) &&
                        DateTime.UtcNow <= (p.AvailableEndDateTimeUtc ?? DateTime.MaxValue)
                ) &&
                (priceMin == null || p.Price >= priceMin) &&
                (priceMax == null || p.Price <= priceMax)
            select p;

        if (!string.IsNullOrEmpty(keywords))
        {
            var langs = await _languageService.GetAllLanguagesAsync(showHidden: true);

            //Set a flag which will to points need to search in localized properties. If showHidden doesn't set to true should be at least two published languages.
            var searchLocalizedValue = languageId > 0 && langs.Count >= 2 && (showHidden || langs.Count(l => l.Published) >= 2);
            IQueryable<int> productsByKeywords;

            var customer = await _workContext.GetCurrentCustomerAsync();
            var activeSearchProvider = await _searchPluginManager.LoadPrimaryPluginAsync(customer, storeId);

            if (activeSearchProvider is not null)
            {
                productsByKeywords = (await activeSearchProvider.SearchProductsAsync(keywords, searchLocalizedValue)).AsQueryable();
            }
            else
            {
                productsByKeywords =
                    from p in _productRepository.Table
                    where p.Name.Contains(keywords) ||
                        (searchDescriptions &&
                            (p.ShortDescription.Contains(keywords) || p.FullDescription.Contains(keywords))) ||
                        (searchManufacturerPartNumber && p.ManufacturerPartNumber == keywords) ||
                        (searchSku && p.Sku == keywords)
                    select p.Id;

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                        from lp in _localizedPropertyRepository.Table
                        let checkName = lp.LocaleKey == nameof(Product.Name) &&
                                        lp.LocaleValue.Contains(keywords)
                        let checkShortDesc = searchDescriptions &&
                                        lp.LocaleKey == nameof(Product.ShortDescription) &&
                                        lp.LocaleValue.Contains(keywords)
                        where
                            lp.LocaleKeyGroup == nameof(Product) && lp.LanguageId == languageId && (checkName || checkShortDesc)

                        select lp.EntityId);
                }
            }

            //search by SKU for ProductAttributeCombination
            if (searchSku)
            {
                productsByKeywords = productsByKeywords.Union(
                    from pac in _productAttributeCombinationRepository.Table
                    where pac.Sku == keywords
                    select pac.ProductId);
            }

            //search by category name if admin allows
            if (_catalogSettings.AllowCustomersToSearchWithCategoryName)
            {
                productsByKeywords = productsByKeywords.Union(
                    from pc in _productCategoryRepository.Table
                    join c in _categoryRepository.Table on pc.CategoryId equals c.Id
                    where c.Name.Contains(keywords)
                    select pc.ProductId
                );

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                    from pc in _productCategoryRepository.Table
                    join lp in _localizedPropertyRepository.Table on pc.CategoryId equals lp.EntityId
                    where lp.LocaleKeyGroup == nameof(Category) &&
                          lp.LocaleKey == nameof(Category.Name) &&
                          lp.LocaleValue.Contains(keywords) &&
                          lp.LanguageId == languageId
                    select pc.ProductId);
                }
            }

            //search by manufacturer name if admin allows
            if (_catalogSettings.AllowCustomersToSearchWithManufacturerName)
            {
                productsByKeywords = productsByKeywords.Union(
                    from pm in _productManufacturerRepository.Table
                    join m in _manufacturerRepository.Table on pm.ManufacturerId equals m.Id
                    where m.Name.Contains(keywords)
                    select pm.ProductId
                );

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                    from pm in _productManufacturerRepository.Table
                    join lp in _localizedPropertyRepository.Table on pm.ManufacturerId equals lp.EntityId
                    where lp.LocaleKeyGroup == nameof(Manufacturer) &&
                          lp.LocaleKey == nameof(Manufacturer.Name) &&
                          lp.LocaleValue.Contains(keywords) &&
                          lp.LanguageId == languageId
                    select pm.ProductId);
                }
            }

            if (searchProductTags)
            {
                productsByKeywords = productsByKeywords.Union(
                    from pptm in _productTagMappingRepository.Table
                    join pt in _productTagRepository.Table on pptm.ProductTagId equals pt.Id
                    where pt.Name.Contains(keywords)
                    select pptm.ProductId
                );

                if (searchLocalizedValue)
                {
                    productsByKeywords = productsByKeywords.Union(
                    from pptm in _productTagMappingRepository.Table
                    join lp in _localizedPropertyRepository.Table on pptm.ProductTagId equals lp.EntityId
                    where lp.LocaleKeyGroup == nameof(ProductTag) &&
                          lp.LocaleKey == nameof(ProductTag.Name) &&
                          lp.LocaleValue.Contains(keywords) &&
                          lp.LanguageId == languageId
                    select pptm.ProductId);
                }
            }

            productsQuery =
                from p in productsQuery
                join pbk in productsByKeywords on p.Id equals pbk
                select p;
        }

        if (categoryIds is not null)
        {
            if (categoryIds.Contains(0))
                categoryIds.Remove(0);

            if (categoryIds.Any())
            {
                var productCategoryQuery =
                    from pc in _productCategoryRepository.Table
                    where (!excludeFeaturedProducts || !pc.IsFeaturedProduct) &&
                        categoryIds.Contains(pc.CategoryId)
                    group pc by pc.ProductId into pc
                    select new
                    {
                        ProductId = pc.Key,
                        DisplayOrder = pc.First().DisplayOrder
                    };

                productsQuery =
                    from p in productsQuery
                    join pc in productCategoryQuery on p.Id equals pc.ProductId
                    orderby pc.DisplayOrder, p.Name
                    select p;
            }
        }

        if (manufacturerIds is not null)
        {
            if (manufacturerIds.Contains(0))
                manufacturerIds.Remove(0);

            if (manufacturerIds.Any())
            {
                var productManufacturerQuery =
                    from pm in _productManufacturerRepository.Table
                    where (!excludeFeaturedProducts || !pm.IsFeaturedProduct) &&
                        manufacturerIds.Contains(pm.ManufacturerId)
                    group pm by pm.ProductId into pm
                    select new
                    {
                        ProductId = pm.Key,
                        DisplayOrder = pm.First().DisplayOrder
                    };

                productsQuery =
                    from p in productsQuery
                    join pm in productManufacturerQuery on p.Id equals pm.ProductId
                    orderby pm.DisplayOrder, p.Name
                    select p;
            }
        }

        if (productTagId > 0)
        {
            productsQuery =
                from p in productsQuery
                join ptm in _productTagMappingRepository.Table on p.Id equals ptm.ProductId
                where ptm.ProductTagId == productTagId
                select p;
        }

        #endregion

        #region B2B

        var b2BAccount = await GetErpAccountByCurrentCustomerAsync();

        if (b2BAccount != null && _b2BB2CFeaturesSettings.UsePrefilterFacet)
        {
            if (b2BAccount.PreFilterFacets?.Trim() == null)
            {
                // if b2buser don't pre filter facets then return no products
                return new PagedList<Product>(new List<Product>(), pageIndex, pageSize, 0);
            }

            IList<int> specIds = null;
            if (b2BAccount.PreFilterFacets?.Trim() != null)
                specIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, b2BAccount.PreFilterFacets?.Trim(), b2BAccount.Id);

            if (!specIds.Any())
            {
                // if b2buser filter facets are not exist as SpecificationAttributeOption then return no products
                return new PagedList<Product>(new List<Product>(), pageIndex, pageSize, 0);
            }

            var preFilterFacetSpecIds = new List<int>();

            foreach (var specId in specIds)
            {
                if (!preFilterFacetSpecIds.Contains(specId))
                    preFilterFacetSpecIds.Add(specId);
            }

            //pass pre filter facet identifiers
            if (preFilterFacetSpecIds != null && preFilterFacetSpecIds.Count > 0)
            {
                preFilterFacetSpecIds.Sort();
                //filterable options
                var filterableOptions = await _specificationAttributeService
                    .GetSpecificationAttributeOptionsByIdsAsync(preFilterFacetSpecIds.ToArray());
                if (filteredSpecOptions == null)
                    filteredSpecOptions = new List<SpecificationAttributeOption>();
                foreach (var filterableOption in filterableOptions)
                {
                    filteredSpecOptions.Add(filterableOption);
                }
            }
        }

        #endregion

        if (filteredSpecOptions?.Count > 0)
        {
            var specificationAttributeIds = filteredSpecOptions
                .Select(sao => sao.SpecificationAttributeId)
                .Distinct();

            foreach (var specificationAttributeId in specificationAttributeIds)
            {
                var optionIdsBySpecificationAttribute = filteredSpecOptions
                    .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                    .Select(o => o.Id);

                var productSpecificationQuery =
                    from psa in _productSpecificationAttributeRepository.Table
                    where optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                    select psa;

                productsQuery =
                    from p in productsQuery
                    where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                    select p;
            }
        }

        return await productsQuery.OrderBy(_localizedPropertyRepository, await _workContext.GetWorkingLanguageAsync(), orderBy).ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets all products displayed on the home page
    /// </summary>
    /// <returns>Products</returns>
    public override async Task<IList<Product>> GetAllProductsDisplayedOnHomepageAsync()
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        _b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);
        IList<int> filteredSpecs = null;
        IList<int> excludeFilteredSpecs = null;

        #region B2B

        var b2BAccount = await GetErpAccountByCurrentCustomerAsync();

        if (b2BAccount != null && _b2BB2CFeaturesSettings.UsePrefilterFacet)
        {
            filteredSpecs = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, b2BAccount.PreFilterFacets?.Trim(), b2BAccount.Id);
            var specialIncludeSpecIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, "", b2BAccount.Id);

            if (specialIncludeSpecIds != null && specialIncludeSpecIds.Any())
            {
                foreach (var includeSpecId in specialIncludeSpecIds)
                {
                    if (!filteredSpecs.Contains(includeSpecId))
                        filteredSpecs.Add(includeSpecId);
                }
            }
            if (filteredSpecs == null)
            {
                return new List<Product>();
            }

            excludeFilteredSpecs = new List<int>();

            var specialExcludeSpecIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsForExcludeByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, "");

            if (specialExcludeSpecIds != null && specialExcludeSpecIds.Any())
            {
                foreach (var excludeSpecId in specialExcludeSpecIds)
                {
                    if (!excludeFilteredSpecs.Contains(excludeSpecId))
                        excludeFilteredSpecs.Add(excludeSpecId);
                }
            }
        }

        #endregion

        var query = from p in _productRepository.Table
                    orderby p.DisplayOrder, p.Id
                    where p.Published &&
                    !p.Deleted &&
                    p.ShowOnHomepage
                    select p;

        if (filteredSpecs != null && b2BAccount != null)
        {
            if (excludeFilteredSpecs != null && excludeFilteredSpecs.Any())
            {
                var excludedProductIds = await _erpSpecificationAttributeService.GetProductIdBySpecificationAttributeOptionNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, "", b2BAccount.Id);

                query = from p in query
                        join psa in _productSpecificationAttributeRepository.Table
                        on p.Id equals psa.ProductId
                        where filteredSpecs.Contains(psa.SpecificationAttributeOptionId) && !excludedProductIds.Contains(p.Id)
                        select p;
            }
            else
            {
                query = from p in query
                        join psa in _productSpecificationAttributeRepository.Table
                        on p.Id equals psa.ProductId
                        where filteredSpecs.Contains(psa.SpecificationAttributeOptionId)
                        select p;
            }
        }
        var products = query.Distinct().ToList();
        return products;
    }

    /// <summary>
    /// Gets product
    /// </summary>
    /// <param name="productId">Product identifier</param>
    /// <returns>Product</returns>
    public override async Task<Product> GetProductByIdAsync(int productId)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        _b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);
        if (productId == 0)
            return null;

        var key = _staticCacheManager.PrepareKeyForDefaultCache(B2BB2CFeaturesDefaults.ProductsByIdCacheKey, productId);
        var product = await _staticCacheManager.GetAsync(key, async () =>
        {
            return await _productRepository.GetByIdAsync(productId);
        });

        #region B2B

        var b2BAccount = await GetErpAccountByCurrentCustomerAsync();

        if (b2BAccount != null && _b2BB2CFeaturesSettings.UsePrefilterFacet)
        {
            var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id);
            var productSpecificationAttributeIds = productSpecificationAttributes.Select(x => x.SpecificationAttributeOptionId);

            var filteredSpecs = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, b2BAccount.PreFilterFacets?.Trim(), b2BAccount.Id);
            var specialIncludeSpecIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, "" /*b2BAccount.SpecialIncludes?.Trim()*/, b2BAccount.Id);
            var specialExcludeSpecIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsForExcludeByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, "" /*b2BAccount.SpecialExcludes?.Trim()*/);

            if (specialIncludeSpecIds != null && specialIncludeSpecIds.Any())
            {
                foreach (var includeSpecId in specialIncludeSpecIds)
                {
                    if (!filteredSpecs.Contains(includeSpecId))
                        filteredSpecs.Add(includeSpecId);
                }
            }

            if (specialIncludeSpecIds != null && specialIncludeSpecIds.Any())
            {
                foreach (var includeSpecId in specialIncludeSpecIds)
                {
                    if (!filteredSpecs.Contains(includeSpecId))
                        filteredSpecs.Add(includeSpecId);
                }
            }

            var commonSpecIds = filteredSpecs.Intersect(productSpecificationAttributeIds);

            if (!commonSpecIds.Any())
            {
                return null;
            }
            if (specialExcludeSpecIds != null && specialExcludeSpecIds.Any() && productSpecificationAttributeIds != null && productSpecificationAttributeIds.Any())
            {
                var commonSpecificationIds = specialExcludeSpecIds.Intersect(productSpecificationAttributeIds);
                if (commonSpecificationIds.Any())
                {
                    return null;
                }
            }

        }

        #endregion

        return product;
    }

    /// <summary>
    /// Get products by identifiers
    /// </summary>
    /// <param name="productIds">Product identifiers</param>
    /// <returns>Products</returns>
    public override async Task<IList<Product>> GetProductsByIdsAsync(int[] productIds)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        _b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);
        if (productIds == null || productIds.Length == 0)
            return new List<Product>();

        var query = from p in _productRepository.Table
                    where productIds.Contains(p.Id) && !p.Deleted
                    select p;

        IList<int> filteredSpecs = null;
        IList<int> excludeFilteredSpecs = null;

        #region B2B

        var b2BAccount = await GetErpAccountByCurrentCustomerAsync();

        if (b2BAccount != null && _b2BB2CFeaturesSettings.UsePrefilterFacet)
        {
            filteredSpecs = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, b2BAccount.PreFilterFacets?.Trim(), b2BAccount.Id);

            //ToDo : No specialInclude property found in entity
            var specialIncludeSpecIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, "", b2BAccount.Id);

            if (specialIncludeSpecIds != null && specialIncludeSpecIds.Any())
            {
                foreach (var includeSpecId in specialIncludeSpecIds)
                {
                    if (!filteredSpecs.Contains(includeSpecId))
                        filteredSpecs.Add(includeSpecId);
                }
            }

            if (filteredSpecs == null)
            {
                return new List<Product>();
            }

            excludeFilteredSpecs = new List<int>();

            var specialExcludeSpecIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsForExcludeByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, "" /*b2BAccount.SpecialExcludes?.Trim()*/);

            if (specialExcludeSpecIds != null && specialExcludeSpecIds.Any())
            {
                foreach (var excludeSpecId in specialExcludeSpecIds)
                {
                    if (!excludeFilteredSpecs.Contains(excludeSpecId))
                        excludeFilteredSpecs.Add(excludeSpecId);
                }
            }
        }

        if (filteredSpecs != null && b2BAccount != null)
        {
            if (excludeFilteredSpecs != null && excludeFilteredSpecs.Any())
            {
                var excludedProductIds = await _erpSpecificationAttributeService.GetProductIdBySpecificationAttributeOptionNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, "" /*b2BAccount.SpecialExcludes?.Trim()*/, b2BAccount.Id);

                query = from p in query
                        join psa in _productSpecificationAttributeRepository.Table
                        on p.Id equals psa.ProductId
                        where filteredSpecs.Contains(psa.SpecificationAttributeOptionId) && !excludedProductIds.Contains(p.Id)
                        select p;
            }
            else
            {
                query = from p in query
                        join psa in _productSpecificationAttributeRepository.Table
                        on p.Id equals psa.ProductId
                        where filteredSpecs.Contains(psa.SpecificationAttributeOptionId)
                        select p;
            }
        }

        #endregion

        var products = query.ToList();
        //sort by passed identifiers
        var sortedProducts = new List<Product>();
        foreach (var id in productIds)
        {
            var product = products.Find(x => x.Id == id);
            if (product != null)
                sortedProducts.Add(product);
        }

        return sortedProducts;
    }

    /// <summary>
    /// Get total quantity
    /// </summary>
    /// <param name="product">Product</param>
    /// <param name="useReservedQuantity">
    /// A value indicating whether we should consider "Reserved Quantity" property 
    /// when "multiple warehouses" are used
    /// </param>
    /// <param name="warehouseId">
    /// Warehouse identifier. Used to limit result to certain warehouse.
    /// Used only with "multiple warehouses" enabled.
    /// </param>
    /// <returns>Result</returns>
    /// 
    public override async Task<int> GetTotalStockQuantityAsync(Product product, bool useReservedQuantity = true, int warehouseId = 0)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        _b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);
        if (product == null)
            throw new ArgumentNullException(nameof(product));
        
        if (product.ManageInventoryMethod != ManageInventoryMethod.ManageStock)
        {
            var categoryIds = new List<int>();
            if (!string.IsNullOrWhiteSpace(_b2BB2CFeaturesSettings.SkipLiveStockCheckCategoryIds))
            {
                categoryIds = _b2BB2CFeaturesSettings.SkipLiveStockCheckCategoryIds.Split(',').Select(int.Parse).ToList();
            }
            var productCategories = await _categoryService.GetProductCategoriesByProductIdAsync(product.Id);
            foreach (var cat in productCategories)
            {
                if (categoryIds.Any(a => a == cat.CategoryId))
                    return 1000;
            }

            //We can calculate total stock quantity when 'Manage inventory' property is set to 'Track inventory'
            return 0;
        }

        if (!product.UseMultipleWarehouses)
            return product.StockQuantity;

        #region B2B

        var erpAccount = await GetErpAccountByCurrentCustomerAsync();

        if (erpAccount != null)
        {
            var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync((await _workContext.GetCurrentCustomerAsync()).Id);
            var productWarehouseInventory = new List<ProductWarehouseInventory>();

            if (erpNopUser != null)
            {
                var warehouseIds = (await _erpWarehouseSalesOrgMapService
                    .GetErpWarehouseSalesOrgMapsBySalesOrgIdAsync(erpAccount.ErpSalesOrgId))?
                    .Select(s => s.NopWarehouseId)?.ToList();

                productWarehouseInventory = (await GetAllProductWarehouseInventoryRecordsAsync(product.Id))?
                    .Where(w => warehouseIds.Contains(w.WarehouseId)).ToList();
            }

            var totalStock = (decimal)productWarehouseInventory.Sum(x => x.StockQuantity);
            if (useReservedQuantity)
            {
                totalStock = totalStock - productWarehouseInventory.Sum(x => x.ReservedQuantity);
            }

            if (_b2BB2CFeaturesSettings.UsePercentageOfAllocatedStock)
            {
                totalStock = totalStock * _b2BB2CFeaturesSettings.PercentageOfStockAllowed;
                totalStock = totalStock / 100;
                return (int)totalStock;
            }
            return (int)totalStock;
        }

        #endregion

        var pwi = _productWarehouseInventoryRepository.Table.Where(wi => wi.ProductId == product.Id);

        if (warehouseId > 0)
            pwi = pwi.Where(x => x.WarehouseId == warehouseId);

        var result = await pwi.SumAsync(x => x.StockQuantity);
        if (useReservedQuantity)
            result -= await pwi.SumAsync(x => x.ReservedQuantity);

        return result;
    }

    /// <summary>
    /// Gets featured products by a category identifier
    /// </summary>
    /// <param name="categoryId">Category identifier</param>
    /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of featured products
    /// </returns>
    public override async Task<IList<Product>> GetCategoryFeaturedProductsAsync(int categoryId, int storeId = 0)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        _b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);
        IList<Product> featuredProducts = new List<Product>();

        if (categoryId == 0)
            return featuredProducts;

        var customer = await _workContext.GetCurrentCustomerAsync();
        var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.CategoryFeaturedProductsIdsKey, categoryId, customerRoleIds, storeId);

        var featuredProductIds = await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from p in _productRepository.Table
                        join pc in _productCategoryRepository.Table on p.Id equals pc.ProductId
                        where p.Published && !p.Deleted && p.VisibleIndividually &&
                              (!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc.Value < DateTime.UtcNow) &&
                              (!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc.Value > DateTime.UtcNow) &&
                              pc.IsFeaturedProduct && categoryId == pc.CategoryId
                        select p;

            #region B2B

            var b2BAccount = await GetErpAccountByCurrentCustomerAsync();

            var filteredSpecOptions = new List<SpecificationAttributeOption>();

            if (b2BAccount != null && _b2BB2CFeaturesSettings.UsePrefilterFacet)
            {
                if (b2BAccount.PreFilterFacets?.Trim() == null)
                {
                    // if b2buser don't pre filter facets then return no products
                    return null;
                }

                IList<int> specIds = null;
                if (b2BAccount.PreFilterFacets?.Trim() != null)
                    specIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, b2BAccount.PreFilterFacets?.Trim(), b2BAccount.Id);

                if (!specIds.Any())
                {
                    // if b2buser filter facets are not exist as SpecificationAttributeOption then return no products
                    return null;
                }

                var preFilterFacetSpecIds = new List<int>();

                foreach (var specId in specIds)
                {
                    if (!preFilterFacetSpecIds.Contains(specId))
                        preFilterFacetSpecIds.Add(specId);
                }

                //pass pre filter facet identifiers
                if (preFilterFacetSpecIds != null && preFilterFacetSpecIds.Count > 0)
                {
                    preFilterFacetSpecIds.Sort();
                    //filterable options
                    var filterableOptions = await _specificationAttributeService
                        .GetSpecificationAttributeOptionsByIdsAsync(preFilterFacetSpecIds.ToArray());

                    foreach (var filterableOption in filterableOptions)
                    {
                        filteredSpecOptions.Add(filterableOption);
                    }
                }
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    query =
                        from p in query
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            #endregion

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            //apply ACL constraints
            query = await _aclService.ApplyAcl(query, customerRoleIds);

            featuredProducts = query.ToList();

            return featuredProducts.Select(p => p.Id).ToList();
        });

        if (!featuredProducts.Any() && (featuredProductIds != null && featuredProductIds.Any()))
            featuredProducts = await _productRepository.GetByIdsAsync(featuredProductIds, cache => default, false);

        return featuredProducts;
    }

    /// <summary>
    /// Gets featured products by manufacturer identifier
    /// </summary>
    /// <param name="manufacturerId">Manufacturer identifier</param>
    /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of featured products
    /// </returns>
    public override async Task<IList<Product>> GetManufacturerFeaturedProductsAsync(int manufacturerId, int storeId = 0)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        _b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);
        IList<Product> featuredProducts = new List<Product>();

        if (manufacturerId == 0)
            return featuredProducts;

        var customer = await _workContext.GetCurrentCustomerAsync();
        var customerRoleIds = await _customerService.GetCustomerRoleIdsAsync(customer);
        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(NopCatalogDefaults.ManufacturerFeaturedProductIdsKey, manufacturerId, customerRoleIds, storeId);

        var featuredProductIds = await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            var query = from p in _productRepository.Table
                        join pm in _productManufacturerRepository.Table on p.Id equals pm.ProductId
                        where p.Published && !p.Deleted && p.VisibleIndividually &&
                              (!p.AvailableStartDateTimeUtc.HasValue || p.AvailableStartDateTimeUtc.Value < DateTime.UtcNow) &&
                              (!p.AvailableEndDateTimeUtc.HasValue || p.AvailableEndDateTimeUtc.Value > DateTime.UtcNow) &&
                              pm.IsFeaturedProduct && manufacturerId == pm.ManufacturerId
                        select p;

            #region B2B

            var b2BAccount = await GetErpAccountByCurrentCustomerAsync();

            var filteredSpecOptions = new List<SpecificationAttributeOption>();

            if (b2BAccount != null && _b2BB2CFeaturesSettings.UsePrefilterFacet)
            {
                if (b2BAccount.PreFilterFacets?.Trim() == null)
                {
                    // if b2buser don't pre filter facets then return no products
                    return null;
                }

                IList<int> specIds = null;
                if (b2BAccount.PreFilterFacets?.Trim() != null)
                    specIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, b2BAccount.PreFilterFacets?.Trim(), b2BAccount.Id);

                if (!specIds.Any())
                {
                    // if b2buser filter facets are not exist as SpecificationAttributeOption then return no products
                    return null;
                }

                var preFilterFacetSpecIds = new List<int>();

                foreach (var specId in specIds)
                {
                    if (!preFilterFacetSpecIds.Contains(specId))
                        preFilterFacetSpecIds.Add(specId);
                }

                //pass pre filter facet identifiers
                if (preFilterFacetSpecIds != null && preFilterFacetSpecIds.Count > 0)
                {
                    preFilterFacetSpecIds.Sort();
                    //filterable options
                    var filterableOptions = await _specificationAttributeService
                        .GetSpecificationAttributeOptionsByIdsAsync(preFilterFacetSpecIds.ToArray());

                    foreach (var filterableOption in filterableOptions)
                    {
                        filteredSpecOptions.Add(filterableOption);
                    }
                }
            }

            if (filteredSpecOptions?.Count > 0)
            {
                var specificationAttributeIds = filteredSpecOptions
                    .Select(sao => sao.SpecificationAttributeId)
                    .Distinct();

                foreach (var specificationAttributeId in specificationAttributeIds)
                {
                    var optionIdsBySpecificationAttribute = filteredSpecOptions
                        .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                        .Select(o => o.Id);

                    var productSpecificationQuery =
                        from psa in _productSpecificationAttributeRepository.Table
                        where optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    query =
                        from p in query
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            #endregion

            //apply store mapping constraints
            query = await _storeMappingService.ApplyStoreMapping(query, storeId);

            //apply ACL constraints
            query = await _aclService.ApplyAcl(query, customerRoleIds);

            return query.Select(p => p.Id).ToList();
        });

        if (!featuredProducts.Any() && featuredProductIds.Any())
            featuredProducts = await _productRepository.GetByIdsAsync(featuredProductIds, cache => default, false);

        return featuredProducts;
    }

    /// <summary>
    /// Gets products which marked as new
    /// </summary>
    /// <param name="storeId">Store identifier; 0 if you want to get all records</param>
    /// <param name="pageIndex">Page index</param>
    /// <param name="pageSize">Page size</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the list of new products
    /// </returns>
    public override async Task<IPagedList<Product>> GetProductsMarkedAsNewAsync(int storeId = 0, int pageIndex = 0, int pageSize = int.MaxValue)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        _b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);
        var query = from p in _productRepository.Table
                    where p.Published && p.VisibleIndividually && p.MarkAsNew && !p.Deleted &&
                          DateTime.UtcNow >= (p.MarkAsNewStartDateTimeUtc ?? SqlDateTime.MinValue.Value) &&
                          DateTime.UtcNow <= (p.MarkAsNewEndDateTimeUtc ?? SqlDateTime.MaxValue.Value)
                    select p;

        //apply store mapping constraints
        query = await _storeMappingService.ApplyStoreMapping(query, storeId);

        //apply ACL constraints
        var customer = await _workContext.GetCurrentCustomerAsync();

        #region B2B

        var b2BAccount = await GetErpAccountByCurrentCustomerAsync();

        var filteredSpecOptions = new List<SpecificationAttributeOption>();

        if (b2BAccount != null && _b2BB2CFeaturesSettings.UsePrefilterFacet)
        {
            if (b2BAccount.PreFilterFacets?.Trim() == null)
            {
                // if b2buser don't pre filter facets then return no products
                return null;
            }

            IList<int> specIds = null;
            if (b2BAccount.PreFilterFacets?.Trim() != null)
                specIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, b2BAccount.PreFilterFacets?.Trim(), b2BAccount.Id);

            if (!specIds.Any())
            {
                // if b2buser filter facets are not exist as SpecificationAttributeOption then return no products
                return null;
            }

            var preFilterFacetSpecIds = new List<int>();

            foreach (var specId in specIds)
            {
                if (!preFilterFacetSpecIds.Contains(specId))
                    preFilterFacetSpecIds.Add(specId);
            }

            //pass pre filter facet identifiers
            if (preFilterFacetSpecIds != null && preFilterFacetSpecIds.Count > 0)
            {
                preFilterFacetSpecIds.Sort();
                //filterable options
                var filterableOptions = await _specificationAttributeService
                    .GetSpecificationAttributeOptionsByIdsAsync(preFilterFacetSpecIds.ToArray());

                foreach (var filterableOption in filterableOptions)
                {
                    filteredSpecOptions.Add(filterableOption);
                }
            }
        }

        if (filteredSpecOptions?.Count > 0)
        {
            var specificationAttributeIds = filteredSpecOptions
                .Select(sao => sao.SpecificationAttributeId)
                .Distinct();

            foreach (var specificationAttributeId in specificationAttributeIds)
            {
                var optionIdsBySpecificationAttribute = filteredSpecOptions
                    .Where(o => o.SpecificationAttributeId == specificationAttributeId)
                    .Select(o => o.Id);

                var productSpecificationQuery =
                    from psa in _productSpecificationAttributeRepository.Table
                    where optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                    select psa;

                query =
                    from p in query
                    where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                    select p;
            }
        }

        #endregion

        query = await _aclService.ApplyAcl(query, customer);

        query = query.OrderByDescending(p => p.CreatedOnUtc);

        return await query.ToPagedListAsync(pageIndex, pageSize);
    }

    /// <summary>
    /// Gets a product by SKU
    /// </summary>
    /// <param name="sku">SKU</param>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the product
    /// </returns>
    public override async Task<Product> GetProductBySkuAsync(string sku)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        _b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(store.Id);
        if (string.IsNullOrEmpty(sku))
            return null;

        sku = sku.Trim();

        var query = from p in _productRepository.Table
                    orderby p.Id
                    where !p.Deleted &&
                          p.Sku == sku
                    select p;
        var product = await query.FirstOrDefaultAsync();

        #region B2B

        var b2BAccount = await GetErpAccountByCurrentCustomerAsync();

        if (b2BAccount != null && _b2BB2CFeaturesSettings.UsePrefilterFacet)
        {
            var productSpecificationAttributes = await _specificationAttributeService.GetProductSpecificationAttributesAsync(product.Id);
            var productSpecificationAttributeIds = productSpecificationAttributes.Select(x => x.SpecificationAttributeOptionId);

            var filteredSpecs = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, b2BAccount.PreFilterFacets?.Trim(), b2BAccount.Id);
            var specialIncludeSpecIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, "" /*b2BAccount.SpecialIncludes?.Trim()*/, b2BAccount.Id);
            var specialExcludeSpecIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsForExcludeByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, "" /*b2BAccount.SpecialExcludes?.Trim()*/);

            if (specialIncludeSpecIds != null && specialIncludeSpecIds.Any())
            {
                foreach (var includeSpecId in specialIncludeSpecIds)
                {
                    if (!filteredSpecs.Contains(includeSpecId))
                        filteredSpecs.Add(includeSpecId);
                }
            }

            if (specialIncludeSpecIds != null && specialIncludeSpecIds.Any())
            {
                foreach (var includeSpecId in specialIncludeSpecIds)
                {
                    if (!filteredSpecs.Contains(includeSpecId))
                        filteredSpecs.Add(includeSpecId);
                }
            }

            var commonSpecIds = filteredSpecs.Intersect(productSpecificationAttributeIds);

            if (!commonSpecIds.Any())
            {
                return null;
            }
            if (specialExcludeSpecIds != null && specialExcludeSpecIds.Any() && productSpecificationAttributeIds != null && productSpecificationAttributeIds.Any())
            {
                var commonSpecificationIds = specialExcludeSpecIds.Intersect(productSpecificationAttributeIds);
                if (commonSpecificationIds.Any())
                {
                    return null;
                }
            }

        }

        #endregion

        return product;
    }

    #endregion

    #endregion
}