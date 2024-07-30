using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Discounts;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Seo;
using Nop.Core.Domain.Shipping;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.ExportImport.Help;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping.Date;
using Nop.Services.Stores;
using Nop.Services.Tax;
using Nop.Web.Framework;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Services.ExportManager
{
    public class CategoryProductsExportManager : ICategoryProductsExportManager
    {
        #region Fields

        private readonly ICategoryService _categoryService;
        private readonly IProductService _productService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILanguageService _languageService;
        private readonly IWorkContext _workContext;
        private readonly IDiscountService _discountService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly ProductEditorSettings _productEditorSettings;
        private readonly ILocalizedEntityService _localizedEntityService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IProductTagService _productTagService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IManufacturerService _manufacturerService;
        private readonly CatalogSettings _catalogSettings;
        private readonly IDateRangeService _dateRangeService;
        private readonly ITaxCategoryService _taxCategoryService;
        private readonly IMeasureService _measureService;
        private readonly IStoreMappingService _storeMappingService;
        private readonly IStoreService _storeService;

        protected readonly IAclService _aclService;
        protected readonly ICustomerService _customerService;
        protected readonly IProductAttributeParser _productAttributeParser;
        protected readonly IRepository<Category> _categoryRepository;
        protected readonly IRepository<CrossSellProduct> _crossSellProductRepository;
        protected readonly IRepository<DiscountProductMapping> _discountProductMappingRepository;
        protected readonly IRepository<LocalizedProperty> _localizedPropertyRepository;
        protected readonly IRepository<Manufacturer> _manufacturerRepository;
        protected readonly IRepository<Product> _productRepository;
        protected readonly IRepository<ProductAttributeCombination> _productAttributeCombinationRepository;
        protected readonly IRepository<ProductAttributeMapping> _productAttributeMappingRepository;
        protected readonly IRepository<ProductCategory> _productCategoryRepository;
        protected readonly IRepository<ProductManufacturer> _productManufacturerRepository;
        protected readonly IRepository<ProductPicture> _productPictureRepository;
        protected readonly IRepository<ProductProductTagMapping> _productTagMappingRepository;
        protected readonly IRepository<ProductReview> _productReviewRepository;
        protected readonly IRepository<ProductReviewHelpfulness> _productReviewHelpfulnessRepository;
        protected readonly IRepository<ProductSpecificationAttribute> _productSpecificationAttributeRepository;
        protected readonly IRepository<ProductTag> _productTagRepository;
        protected readonly IRepository<ProductVideo> _productVideoRepository;
        protected readonly IRepository<ProductWarehouseInventory> _productWarehouseInventoryRepository;
        protected readonly IRepository<RelatedProduct> _relatedProductRepository;
        protected readonly IRepository<Shipment> _shipmentRepository;
        protected readonly IRepository<StockQuantityHistory> _stockQuantityHistoryRepository;
        protected readonly IRepository<TierPrice> _tierPriceRepository;
        protected readonly ISearchPluginManager _searchPluginManager;
        protected readonly IStaticCacheManager _staticCacheManager;
        protected readonly LocalizationSettings _localizationSettings;

        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IErpAccountService _erpAccountService;
        private readonly IPermissionService _permissionService;
        private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
        private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;
        private readonly IStoreContext _storeContext;
        private readonly ISettingService _settingService;

        private readonly IErpSpecialPriceService _erpSpecialPriceService;
        private readonly IErpNopUserService _erpNopUserService;
        private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;

        #endregion

        #region Ctor

        public CategoryProductsExportManager(ICategoryService categoryService,
            IProductService productService,
            ICustomerActivityService customerActivityService,
            ILocalizationService localizationService,
            ILanguageService languageService,
            IWorkContext workContext,
            IDiscountService discountService,
            IGenericAttributeService genericAttributeService,
            ProductEditorSettings productEditorSettings,
            ILocalizedEntityService localizedEntityService,
            IUrlRecordService urlRecordService,
            IProductTagService productTagService,
            ISpecificationAttributeService specificationAttributeService,
            IProductAttributeService productAttributeService,
            IManufacturerService manufacturerService,
            CatalogSettings catalogSettings,
            IDateRangeService dateRangeService,
            ITaxCategoryService taxCategoryService,
            IMeasureService measureService,
            IStoreService storeService,
            IAclService aclService,
            ICustomerService customerService,
            IProductAttributeParser productAttributeParser,
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
            LocalizationSettings localizationSettings,
            IActionContextAccessor actionContextAccessor,
            IErpAccountService erpAccountService,
            IPermissionService permissionService,
            B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
            IErpSpecificationAttributeService erpSpecificationAttributeService,
            IStoreContext storeContext,
            ISettingService settingService,
            IErpSpecialPriceService erpSpecialPriceService,
            IErpNopUserService erpNopUserService,
            IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService)
        {
            _categoryService = categoryService;
            _productService = productService;
            _customerActivityService = customerActivityService;
            _localizationService = localizationService;
            _languageService = languageService;
            _workContext = workContext;
            _discountService = discountService;
            _genericAttributeService = genericAttributeService;
            _productEditorSettings = productEditorSettings;
            _localizedEntityService = localizedEntityService;
            _urlRecordService = urlRecordService;
            _productTagService = productTagService;
            _specificationAttributeService = specificationAttributeService;
            _productAttributeService = productAttributeService;
            _manufacturerService = manufacturerService;
            _catalogSettings = catalogSettings;
            _dateRangeService = dateRangeService;
            _taxCategoryService = taxCategoryService;
            _measureService = measureService;
            _storeService = storeService;
            _aclService = aclService;
            _customerService = customerService;
            _productAttributeParser = productAttributeParser;
            _categoryRepository = categoryRepository;
            _crossSellProductRepository = crossSellProductRepository;
            _discountProductMappingRepository = discountProductMappingRepository;
            _localizedPropertyRepository = localizedPropertyRepository;
            _manufacturerRepository = manufacturerRepository;
            _productRepository = productRepository;
            _productAttributeCombinationRepository = productAttributeCombinationRepository;
            _productAttributeMappingRepository = productAttributeMappingRepository;
            _productCategoryRepository = productCategoryRepository;
            _productManufacturerRepository = productManufacturerRepository;
            _productPictureRepository = productPictureRepository;
            _productTagMappingRepository = productTagMappingRepository;
            _productReviewRepository = productReviewRepository;
            _productReviewHelpfulnessRepository = productReviewHelpfulnessRepository;
            _productSpecificationAttributeRepository = productSpecificationAttributeRepository;
            _productTagRepository = productTagRepository;
            _productVideoRepository = productVideoRepository;
            _productWarehouseInventoryRepository = productWarehouseInventoryRepository;
            _relatedProductRepository = relatedProductRepository;
            _shipmentRepository = shipmentRepository;
            _stockQuantityHistoryRepository = stockQuantityHistoryRepository;
            _tierPriceRepository = tierPriceRepository;
            _searchPluginManager = searchPluginManager;
            _staticCacheManager = staticCacheManager;
            _localizationSettings = localizationSettings;
            _actionContextAccessor = actionContextAccessor;
            _erpAccountService = erpAccountService;
            _permissionService = permissionService;
            _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
            _erpSpecificationAttributeService = erpSpecificationAttributeService;
            _storeContext = storeContext;
            _settingService = settingService;
            _erpSpecialPriceService = erpSpecialPriceService;
            _erpNopUserService = erpNopUserService;
            _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
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

        protected virtual async Task<int> WriteCategoriesAsync(XmlWriter xmlWriter, int parentCategoryId, int totalCategories)
        {
            var categories = await _categoryService.GetAllCategoriesByParentCategoryIdAsync(parentCategoryId, true);
            if (categories == null || !categories.Any())
                return totalCategories;

            totalCategories += categories.Count;

            var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

            foreach (var category in categories)
            {
                await xmlWriter.WriteStartElementAsync("Category");

                await xmlWriter.WriteStringAsync("Id", category.Id);

                await WriteLocalizedPropertyXmlAsync(category, c => c.Name, xmlWriter, languages);
                await WriteLocalizedPropertyXmlAsync(category, c => c.Description, xmlWriter, languages);
                await xmlWriter.WriteStringAsync("CategoryTemplateId", category.CategoryTemplateId);
                await WriteLocalizedPropertyXmlAsync(category, c => c.MetaKeywords, xmlWriter, languages, await IgnoreExportCategoryPropertyAsync());
                await WriteLocalizedPropertyXmlAsync(category, c => c.MetaDescription, xmlWriter, languages, await IgnoreExportCategoryPropertyAsync());
                await WriteLocalizedPropertyXmlAsync(category, c => c.MetaTitle, xmlWriter, languages, await IgnoreExportCategoryPropertyAsync());
                await WriteLocalizedSeNameXmlAsync(category, xmlWriter, languages, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("ParentCategoryId", category.ParentCategoryId);
                await xmlWriter.WriteStringAsync("PictureId", category.PictureId);
                await xmlWriter.WriteStringAsync("PageSize", category.PageSize, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("AllowCustomersToSelectPageSize", category.AllowCustomersToSelectPageSize, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("PageSizeOptions", category.PageSizeOptions, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("PriceRangeFiltering", category.PriceRangeFiltering, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("PriceFrom", category.PriceFrom, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("PriceTo", category.PriceTo, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("ManuallyPriceRange", category.ManuallyPriceRange, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("ShowOnHomepage", category.ShowOnHomepage, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("IncludeInTopMenu", category.IncludeInTopMenu, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("Published", category.Published, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("Deleted", category.Deleted, true);
                await xmlWriter.WriteStringAsync("DisplayOrder", category.DisplayOrder);
                await xmlWriter.WriteStringAsync("CreatedOnUtc", category.CreatedOnUtc, await IgnoreExportCategoryPropertyAsync());
                await xmlWriter.WriteStringAsync("UpdatedOnUtc", category.UpdatedOnUtc, await IgnoreExportCategoryPropertyAsync());

                await xmlWriter.WriteStartElementAsync("Products");
                var productCategories = await _categoryService.GetProductCategoriesByCategoryIdAsync(category.Id, showHidden: true);
                foreach (var productCategory in productCategories)
                {
                    var product = await _productService.GetProductByIdAsync(productCategory.ProductId);
                    if (product == null || product.Deleted)
                        continue;

                    await xmlWriter.WriteStartElementAsync("ProductCategory");
                    await xmlWriter.WriteStringAsync("ProductCategoryId", productCategory.Id);
                    await xmlWriter.WriteStringAsync("ProductId", productCategory.ProductId);
                    await WriteLocalizedPropertyXmlAsync(product, p => p.Name, xmlWriter, languages, overriddenNodeName: "ProductName");
                    await xmlWriter.WriteStringAsync("IsFeaturedProduct", productCategory.IsFeaturedProduct);
                    await xmlWriter.WriteStringAsync("DisplayOrder", productCategory.DisplayOrder);
                    await xmlWriter.WriteEndElementAsync();
                }

                await xmlWriter.WriteEndElementAsync();

                await xmlWriter.WriteStartElementAsync("SubCategories");
                totalCategories = await WriteCategoriesAsync(xmlWriter, category.Id, totalCategories);
                await xmlWriter.WriteEndElementAsync();
                await xmlWriter.WriteEndElementAsync();
            }

            return totalCategories;
        }
        protected virtual async Task<object> GetCategoriesAsync(Product product)
        {
            string categoryNames = null;
            foreach (var pc in await _categoryService.GetProductCategoriesByProductIdAsync(product.Id, true))
            {
                if (_catalogSettings.ExportImportRelatedEntitiesByName)
                {
                    var category = await _categoryService.GetCategoryByIdAsync(pc.CategoryId);
                    categoryNames += _catalogSettings.ExportImportProductCategoryBreadcrumb
                        ? await _categoryService.GetFormattedBreadCrumbAsync(category)
                        : category.Name;
                }
                else
                {
                    categoryNames += pc.CategoryId.ToString();
                }

                categoryNames += ";";
            }

            return categoryNames;
        }

        protected virtual async Task<object> GetManufacturersAsync(Product product)
        {
            string manufacturerNames = null;
            foreach (var pm in await _manufacturerService.GetProductManufacturersByProductIdAsync(product.Id, true))
            {
                if (_catalogSettings.ExportImportRelatedEntitiesByName)
                {
                    var manufacturer = await _manufacturerService.GetManufacturerByIdAsync(pm.ManufacturerId);
                    manufacturerNames += manufacturer.Name;
                }
                else
                {
                    manufacturerNames += pm.ManufacturerId.ToString();
                }

                manufacturerNames += ";";
            }

            return manufacturerNames;
        }

        protected virtual async Task<object> GetLimitedToStoresAsync(Product product)
        {
            string limitedToStores = null;
            foreach (var storeMapping in await _storeMappingService.GetStoreMappingsAsync(product))
            {
                var store = await _storeService.GetStoreByIdAsync(storeMapping.StoreId);

                limitedToStores += _catalogSettings.ExportImportRelatedEntitiesByName ? store.Name : store.Id.ToString();

                limitedToStores += ";";
            }

            return limitedToStores;
        }

        protected virtual async Task<bool> IgnoreExportProductPropertyAsync(Func<ProductEditorSettings, bool> func)
        {
            var productAdvancedMode = true;
            try
            {
                productAdvancedMode = await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), "product-advanced-mode");
            }
            catch (ArgumentNullException)
            {

            }

            return !productAdvancedMode && !func(_productEditorSettings);
        }

        protected virtual async Task<bool> IgnoreExportCategoryPropertyAsync()
        {
            try
            {
                return !await _genericAttributeService.GetAttributeAsync<bool>(await _workContext.GetCurrentCustomerAsync(), "category-advanced-mode");
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        protected virtual async Task<bool> IgnoreExportLimitedToStoreAsync()
        {
            return _catalogSettings.IgnoreStoreLimitations ||
                   !_catalogSettings.ExportImportProductUseLimitedToStores ||
                   (await _storeService.GetAllStoresAsync()).Count == 1;
        }

        private async Task<TProperty> GetLocalizedAsync<TEntity, TProperty>(TEntity entity, Expression<Func<TEntity, TProperty>> keySelector,
            Language language) where TEntity : BaseEntity, ILocalizedEntity
        {
            if (entity == null)
                return default(TProperty);

            return await _localizationService.GetLocalizedAsync(entity, keySelector, language.Id, false);
        }

        private async Task WriteLocalizedSeNameXmlAsync<TEntity>(TEntity entity, XmlWriter xmlWriter, IList<Language> languages,
            bool ignore = false, string overriddenNodeName = null)
            where TEntity : BaseEntity, ISlugSupported
        {
            if (ignore)
                return;

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            var nodeName = "SEName";
            if (!string.IsNullOrWhiteSpace(overriddenNodeName))
                nodeName = overriddenNodeName;

            await xmlWriter.WriteStartElementAsync(nodeName);
            await xmlWriter.WriteStringAsync("Standard", await _urlRecordService.GetSeNameAsync(entity, 0));

            if (languages.Count >= 2)
            {
                await xmlWriter.WriteStartElementAsync("Locales");

                foreach (var language in languages)
                    if (await _urlRecordService.GetSeNameAsync(entity, language.Id, returnDefaultValue: false) is string seName && !string.IsNullOrWhiteSpace(seName))
                        await xmlWriter.WriteStringAsync(language.UniqueSeoCode, seName);

                await xmlWriter.WriteEndElementAsync();
            }

            await xmlWriter.WriteEndElementAsync();
        }

        private async Task WriteLocalizedPropertyXmlAsync<TEntity, TPropType>(TEntity entity, Expression<Func<TEntity, TPropType>> keySelector,
            XmlWriter xmlWriter, IList<Language> languages, bool ignore = false, string overriddenNodeName = null)
            where TEntity : BaseEntity, ILocalizedEntity
        {
            if (ignore)
                return;

            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            if (keySelector.Body is not MemberExpression member)
                throw new ArgumentException($"Expression '{keySelector}' refers to a method, not a property.");

            if (member.Member is not PropertyInfo propInfo)
                throw new ArgumentException($"Expression '{keySelector}' refers to a field, not a property.");

            var localeKeyGroup = entity.GetType().Name;
            var localeKey = propInfo.Name;

            var nodeName = localeKey;
            if (!string.IsNullOrWhiteSpace(overriddenNodeName))
                nodeName = overriddenNodeName;

            await xmlWriter.WriteStartElementAsync(nodeName);
            await xmlWriter.WriteStringAsync("Standard", propInfo.GetValue(entity));

            if (languages.Count >= 2)
            {
                await xmlWriter.WriteStartElementAsync("Locales");

                var properties = await _localizedEntityService.GetEntityLocalizedPropertiesAsync(entity.Id, localeKeyGroup, localeKey);
                foreach (var language in languages)
                    if (properties.FirstOrDefault(lp => lp.LanguageId == language.Id) is LocalizedProperty localizedProperty)
                        await xmlWriter.WriteStringAsync(language.UniqueSeoCode, localizedProperty.LocaleValue);

                await xmlWriter.WriteEndElementAsync();
            }

            await xmlWriter.WriteEndElementAsync();
        }

        #endregion

        #region Methods

        public virtual async Task<byte[]> ExportProductsToXlsxAsync(IEnumerable<Product> products)
        {
            var currentVendor = await _workContext.GetCurrentVendorAsync();
            var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);

            var localizedProperties = new[]
            {
                new PropertyByName<Product, Language>("Name", async (p, l) => await _localizationService.GetLocalizedAsync(p, x => x.Name, l.Id, false))
            };

            var properties = new[]
            {
                new PropertyByName<Product, Language>("Name", (p, l) => p.Name),
                new PropertyByName<Product, Language>("Price", (p, l) => p.Price),
                new PropertyByName<Product, Language>("SKU", (p, l) => p.Sku),
                new PropertyByName<Product, Language>("Quantity", (p, l) => p.OrderMinimumQuantity)
            };

            var productList = products.ToList();

            if (!_catalogSettings.ExportImportProductAttributes && !_catalogSettings.ExportImportProductSpecificationAttributes)
                return await new PropertyManager<Product, Language>(properties, _catalogSettings).ExportToXlsxAsync(productList);

            //activity log
            await _customerActivityService.InsertActivityAsync("ExportCategoryProducts",
                string.Format(await _localizationService.GetResourceAsync("ActivityLog.ExportCategoryProducts"), productList.Count));


            return await new PropertyManager<Product, Language>(properties, _catalogSettings, localizedProperties, languages).ExportToXlsxAsync(productList);
        }

        public virtual async Task<IPagedList<Product>> SearchProductsAsync(
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
                bool? overridePublished = null, bool showProductWithoutAttributes = false)
        {
            var currCustomer = await _workContext.GetCurrentCustomerAsync();
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

            #region B2B

            var b2BAccount = await _erpAccountService.GetActiveErpAccountByCustomerIdAsync(currCustomer.Id);

            if (!IsAdminRoute() && b2BAccount != null)
            {
                if (b2BAccount.PreFilterFacets?.Trim() == null)
                {
                    // if b2buser don't pre filter facets then return no products
                    var noProducts = new List<Product>();
                    return new PagedList<Product>(noProducts, pageIndex, pageSize, 0);
                }
                IList<int> specIds = null;
                if (b2BAccount.PreFilterFacets?.Trim() != null)
                    specIds = await _erpSpecificationAttributeService.GetSpecificationAttributeOptionIdsByNames(_b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId, b2BAccount.PreFilterFacets?.Trim(), b2BAccount.Id);

                if (!specIds.Any())
                {
                    // if b2buser filter facets are not exist as SpecificationAttributeOption then return no products
                    var noProducts = new List<Product>();
                    return new PagedList<Product>(noProducts, pageIndex, pageSize, 0);
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
                        where psa.AllowFiltering && optionIdsBySpecificationAttribute.Contains(psa.SpecificationAttributeOptionId)
                        select psa;

                    productsQuery =
                        from p in productsQuery
                        where productSpecificationQuery.Any(pc => pc.ProductId == p.Id)
                        select p;
                }
            }

            return await productsQuery.OrderBy(_localizedPropertyRepository, await _workContext.GetWorkingLanguageAsync(), orderBy).ToPagedListAsync(pageIndex, pageSize);
        }

        #endregion
    }
}