using System.Reflection;
using System.Text.RegularExpressions;
using FluentValidation;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Catalog;
using Nop.Services.Catalog;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Tax;
using Nop.Services.Vendors;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncWorkflowMessage;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Validators.Helpers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices;

public class ErpProductSyncService : IErpProductSyncService
{
    #region Fields

    private readonly IVendorService _vendorService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IShippingService _shippingService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ITaxCategoryService _taxCategoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IProductTemplateService _productTemplateService;
    private readonly ICategoryTemplateService _categoryTemplateService;
    private readonly IManufacturerTemplateService _manufacturerTemplateService;
    private readonly ISpecificationAttributeService _specificationAttributeService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly IErpProductService _erpProductService;
    private readonly IErpIntegrationPluginManager _erpIntegrationPluginManager;
    private readonly IErpSalesOrgService _erpSalesOrgService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IStaticCacheManager _staticCacheManager;
    private readonly ISyncWorkflowMessageService _syncWorkflowMessageService;
    private const int CATEGORY_PAGE_SIZE = 5;
    private const string MANUFACTURER_TEMPLATE_VIEWPATH = "ManufacturerTemplate.ProductsInGridOrLines";
    private const string PRODUCT_TEMPLATE_VIEWPATH = "ProductTemplate.Simple";
    private const string CATEGORY_TEMPLATE_VIEWPATH = "CategoryTemplate.ProductsInGridOrLines";
    private const int MANUFACTURER_PAGE_SIZE = 6;
    private const string MANUFACTURER_PAGE_SIZE_OPTIONS = "6, 3, 9";
    private readonly IValidator<Product> _productValidator;
    private readonly IValidator<Manufacturer> _manufacturerValidator;
    private readonly IValidator<Category> _categoryValidator;

    #endregion

    #region Ctor

    public ErpProductSyncService(IVendorService vendorService,
        IProductService productService,
        ICategoryService categoryService,
        IShippingService shippingService,
        IUrlRecordService urlRecordService,
        ITaxCategoryService taxCategoryService,
        IManufacturerService manufacturerService,
        IProductTemplateService productTemplateService,
        ICategoryTemplateService categoryTemplateService,
        IManufacturerTemplateService manufacturerTemplateService,
        ISpecificationAttributeService specificationAttributeService,
        ISyncLogService erpSyncLogService,
        IErpProductService erpProductService,
        IErpIntegrationPluginManager erpIntegrationPluginManager,
        IErpSalesOrgService erpSalesOrgService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IValidator<Product> productValidator,
        IValidator<Manufacturer> manufacturerValidator,
        IValidator<Category> categoryValidatory,
        IStaticCacheManager staticCacheManager,
        ISyncWorkflowMessageService syncWorkflowMessageService)
    {
        _vendorService = vendorService;
        _productService = productService;
        _categoryService = categoryService;
        _shippingService = shippingService;
        _urlRecordService = urlRecordService;
        _taxCategoryService = taxCategoryService;
        _manufacturerService = manufacturerService;
        _productTemplateService = productTemplateService;
        _categoryTemplateService = categoryTemplateService;
        _manufacturerTemplateService = manufacturerTemplateService;
        _specificationAttributeService = specificationAttributeService;
        _erpSyncLogService = erpSyncLogService;
        _erpProductService = erpProductService;
        _erpSalesOrgService = erpSalesOrgService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _productValidator = productValidator;
        _manufacturerValidator = manufacturerValidator;
        _categoryValidator = categoryValidatory;
        _staticCacheManager = staticCacheManager;
        _syncWorkflowMessageService = syncWorkflowMessageService;
        _erpIntegrationPluginManager = erpIntegrationPluginManager;
    }

    #endregion

    #region Utilities

    private async Task SaveOrUpdateEntitySeNameAsync<T>(T entity) where T : BaseEntity
    {
        if (entity is Category category)
        {
            var seName = await _urlRecordService.GetSeNameAsync(category);

            if (string.IsNullOrWhiteSpace(seName))
            {
                seName = await _urlRecordService.ValidateSeNameAsync(category, string.Empty, category.Name, true);
                await _urlRecordService.SaveSlugAsync(category, seName, 0);
            }
        }
        else if (entity is Product product)
        {
            var seName = await _urlRecordService.GetSeNameAsync(product);

            if (string.IsNullOrWhiteSpace(seName))
            {
                seName = await _urlRecordService.ValidateSeNameAsync(product, string.Empty, product.Name, true);
                await _urlRecordService.SaveSlugAsync(product, seName, 0);
            }
        }
        else if (entity is Manufacturer manufacturer)
        {
            var seName = await _urlRecordService.GetSeNameAsync(manufacturer);

            if (string.IsNullOrWhiteSpace(seName))
            {
                seName = await _urlRecordService.ValidateSeNameAsync(manufacturer, string.Empty, manufacturer.Name, true);
                await _urlRecordService.SaveSlugAsync(manufacturer, seName, 0);
            }
        }
    }

    private async Task<bool> IsthisProductIsValidAsync(Product product)
    {
        if (product is null)
            return false;

        var validationResult = await _productValidator.ValidateAsync(product);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                ErpSyncLevel.Product,
                $"Data mapping skipped for {nameof(Product)}, {nameof(Product.Sku)}: {product.Sku}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    private async Task<bool> IsValidCategoryAsync(Category category)
    {
        if (category is null)
            return false;

        var validationResult = await _categoryValidator.ValidateAsync(category);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                ErpSyncLevel.Product,
                $"Data mapping skipped for {nameof(Category)}, {nameof(Category.Name)}: {category.Name}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    private async Task<bool> IsValidManufacturerAsync(Manufacturer manufacturer)
    {
        if (manufacturer is null)
            return false;

        var validationResult = await _manufacturerValidator.ValidateAsync(manufacturer);

        if (!validationResult.IsValid)
        {
            var errorMessages = ErpDataValidationHelper.PrepareValidationLog(validationResult);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                ErpSyncLevel.Product,
                $"Data mapping skipped for {nameof(Manufacturer)}, {nameof(Manufacturer.Name)}: {manufacturer.Name}. \r\n {errorMessages}");
        }

        return validationResult.IsValid;
    }

    #endregion

    #region Method

    public virtual async Task<bool> IsErpProductSyncSuccessfulAsync(string? stockCode, bool isManualTrigger = false, bool isIncrementalSync = true, CancellationToken cancellationToken = default)
    {
        var erpIntegrationPlugin = await _erpIntegrationPluginManager.LoadActiveERPIntegrationPlugin();

        if (erpIntegrationPlugin is null)
        {
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                ErpSyncLevel.Product,
                $"No integration method found. Unable to run {ErpDataSchedulerDefaults.ErpProductSyncTaskName}.");

            return false;
        }

        var previousStart = "0";
        var lastSyncedErpProduct = string.Empty;
        //var syncStartTime = DateTime.UtcNow.AddMinutes(-10);

        try
        {
            #region Data Collections

            var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();
            if (!salesOrgs.Any())
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                    ErpSyncLevel.Product,
                    $"No Sales org found. Unable to run {ErpDataSchedulerDefaults.ErpProductSyncTaskName}.");

                return false;
            }

            var lineBreakReplacer = new Regex(@"\r?\n");
            var programName = Assembly.GetExecutingAssembly().GetName().Name;

            var manufacturerTemplate = (await _manufacturerTemplateService.GetAllManufacturerTemplatesAsync())
                .FirstOrDefault(mftemp => mftemp.ViewPath.Equals(MANUFACTURER_TEMPLATE_VIEWPATH));
            var productTemplate = (await _productTemplateService.GetAllProductTemplatesAsync())
                .FirstOrDefault(x => x.ViewPath.Equals(PRODUCT_TEMPLATE_VIEWPATH));
            var categoryTemplate = (await _categoryTemplateService.GetAllCategoryTemplatesAsync())
                .FirstOrDefault(x => x.ViewPath.Equals(CATEGORY_TEMPLATE_VIEWPATH));

            var specificationAttributes = await _specificationAttributeService.GetSpecificationAttributesAsync();
            var preFilterSpecificAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(
                _b2BB2CFeaturesSettings.PreFilterFacetSpecificationAttributeId);
            var uomSpecificAttribute = await _specificationAttributeService.GetSpecificationAttributeByIdAsync(
                _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId);

            var allVendors = (await _vendorService.GetAllVendorsAsync(showHidden: true)).ToList();

            var allWarehouses = await _shippingService.GetAllWarehousesAsync();
            var allTaxCategories = await _taxCategoryService.GetAllTaxCategoriesAsync();
            var allManufacturers = (await _manufacturerService.GetAllManufacturersAsync(showHidden: true)).ToList();

            var allCategories = await _categoryService.GetAllCategoriesAsync(showHidden: true);
            if (allCategories is null)
                allCategories = new List<Category>();
            var allProductCategories = new List<ProductCategory>();
            foreach (var category in allCategories)
            {
                allProductCategories.AddRange((await _categoryService.GetProductCategoriesByCategoryIdAsync(category.Id, showHidden: true)).ToList());
            }

            #endregion

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                    ErpSyncLevel.Product,
                    "Erp Product Sync started.");

            foreach (var salesOrg in salesOrgs)
            {
                var start = "0";
                previousStart = "0";
                var isError = false;
                var totalSyncedSoFar = 0;
                var totalNotSyncedSoFar = 0;
                List<Product> products;

                while (true)
                {
                    var erpGetRequestModel = new ErpGetRequestModel
                    {
                        Start = start,
                        Location = salesOrg.Code,
                        ProductSku = stockCode,
                        DateFrom = isIncrementalSync ? salesOrg.LastErpProductSyncTimeOnUtc : null                        
                    };

                    var response = await erpIntegrationPlugin.GetProductsFromErpAsync(erpGetRequestModel);

                    if (response.ErpResponseModel.IsError)
                    {
                        isError = true;

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                            ErpSyncLevel.Product,
                            response.ErpResponseModel.ErrorShortMessage,
                            response.ErpResponseModel.ErrorFullMessage);

                        await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                            DateTime.UtcNow,
                            ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                            response.ErpResponseModel.ErrorShortMessage + "\n\n" + response.ErpResponseModel.ErrorFullMessage);

                        break;
                    }
                    else if (response.Data is null)
                    {
                        isError = false;
                        break;
                    }

                    previousStart = start;
                    start = response.ErpResponseModel.Next;

                    var responseData = response.Data
                        .Where(x => !string.IsNullOrWhiteSpace(x.Sku.Trim().ToLower()))
                        .GroupBy(x => x.Sku.Trim().ToLower())
                        .Select(g => g.Last());

                    if (responseData == null)
                    {
                        isError = false;
                        break;
                    }

                    totalNotSyncedSoFar += response.Data.Count - responseData.Count();

                    products = (List<Product>?)await _erpProductService
                            .GetProductsBySkuAsync(
                                responseData.Select(x => x.Sku.Trim().ToLower()).ToArray(),
                                filterOutDeleted: true,
                                filterOutUnpublished: false);

                    foreach (var erpProduct in responseData)
                    {
                        #region Products

                        var thisProductIsValid = false;
                        var oldErpProduct = products.FirstOrDefault(x => x.Sku.Trim().ToLower() == erpProduct.Sku.Trim().ToLower());

                        if (oldErpProduct is null)
                        {
                            oldErpProduct = new Product();
                            oldErpProduct.Sku = erpProduct.Sku;
                            oldErpProduct.ManufacturerPartNumber = erpProduct.ManufacturerPartNumber;

                            oldErpProduct.ShortDescription = (erpProduct.ShortDescription.Length > 400) ? erpProduct.ShortDescription.Substring(0, 400) : erpProduct.ShortDescription;
                            oldErpProduct.FullDescription = lineBreakReplacer.Replace((erpProduct.FullDescription.Length > 400)
                                ? erpProduct.FullDescription.Substring(0, 400) : erpProduct.FullDescription, "<br/>");

                            oldErpProduct.Name = string.IsNullOrEmpty(erpProduct.Name) ? erpProduct.Sku : erpProduct.Name;

                            oldErpProduct.ProductType = ProductType.SimpleProduct;
                            oldErpProduct.VisibleIndividually = true;
                            oldErpProduct.AdminComment = $"Created by {programName} (B2B) on {DateTime.UtcNow:u}";
                            oldErpProduct.ProductTemplateId = productTemplate?.Id ?? 0;

                            oldErpProduct.WarehouseId = allWarehouses.FirstOrDefault(x => x.Name.Equals(erpProduct.WarehouseNameOrCode))?.Id ?? 0;
                            oldErpProduct.VendorId = allVendors.Find(x => x.Name.Equals(erpProduct.VendorCode) || x.Name.Equals(erpProduct.VendorName))?.Id ?? 0;

                            oldErpProduct.StockQuantity = Convert.ToInt32(Math.Min(Math.Max(Math.Round(erpProduct.StockQuantity ?? 0), int.MinValue), int.MaxValue));
                            oldErpProduct.OrderMinimumQuantity = 1;

                            oldErpProduct.IsShipEnabled = true;

                            oldErpProduct.PreOrderAvailabilityStartDateTimeUtc = DateTime.UtcNow;
                            oldErpProduct.Price = erpProduct.Price ?? 0;
                            oldErpProduct.OldPrice = erpProduct.Price ?? 0;

                            oldErpProduct.Weight = erpProduct.Weight ?? 0;
                            oldErpProduct.Length = erpProduct.Length ?? 0;
                            oldErpProduct.Height = erpProduct.Height ?? 0;
                            oldErpProduct.Width = erpProduct.Width ?? 0;
                            oldErpProduct.AvailableStartDateTimeUtc = DateTime.UtcNow;
                            oldErpProduct.DisplayOrder = 1;

                            oldErpProduct.ManageInventoryMethodId = _b2BB2CFeaturesSettings.TrackInventoryMethodId;
                            oldErpProduct.LowStockActivityId = _b2BB2CFeaturesSettings.LowStockActivityId_DefaultValue;
                            oldErpProduct.BackorderModeId = _b2BB2CFeaturesSettings.BackorderModeId_DefaultValue;
                            oldErpProduct.AllowBackInStockSubscriptions = _b2BB2CFeaturesSettings.AllowBackInStockSubscriptions_DefaultValue;
                            oldErpProduct.ProductAvailabilityRangeId = _b2BB2CFeaturesSettings.ProductAvailabilityRangeId_DefaultValue;
                            oldErpProduct.AvailableForPreOrder = _b2BB2CFeaturesSettings.AvailableForPreOrder_DefaultValue;
                            oldErpProduct.DisplayStockAvailability = _b2BB2CFeaturesSettings.DisplayStockAvailability_DefaultValue;
                            oldErpProduct.DisplayStockQuantity = _b2BB2CFeaturesSettings.DisplayStockQuantity_DefaultValue;
                            oldErpProduct.OrderMaximumQuantity = _b2BB2CFeaturesSettings.OrderMaximumQuantity;

                            oldErpProduct.Gtin = erpProduct.Gtin;
                            oldErpProduct.ProductCost = erpProduct.ProductCost ?? 0;

                            if (!string.IsNullOrWhiteSpace(erpProduct.TaxCategoryName))
                            {
                                oldErpProduct.TaxCategoryId = allTaxCategories.FirstOrDefault(x => x.Name.Equals(erpProduct.TaxCategoryName))?.Id ?? 0;
                            }

                            oldErpProduct.Published = erpProduct.Published;
                            oldErpProduct.Deleted = false;
                            oldErpProduct.CreatedOnUtc = DateTime.UtcNow;
                            oldErpProduct.UpdatedOnUtc = DateTime.UtcNow;

                            if (await IsthisProductIsValidAsync(oldErpProduct))
                            {
                                thisProductIsValid = true;
                                await _productService.InsertProductAsync(oldErpProduct);
                            }
                        }
                        else
                        {
                            oldErpProduct.Sku = erpProduct.Sku;
                            oldErpProduct.ManufacturerPartNumber = erpProduct.ManufacturerPartNumber;

                            oldErpProduct.ShortDescription = (erpProduct.ShortDescription.Length > 400) ? erpProduct.ShortDescription[..400] : erpProduct.ShortDescription;
                            oldErpProduct.FullDescription = lineBreakReplacer.Replace((erpProduct.FullDescription.Length > 400)
                                ? erpProduct.FullDescription.Substring(0, 400) : erpProduct.FullDescription, "<br/>");

                            oldErpProduct.Name = string.IsNullOrEmpty(erpProduct.Name) ? erpProduct.Sku : erpProduct.Name;

                            oldErpProduct.ProductType = ProductType.SimpleProduct;
                            oldErpProduct.VisibleIndividually = true;
                            oldErpProduct.AdminComment = $"Updated by {programName} (B2B) on {DateTime.UtcNow:u}";
                            oldErpProduct.ProductTemplateId = productTemplate?.Id ?? 0;

                            oldErpProduct.WarehouseId = allWarehouses.FirstOrDefault(x => x.Name.Equals(erpProduct.WarehouseNameOrCode))?.Id ?? 0;
                            oldErpProduct.VendorId = allVendors.Find(x => x.Name.Equals(erpProduct.VendorCode) || x.Name.Equals(erpProduct.VendorName))?.Id ?? 0;

                            oldErpProduct.StockQuantity = Convert.ToInt32(Math.Min(Math.Max(Math.Round(erpProduct.StockQuantity ?? 0), int.MinValue), int.MaxValue));
                            oldErpProduct.OrderMinimumQuantity = 1;

                            oldErpProduct.IsShipEnabled = true;

                            oldErpProduct.PreOrderAvailabilityStartDateTimeUtc = DateTime.UtcNow;
                            oldErpProduct.Price = erpProduct.Price ?? 0;
                            oldErpProduct.OldPrice = erpProduct.Price ?? 0;

                            oldErpProduct.Weight = erpProduct.Weight ?? 0;
                            oldErpProduct.Length = erpProduct.Length ?? 0;
                            oldErpProduct.Height = erpProduct.Height ?? 0;
                            oldErpProduct.Width = erpProduct.Width ?? 0;
                            oldErpProduct.AvailableStartDateTimeUtc = DateTime.UtcNow;
                            oldErpProduct.DisplayOrder = 1;

                            oldErpProduct.ManageInventoryMethodId = _b2BB2CFeaturesSettings.TrackInventoryMethodId;
                            oldErpProduct.LowStockActivityId = _b2BB2CFeaturesSettings.LowStockActivityId_DefaultValue;
                            oldErpProduct.BackorderModeId = _b2BB2CFeaturesSettings.BackorderModeId_DefaultValue;
                            oldErpProduct.AllowBackInStockSubscriptions = _b2BB2CFeaturesSettings.AllowBackInStockSubscriptions_DefaultValue;
                            oldErpProduct.ProductAvailabilityRangeId = _b2BB2CFeaturesSettings.ProductAvailabilityRangeId_DefaultValue;
                            oldErpProduct.AvailableForPreOrder = _b2BB2CFeaturesSettings.AvailableForPreOrder_DefaultValue;
                            oldErpProduct.DisplayStockAvailability = _b2BB2CFeaturesSettings.DisplayStockAvailability_DefaultValue;
                            oldErpProduct.DisplayStockQuantity = _b2BB2CFeaturesSettings.DisplayStockQuantity_DefaultValue;
                            oldErpProduct.OrderMaximumQuantity = _b2BB2CFeaturesSettings.OrderMaximumQuantity;

                            oldErpProduct.Gtin = erpProduct.Gtin;
                            oldErpProduct.ProductCost = erpProduct.ProductCost ?? 0;

                            if (!string.IsNullOrWhiteSpace(erpProduct.TaxCategoryName))
                            {
                                oldErpProduct.TaxCategoryId = allTaxCategories.FirstOrDefault(x => x.Name.Equals(erpProduct.TaxCategoryName))?.Id ?? 0;
                            }

                            oldErpProduct.Published = erpProduct.Published;
                            oldErpProduct.UpdatedOnUtc = DateTime.UtcNow;

                            if (await IsthisProductIsValidAsync(oldErpProduct))
                            {
                                thisProductIsValid = true;
                                await _productService.UpdateProductAsync(oldErpProduct);
                            }
                        }

                        //search engine name
                        await SaveOrUpdateEntitySeNameAsync(oldErpProduct);

                        #endregion

                        #region Categories

                        if (erpProduct.ProductCategories.Any())
                        {
                            var incommingCategoryIds = new List<int>();
                            var categories = erpProduct.ProductCategories.ToList();
                            var parentCategoryId = 0;

                            for (int i = 0; i < categories.Count; i++)
                            {
                                if (string.IsNullOrWhiteSpace(categories[i].CategoryName))
                                {
                                    continue;
                                }

                                var categoriesWithSameName = allCategories.Where
                                    (category => category.Name.ToLower().Trim() == categories[i].CategoryName.ToLower().Trim());

                                var currentCategory = categoriesWithSameName.FirstOrDefault();

                                #region Delete all categories with same name except the first one

                                var categoriesToDelete = new List<Category>();
                                var productCategoriesToDelete = new List<ProductCategory>();

                                foreach (var category in categoriesWithSameName.Skip(1).ToList())
                                {
                                    productCategoriesToDelete.AddRange(allProductCategories.Where(pc => pc.CategoryId == category.Id));
                                    categoriesToDelete.Add(category);
                                }
                                foreach (var productCategory in productCategoriesToDelete)
                                {
                                    allProductCategories.Remove(productCategory);
                                    await _categoryService.DeleteProductCategoryAsync(productCategory);
                                }
                                if (categoriesToDelete.Count > 0)
                                {
                                    categoriesToDelete.ForEach(x => allCategories.Remove(x));
                                    await _categoryService.DeleteCategoriesAsync(categoriesToDelete);
                                }

                                #endregion

                                if (currentCategory is null)
                                {
                                    currentCategory = new Category();
                                    currentCategory.Name = categories[i].CategoryName.Trim();
                                    currentCategory.Description = categories[i].Description;
                                    currentCategory.CategoryTemplateId = categoryTemplate?.Id ?? 0;
                                    currentCategory.CreatedOnUtc = DateTime.UtcNow;
                                    currentCategory.UpdatedOnUtc = DateTime.UtcNow;
                                    currentCategory.Published = true;
                                    currentCategory.AllowCustomersToSelectPageSize = true;
                                    currentCategory.PageSize = CATEGORY_PAGE_SIZE;
                                    currentCategory.ParentCategoryId = parentCategoryId;
                                    await _categoryService.InsertCategoryAsync(currentCategory);

                                    if (await IsValidCategoryAsync(currentCategory))
                                    {
                                        await _categoryService.InsertCategoryAsync(currentCategory);
                                        allCategories.Add(currentCategory);
                                    }
                                }
                                else
                                {
                                    currentCategory.UpdatedOnUtc = DateTime.UtcNow;
                                    await _categoryService.UpdateCategoryAsync(currentCategory);
                                }

                                if (await IsValidCategoryAsync(currentCategory))
                                {
                                    //search engine name
                                    await SaveOrUpdateEntitySeNameAsync(currentCategory);

                                    parentCategoryId = currentCategory.Id;
                                    incommingCategoryIds.Add(currentCategory.Id);
                                }
                            }

                            if (thisProductIsValid)
                            {
                                var existingCategoryMapping = allProductCategories.Where(pc => pc.ProductId == oldErpProduct.Id).ToList();

                                if (parentCategoryId > 0 &&
                                    !existingCategoryMapping.Exists(a => a.CategoryId == parentCategoryId && a.ProductId == oldErpProduct.Id))
                                {
                                    var newProductCategory = new ProductCategory
                                    {
                                        ProductId = oldErpProduct.Id,
                                        CategoryId = parentCategoryId,
                                        DisplayOrder = 0
                                    };

                                    await _categoryService.InsertProductCategoryAsync(newProductCategory);

                                    allProductCategories.Add(newProductCategory);
                                    existingCategoryMapping.Add(newProductCategory);
                                }

                                if (parentCategoryId > 0)
                                {
                                    foreach (var category in existingCategoryMapping.Where(a => a.CategoryId != parentCategoryId))
                                    {
                                        await _categoryService.DeleteProductCategoryAsync(category);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Specification Attribute

                        foreach (var attribute in erpProduct.ProductAttributes)
                        {
                            var specAttr = specificationAttributes.FirstOrDefault(sa => sa.Name == attribute.Key);
                            if (specAttr is null)
                            {
                                // If it doesn't exist yet, we create it
                                specAttr = new SpecificationAttribute
                                {
                                    DisplayOrder = 0,
                                    Name = attribute.Key
                                };
                                await _specificationAttributeService.InsertSpecificationAttributeAsync(specAttr);
                                specificationAttributes.Add(specAttr);
                            }

                            if (string.IsNullOrWhiteSpace(attribute.Value))
                                continue;

                            var specificationAttributeOptions = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specAttr.Id);
                            var existingSpecAttrOptionIds = new List<int>();
                            var productSpecAttrMappingToDelete = new List<ProductSpecificationAttribute>();
                            var attributeValues = attribute.Value.Trim().Split(',', StringSplitOptions.RemoveEmptyEntries);

                            foreach (var attrVal in attributeValues.Select(x => x.Trim()))
                            {
                                var specAttrOptionId = specificationAttributeOptions?.FirstOrDefault(o => o.Name == attrVal)?.Id ?? 0;

                                if (specAttrOptionId == 0)
                                {
                                    // if no such option exists we create it
                                    var specAttrOption = new SpecificationAttributeOption();
                                    specAttrOption.Name = attrVal;
                                    specAttrOption.SpecificationAttributeId = specAttr.Id;

                                    await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(specAttrOption);
                                    specAttrOptionId = specAttrOption.Id;
                                }

                                if (thisProductIsValid)
                                {
                                    var psaMappings = await _specificationAttributeService.GetProductSpecificationAttributesAsync(oldErpProduct.Id, specAttrOptionId);
                                    var psaMapping = psaMappings.FirstOrDefault();

                                    if (psaMapping is null)
                                    {
                                        psaMapping = new ProductSpecificationAttribute();
                                        psaMapping.ProductId = oldErpProduct.Id;
                                        psaMapping.SpecificationAttributeOptionId = specAttrOptionId;
                                        psaMapping.AttributeTypeId = (int)SpecificationAttributeType.Option;
                                        psaMapping.CustomValue = attrVal;
                                        if (specAttr.Name.Equals(uomSpecificAttribute?.Name))
                                        {
                                            psaMapping.ShowOnProductPage = true;
                                            psaMapping.AllowFiltering = true;
                                        }
                                        if (specAttr.Name.Equals(preFilterSpecificAttribute?.Name))
                                        {
                                            psaMapping.ShowOnProductPage = false;
                                            psaMapping.AllowFiltering = false;
                                        }
                                        await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psaMapping);
                                    }
                                    else
                                    {
                                        psaMapping.SpecificationAttributeOptionId = specAttrOptionId;
                                        psaMapping.AttributeTypeId = (int)SpecificationAttributeType.Option;
                                        psaMapping.CustomValue = attrVal;
                                        if (specAttr.Name.Equals(uomSpecificAttribute?.Name ?? "UnitOfMeasure"))
                                        {
                                            psaMapping.ShowOnProductPage = true;
                                            psaMapping.AllowFiltering = true;
                                        }
                                        if (specAttr.Name.Equals(preFilterSpecificAttribute?.Name ?? "PrefilterFacet"))
                                        {
                                            psaMapping.ShowOnProductPage = false;
                                            psaMapping.AllowFiltering = false;
                                        }
                                        await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(psaMapping);
                                    }

                                    existingSpecAttrOptionIds.Add(specAttrOptionId);
                                    productSpecAttrMappingToDelete.AddRange(psaMappings?.Where(x => x.Id != psaMapping.Id));
                                }
                            }

                            if (thisProductIsValid)
                            {
                                foreach (var specAttrOption in specificationAttributeOptions?.Where(o => !existingSpecAttrOptionIds.Contains(o.Id)))
                                {
                                    productSpecAttrMappingToDelete.AddRange(await _specificationAttributeService.GetProductSpecificationAttributesAsync(oldErpProduct.Id, specAttrOption.Id));
                                }
                            }

                            if (productSpecAttrMappingToDelete.Count != 0)
                            {
                                foreach (var psaMapping in productSpecAttrMappingToDelete)
                                {
                                    if (psaMapping != null)
                                    {
                                        await _specificationAttributeService.DeleteProductSpecificationAttributeAsync(psaMapping);
                                    }
                                }
                            }
                        }

                        #endregion

                        #region Manufacturer

                        if (!string.IsNullOrWhiteSpace(erpProduct.ManufacturerName))
                        {
                            var existingProductManufacturers = await _manufacturerService.GetProductManufacturersByProductIdAsync(oldErpProduct.Id, showHidden: true);

                            var manufacturersWithSameName = allManufacturers
                                .Where(mft => mft.Name.ToLower().Trim().Equals(erpProduct.ManufacturerName.ToLower().Trim())
                                || mft.Name.ToLower().Trim().Equals(erpProduct.ManufacturerCode.ToLower().Trim()));

                            var currentManufacturer = manufacturersWithSameName.FirstOrDefault();

                            #region Delete all manufacturers with same name except the first one

                            foreach (var manufacturer in manufacturersWithSameName.Skip(1).ToList())
                            {
                                var productManufacturersToDelete = existingProductManufacturers
                                    .Where(pm => pm.ManufacturerId == manufacturer.Id)
                                    .ToList();

                                foreach (var productManufacturer in productManufacturersToDelete)
                                {
                                    existingProductManufacturers.Remove(productManufacturer);
                                    await _manufacturerService.DeleteProductManufacturerAsync(productManufacturer);
                                }

                                allManufacturers.Remove(manufacturer);
                                await _manufacturerService.DeleteManufacturerAsync(manufacturer);
                            }

                            #endregion

                            if (currentManufacturer is null)
                            {
                                currentManufacturer = new Manufacturer();
                                currentManufacturer.Name = erpProduct.ManufacturerName.Trim();
                                currentManufacturer.ManufacturerTemplateId = manufacturerTemplate?.Id ?? 0;
                                currentManufacturer.PageSize = MANUFACTURER_PAGE_SIZE;
                                currentManufacturer.AllowCustomersToSelectPageSize = true;
                                currentManufacturer.PageSizeOptions = MANUFACTURER_PAGE_SIZE_OPTIONS;
                                currentManufacturer.PriceRangeFiltering = true;
                                currentManufacturer.ManuallyPriceRange = true;
                                currentManufacturer.PriceFrom = NopCatalogDefaults.DefaultPriceRangeFrom;
                                currentManufacturer.PriceTo = NopCatalogDefaults.DefaultPriceRangeTo;
                                currentManufacturer.Published = true;
                                currentManufacturer.DisplayOrder = 1;
                                currentManufacturer.CreatedOnUtc = DateTime.UtcNow;
                                currentManufacturer.UpdatedOnUtc = DateTime.UtcNow;

                                if (await IsValidManufacturerAsync(currentManufacturer))
                                {
                                    await _manufacturerService.InsertManufacturerAsync(currentManufacturer);
                                    allManufacturers.Add(currentManufacturer);
                                }
                            }
                            else
                            {
                                currentManufacturer.UpdatedOnUtc = DateTime.UtcNow;
                                await _manufacturerService.UpdateManufacturerAsync(currentManufacturer);
                            }

                            if (await IsValidManufacturerAsync(currentManufacturer) && thisProductIsValid)
                            {
                                //search engine name
                                await SaveOrUpdateEntitySeNameAsync(currentManufacturer);

                                if (existingProductManufacturers.Any())
                                {
                                    var productManufacturer = existingProductManufacturers.FirstOrDefault();
                                    var productManufacturersToDelete = existingProductManufacturers.Skip(1).ToList();

                                    foreach (var prodMfct in productManufacturersToDelete)
                                    {
                                        existingProductManufacturers.Remove(prodMfct);
                                        await _manufacturerService.DeleteProductManufacturerAsync(prodMfct);
                                    }

                                    if (productManufacturer.ManufacturerId != currentManufacturer.Id)
                                    {
                                        productManufacturer.ManufacturerId = currentManufacturer.Id;
                                        await _manufacturerService.UpdateProductManufacturerAsync(productManufacturer);
                                    }
                                }
                                else
                                {
                                    var newProductManufacturer = new ProductManufacturer
                                    {
                                        ManufacturerId = currentManufacturer.Id,
                                        ProductId = oldErpProduct.Id,
                                        IsFeaturedProduct = false,
                                        DisplayOrder = 1
                                    };

                                    await _manufacturerService.InsertProductManufacturerAsync(newProductManufacturer);
                                }
                            }
                        }

                        #endregion

                        if (thisProductIsValid)
                        {
                            lastSyncedErpProduct = oldErpProduct.Sku;
                            totalSyncedSoFar++;
                        }
                        else
                        {
                            totalNotSyncedSoFar++;
                        }
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                            ErpSyncLevel.Product,
                            $"The Erp Product Sync run is cancelled for Sales Org: ({salesOrg.Code}) {salesOrg.Name}." +
                            (!string.IsNullOrWhiteSpace(lastSyncedErpProduct) ? 
                            $"The last synced Erp Product: {lastSyncedErpProduct} in this batch. " : string.Empty) +
                            $"Total products synced so far: {totalSyncedSoFar} " +
                            $"And total products not sync due to invalid data: {totalNotSyncedSoFar}");

                        return false;
                    }

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                        ErpSyncLevel.Product,
                        (!string.IsNullOrWhiteSpace(lastSyncedErpProduct) ?
                        $"The last synced Erp Product: {lastSyncedErpProduct} in this batch. " : string.Empty) +
                        $"Total product synced so far: {totalSyncedSoFar}");
                }

                if (!isError)
                {
                    //await _erpProductService.UnpublishAllOldProduct(syncStartTime);
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                        ErpSyncLevel.Product,
                        $"Erp Product sync successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }
                else
                {
                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                        ErpSyncLevel.Product,
                        $"Erp Product sync is partially or not successful for Sales Org: ({salesOrg.Code}) {salesOrg.Name}");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                    ErpSyncLevel.Product,
                    (!string.IsNullOrWhiteSpace(lastSyncedErpProduct) ? 
                    $"The last synced Erp Product: {lastSyncedErpProduct}. " : string.Empty) +
                    $"Total product synced so far: {totalSyncedSoFar} " +
                    $"And total products not sync due to invalid data: {totalNotSyncedSoFar}");

                salesOrg.LastErpProductSyncTimeOnUtc = DateTime.UtcNow;
                await _erpSalesOrgService.UpdateErpSalesOrgAsync(salesOrg);
            }

            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");
            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                ErpSyncLevel.Product,
                "Erp Product Sync ended.");

            return true;
        }
        catch (Exception ex)
        {
            await _staticCacheManager.RemoveByPrefixAsync("nop.pres.jcarousel.");

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                ErpSyncLevel.Product,
                ex.Message,
                ex.StackTrace ?? string.Empty);

            await _syncWorkflowMessageService.SendSyncFailNotificationAsync(
                DateTime.UtcNow,
                ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                ex.Message + "\n\n" + ex.StackTrace);

            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                ErpSyncLevel.Product,
                "Erp Product Sync ended.");

            return false;
        }
    }

    #endregion
}