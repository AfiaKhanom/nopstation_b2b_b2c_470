using System.Reflection;
using System.Text.RegularExpressions;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Common;
using Nop.Core.Domain.Shipping;
using Nop.Core.Domain.Tax;
using Nop.Core.Domain.Vendors;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Services.Vendors;
using NopStation.Plugin.B2B.B2BB2CFeatures;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Model;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskServices
{
    public class ErpProductSyncService : IErpProductSyncService
    {
        #region Fields

        private readonly TaxSettings _taxSettings;
        private readonly IStoreContext _storeContext;
        private readonly IVendorService _vendorService;
        private readonly ISettingService _settingService;
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IShippingService _shippingService;
        private readonly IUrlRecordService _urlRecordService;
        private readonly IManufacturerService _manufacturerService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IManufacturerTemplateService _manufacturerTemplateService;
        private readonly ISpecificationAttributeService _specificationAttributeService;
        private readonly ISyncLogService _erpSyncLogService;
        private readonly IErpProductService _erpProductService;
        private readonly IErpDataClearCacheService _erpDataClearCacheService;
        private readonly IErpIntegrationPluginManager _erpIntegrationPluginService;
        private readonly IErpSalesOrgService _erpSalesOrgService;
        private const int CATEGORY_PAGE_SIZE = 5;
        private const string MANUFACTURER_TEMPLATE_NAME = "Products in Grid or Lines";
        private const int MANUFACTURER_PAGE_SIZE = 6;
        private const string MANUFACTURER_PAGE_SIZE_OPTIONS = "6, 3, 9";

        #endregion

        #region Ctor

        public ErpProductSyncService(
            TaxSettings taxSettings,
            IStoreContext storeContext,
            IVendorService vendorService,
            ISettingService settingService,
            IProductService productService,
            ICategoryService categoryService,
            IShippingService shippingService,
            IUrlRecordService urlRecordService,
            IManufacturerService manufacturerService,
            IGenericAttributeService genericAttributeService,
            IManufacturerTemplateService manufacturerTemplateService,
            ISpecificationAttributeService specificationAttributeService,
            ISyncLogService erpSyncLogService,
            IErpProductService erpProductService,
            IErpDataClearCacheService erpDataClearCacheService,
            IErpIntegrationPluginManager erpIntegrationPluginService,
            IErpSalesOrgService erpSalesOrgService)
        {
            _taxSettings = taxSettings;
            _storeContext = storeContext;
            _vendorService = vendorService;
            _settingService = settingService;
            _productService = productService;
            _categoryService = categoryService;
            _shippingService = shippingService;
            _urlRecordService = urlRecordService;
            _manufacturerService = manufacturerService;
            _genericAttributeService = genericAttributeService;
            _manufacturerTemplateService = manufacturerTemplateService;
            _specificationAttributeService = specificationAttributeService;
            _erpSyncLogService = erpSyncLogService;
            _erpProductService = erpProductService;
            _erpDataClearCacheService = erpDataClearCacheService;
            _erpIntegrationPluginService = erpIntegrationPluginService;
            _erpSalesOrgService = erpSalesOrgService;
        }

        #endregion

        #region Utilities

        private async Task SaveOrUpdateEntitySeNameAsync<T>(T entity) where T : BaseEntity
        {
            if (entity is Category category)
            {
                var seName = await _urlRecordService.GetSeNameAsync(category);

                if (string.IsNullOrEmpty(seName))
                {
                    seName = await _urlRecordService.ValidateSeNameAsync(category, string.Empty, category.Name, true);
                    await _urlRecordService.SaveSlugAsync(category, seName, 0);
                }
            }
            else if (entity is Product product)
            {
                var seName = await _urlRecordService.GetSeNameAsync(product);

                if (string.IsNullOrEmpty(seName))
                {
                    seName = await _urlRecordService.ValidateSeNameAsync(product, string.Empty, product.Name, true);
                    await _urlRecordService.SaveSlugAsync(product, seName, 0);
                }
            }
            else if (entity is Manufacturer manufacturer)
            {
                var seName = await _urlRecordService.GetSeNameAsync(manufacturer);

                if (string.IsNullOrEmpty(seName))
                {
                    seName = await _urlRecordService.ValidateSeNameAsync(manufacturer, string.Empty, manufacturer.Name, true);
                    await _urlRecordService.SaveSlugAsync(manufacturer, seName, 0);
                }
            }
        }

        #endregion

        #region Method

        public async virtual Task<bool> IsErpProductSyncSuccessfulAsync()
        {
            var erpIntegrationPlugin = await _erpIntegrationPluginService.LoadActiveERPIntegrationPlugin();

            if (erpIntegrationPlugin is null)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                    ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                    ErpSyncLavel.Product,
                    "No integration method found.");

                return false;
            }

            var warehouse = await _shippingService.GetNearestWarehouseAsync(new Address()) ?? new Warehouse();
            var erpDataSchedulersettings = await _settingService.LoadSettingAsync<ErpDataSchedulerSettings>();

            var lineBreakReplacer = new Regex(@"\r?\n");
            var programName = Assembly.GetExecutingAssembly().GetName().Name;

            var previousStart = "0";
            var lastErpProductSynced = new Product();
            var syncStartTime = DateTime.UtcNow.AddMinutes(-10);

            try
            {
                #region Collections

                var allVendors = (await _vendorService.GetAllVendorsAsync()).ToList();

                var allManufacturers = (await _manufacturerService.GetAllManufacturersAsync()).ToList();
                var manufacturerTemplateInGridAndLines = (await _manufacturerTemplateService.GetAllManufacturerTemplatesAsync())
                    .FirstOrDefault(mftemp => mftemp.Name == MANUFACTURER_TEMPLATE_NAME) ?? new ManufacturerTemplate();

                var specificationAttributes = await _specificationAttributeService.GetSpecificationAttributesAsync();

                var allCategories = (await _categoryService.GetAllCategoriesAsync()).ToList();
                var allProductCategories = new List<ProductCategory>();
                foreach (var category in allCategories)
                {
                    allProductCategories.AddRange((await _categoryService.GetProductCategoriesByCategoryIdAsync(category.Id)).ToList());
                }

                var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var b2BB2CFeaturesSettings = await _settingService.LoadSettingAsync<B2BB2CFeaturesSettings>(storeScope);
                var listOfSalesOrgs = new List<ErpSalesOrg>();
                var salesOrgCode = await erpIntegrationPlugin.GetSalesOrgCodeFromIQIntegrationSettings();

                if (!string.IsNullOrWhiteSpace(salesOrgCode))
                {
                    var salesOrg = (await _erpSalesOrgService.GetAllErpSalesOrgAsync(code: salesOrgCode)).FirstOrDefault();

                    if (salesOrg == null)
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpStockSyncTaskName,
                        ErpSyncLavel.Stock,
                        $"No Sales org found with Sales org code: {salesOrgCode}. Unable to run {ErpDataSchedulerDefaults.ErpStockSyncTaskName}.");

                        return false;
                    }
                    else
                    {
                        listOfSalesOrgs.Add(salesOrg);
                    }
                }
                else
                {
                    var salesOrgs = await _erpSalesOrgService.GetAllErpSalesOrgsAsync();

                    if (salesOrgs.Any())
                    {
                        listOfSalesOrgs.AddRange(salesOrgs);
                    }
                }

                #endregion

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                        ErpSyncLavel.Product,
                        "Erp Product Sync started.");

                foreach (var salesOrg in listOfSalesOrgs)
                {
                    var start = "0";
                    previousStart = "0";
                    var isError = false;
                    var totalSyncedSoFar = 0;
                    lastErpProductSynced = new Product();
                    if (erpDataSchedulersettings.StartProductSyncAfterLastSyncedProduct)
                    {
                        start = (await _genericAttributeService.GetAttributeAsync<string?>(new Product(), ErpDataSchedulerDefaults.LastSyncedProductSkuFromErp)) ?? "0";
                        syncStartTime = (await _genericAttributeService.GetAttributeAsync<DateTime?>(new Product(), ErpDataSchedulerDefaults.LastSyncStartTimeBeforeDisruption)) ?? DateTime.UtcNow.AddMinutes(-10);
                    }

                    while (true)
                    {
                        var erpGetRequestModel = new ErpGetRequestModel
                        {
                            Start = start,
                            Location = salesOrg.Code
                        };

                        var response = await erpIntegrationPlugin.GetProductsFromErpAsync(erpGetRequestModel);

                        if (response.ErpResponseModel.IsError || response.Data is null)
                        {
                            isError = true;

                            await _erpSyncLogService.SyncLogSaveOnFileAsync(
                                ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                                ErpSyncLavel.Product,
                                response.ErpResponseModel.ErrorShortMessage,
                                response.ErpResponseModel.ErrorFullMessage);

                            break;
                        }

                        previousStart = start;
                        start = response.ErpResponseModel.Next;

                        foreach (var erpProduct in response.Data)
                        {
                            var oldErpProduct = await _productService.GetProductBySkuAsync(erpProduct.ItemNo) ?? new Product();
                            if (oldErpProduct.Id <= 0)
                            {
                                oldErpProduct.Sku = erpProduct.ItemNo;
                                oldErpProduct.ManufacturerPartNumber = erpProduct.MasterCode ?? string.Empty;

                                oldErpProduct.ShortDescription = (erpProduct.Description.Length > 400) ? erpProduct.Description.Substring(0, 400) : erpProduct.Description;
                                oldErpProduct.FullDescription = lineBreakReplacer.Replace((erpProduct.FullDescription.Length > 400)
                                    ? erpProduct.FullDescription.Substring(0, 400) : erpProduct.FullDescription, "<br/>");

                                oldErpProduct.Name = string.IsNullOrEmpty(erpProduct.Description) ? erpProduct.ItemNo : erpProduct.Description;

                                oldErpProduct.ProductType = ProductType.SimpleProduct;
                                oldErpProduct.VisibleIndividually = true;
                                oldErpProduct.AdminComment = $"Created by {programName} (B2B) on {DateTime.UtcNow.ToString("u")}";
                                oldErpProduct.ProductTemplateId = 1;

                                var vendor = allVendors.Find(vendor => vendor.Name == erpProduct.VendorName) ?? new Vendor();

                                oldErpProduct.VendorId = vendor.Id;
                                oldErpProduct.WarehouseId = warehouse.Id;

                                oldErpProduct.StockQuantity = int.Max(Convert.ToInt32(erpProduct.InStockforLocNo), 0);
                                oldErpProduct.OrderMinimumQuantity = 1;

                                oldErpProduct.AllowAddingOnlyExistingAttributeCombinations = true;
                                oldErpProduct.IsShipEnabled = true;

                                oldErpProduct.PreOrderAvailabilityStartDateTimeUtc = DateTime.UtcNow;
                                oldErpProduct.Price = erpProduct.SellingPriceA ?? 0;
                                oldErpProduct.OldPrice = erpProduct.SellingPriceA ?? 0;

                                oldErpProduct.Weight = erpProduct.Weight ?? 0;
                                oldErpProduct.Length = erpProduct.Length ?? 0;
                                oldErpProduct.Height = erpProduct.Height ?? 0;
                                oldErpProduct.AvailableStartDateTimeUtc = DateTime.UtcNow;
                                oldErpProduct.DisplayOrder = 1;

                                oldErpProduct.ManageInventoryMethodId = b2BB2CFeaturesSettings.TrackInventoryMethodId;
                                oldErpProduct.LowStockActivityId = b2BB2CFeaturesSettings.LowStockActivityId_DefaultValue;
                                oldErpProduct.BackorderModeId = b2BB2CFeaturesSettings.BackorderModeId_DefaultValue;
                                oldErpProduct.AllowBackInStockSubscriptions = b2BB2CFeaturesSettings.AllowBackInStockSubscriptions_DefaultValue;
                                oldErpProduct.ProductAvailabilityRangeId = b2BB2CFeaturesSettings.ProductAvailabilityRangeId_DefaultValue;
                                oldErpProduct.AvailableForPreOrder = b2BB2CFeaturesSettings.AvailableForPreOrder_DefaultValue;
                                oldErpProduct.DisplayStockAvailability = b2BB2CFeaturesSettings.DisplayStockAvailability_DefaultValue;
                                oldErpProduct.DisplayStockQuantity = b2BB2CFeaturesSettings.DisplayStockQuantity_DefaultValue;
                                oldErpProduct.OrderMaximumQuantity = b2BB2CFeaturesSettings.OrderMaximumQuantity;

                                if (erpProduct.VatRate == "1")
                                {
                                    oldErpProduct.IsTaxExempt = false;
                                    oldErpProduct.TaxCategoryId = _taxSettings.DefaultTaxCategoryId;
                                }

                                oldErpProduct.Published = true;
                                oldErpProduct.CreatedOnUtc = DateTime.UtcNow;
                                oldErpProduct.UpdatedOnUtc = DateTime.UtcNow;

                                await _productService.InsertProductAsync(oldErpProduct);
                            }
                            else
                            {
                                oldErpProduct.ShortDescription = (erpProduct.Description.Length > 400) ? erpProduct.Description.Substring(0, 400) : erpProduct.Description;

                                oldErpProduct.FullDescription = lineBreakReplacer.Replace((erpProduct.FullDescription.Length > 400)
                                    ? erpProduct.FullDescription.Substring(0, 400) : erpProduct.FullDescription, "<br/>");

                                oldErpProduct.Name = string.IsNullOrEmpty(erpProduct.Description) ? erpProduct.ItemNo : erpProduct.Description;

                                oldErpProduct.AdminComment = $"Updated by {programName} (B2B) on {DateTime.UtcNow.ToString("u")}";

                                oldErpProduct.Weight = erpProduct.Weight ?? 0;
                                oldErpProduct.Length = erpProduct.Length ?? 0;
                                oldErpProduct.Height = erpProduct.Height ?? 0;

                                if (erpProduct.VatRate == "1")
                                {
                                    oldErpProduct.IsTaxExempt = false;
                                    oldErpProduct.TaxCategoryId = _taxSettings.DefaultTaxCategoryId;
                                }
                                else if (erpProduct.VatRate == "0")
                                {
                                    oldErpProduct.IsTaxExempt = true;
                                    oldErpProduct.TaxCategoryId = 0;
                                }

                                oldErpProduct.UpdatedOnUtc = DateTime.UtcNow;

                                await _productService.UpdateProductAsync(oldErpProduct);
                            }

                            //search engine name
                            await SaveOrUpdateEntitySeNameAsync(oldErpProduct);

                            #region Categories

                            if (erpProduct.Categories.Any())
                            {
                                var incommingCategoryIds = new List<int>();
                                var categories = erpProduct.Categories.ToList();
                                var parentCategoryId = 0;

                                var existingCategoryMapping = allProductCategories.Where(pc => pc.ProductId == oldErpProduct.Id).ToList();

                                for (int i = 0; i < categories.Count; i++)
                                {
                                    if (string.IsNullOrEmpty(categories[i].CategoryName))
                                    {
                                        continue;
                                    }
                                    var currentCategory = allCategories.Find(category => category.Name == categories[i].CategoryName) ?? new Category();

                                    if (currentCategory.Id == 0)
                                    {
                                        currentCategory.Name = categories[i].CategoryName;
                                        currentCategory.Description = string.Empty;
                                        currentCategory.CategoryTemplateId = 1;
                                        currentCategory.CreatedOnUtc = DateTime.UtcNow;
                                        currentCategory.UpdatedOnUtc = DateTime.UtcNow;
                                        currentCategory.Published = true;
                                        currentCategory.AllowCustomersToSelectPageSize = true;
                                        currentCategory.PageSize = CATEGORY_PAGE_SIZE;
                                        currentCategory.ParentCategoryId = parentCategoryId;
                                        currentCategory.ShowOnHomepage = true;
                                        currentCategory.IncludeInTopMenu = true;
                                        await _categoryService.InsertCategoryAsync(currentCategory);

                                        allCategories.Add(currentCategory);
                                    }
                                    else
                                    {
                                        currentCategory.UpdatedOnUtc = DateTime.UtcNow;
                                        await _categoryService.UpdateCategoryAsync(currentCategory);
                                    }

                                    //search engine name
                                    await SaveOrUpdateEntitySeNameAsync(currentCategory);

                                    parentCategoryId = currentCategory.Id;
                                    incommingCategoryIds.Add(currentCategory.Id);

                                    if (!existingCategoryMapping.Any(a => a.CategoryId == parentCategoryId))
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
                                }

                                foreach (var category in existingCategoryMapping.Where(a => !incommingCategoryIds.Exists(id => id == a.CategoryId)))
                                {
                                    await _categoryService.DeleteProductCategoryAsync(category);
                                }
                            }

                            #endregion

                            #region Specification Attribute

                            foreach (var attribute in erpProduct.Attributes)
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

                                var specificationAttributeOptions = await _specificationAttributeService.GetSpecificationAttributeOptionsBySpecificationAttributeAsync(specAttr.Id);
                                int specAttrOptionId = specificationAttributeOptions.FirstOrDefault(o => o.Name == attribute.Value)?.Id ?? 0;

                                if (specAttrOptionId == 0)
                                {
                                    // if no such option exists we create it
                                    var specAttrOption = new SpecificationAttributeOption();
                                    specAttrOption.Name = attribute.Value;
                                    specAttrOption.SpecificationAttributeId = specAttr.Id;

                                    await _specificationAttributeService.InsertSpecificationAttributeOptionAsync(specAttrOption);
                                    specAttrOptionId = specAttrOption.Id;
                                }

                                var psaMapping = (await _specificationAttributeService.GetProductSpecificationAttributesAsync(oldErpProduct.Id, specAttrOptionId)).FirstOrDefault();
                                if (psaMapping != null)
                                {
                                    psaMapping.SpecificationAttributeOptionId = specAttrOptionId;
                                    psaMapping.AttributeTypeId = (int)SpecificationAttributeType.Option;
                                    psaMapping.CustomValue = attribute.Value;
                                    psaMapping.ShowOnProductPage = true;
                                    await _specificationAttributeService.UpdateProductSpecificationAttributeAsync(psaMapping);
                                }
                                else
                                {
                                    psaMapping = new ProductSpecificationAttribute()
                                    {
                                        ProductId = oldErpProduct.Id,
                                        SpecificationAttributeOptionId = specAttrOptionId,
                                        AttributeTypeId = (int)SpecificationAttributeType.Option,
                                        CustomValue = attribute.Value
                                    };
                                    await _specificationAttributeService.InsertProductSpecificationAttributeAsync(psaMapping);
                                }

                            }

                            #endregion

                            #region Manufacturer

                            if (!string.IsNullOrWhiteSpace(erpProduct.Brand))
                            {
                                var currentManufacturer = allManufacturers.Find(mft => mft.Name == erpProduct.Brand) ?? new Manufacturer();

                                if (currentManufacturer.Id == 0)
                                {
                                    currentManufacturer.Name = erpProduct.Brand;
                                    currentManufacturer.ManufacturerTemplateId = manufacturerTemplateInGridAndLines.Id;
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

                                    await _manufacturerService.InsertManufacturerAsync(currentManufacturer);

                                    allManufacturers.Add(currentManufacturer);
                                }
                                else
                                {
                                    currentManufacturer.UpdatedOnUtc = DateTime.UtcNow;
                                    await _manufacturerService.UpdateManufacturerAsync(currentManufacturer);
                                }

                                //search engine name
                                await SaveOrUpdateEntitySeNameAsync(currentManufacturer);

                                var existingProductManufacturers = await _manufacturerService.GetProductManufacturersByManufacturerIdAsync(currentManufacturer.Id, showHidden: true);

                                var alreadyExistsProductManufacturer = _manufacturerService.FindProductManufacturer(existingProductManufacturers, oldErpProduct.Id, currentManufacturer.Id);

                                //whether product manufacturer with such parameters already exists
                                if (alreadyExistsProductManufacturer == null)
                                {
                                    //insert the new product manufacturer mapping
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

                            #endregion

                            lastErpProductSynced = oldErpProduct;
                            totalSyncedSoFar++;

                            #region Cache clear for this erp product

                            await _erpDataClearCacheService.ClearCacheOfEntity(oldErpProduct, oldErpProduct.Id);

                            #endregion
                        }

                        if (erpDataSchedulersettings.StartProductSyncAfterLastSyncedProduct)
                        {
                            await _genericAttributeService.SaveAttributeAsync<string?>(new Product(), ErpDataSchedulerDefaults.LastSyncedProductSkuFromErp, $"{lastErpProductSynced?.Sku ?? previousStart}");
                        }

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                            ErpSyncLavel.Product,
                            (lastErpProductSynced is not null ? $"The last synced Erp Product: {lastErpProductSynced.Sku} in this batch." : string.Empty) + $"Total product synced so far: {totalSyncedSoFar}");
                    }

                    if (!isError)
                    {
                        await _erpProductService.UnpublishAllOldProduct(syncStartTime);

                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                            ErpSyncLavel.Product,
                            $"Erp Product sync successful. The products which were updated before {syncStartTime} are unpublished.");

                        // Clear attributes if sync successful.
                        if (erpDataSchedulersettings.StartProductSyncAfterLastSyncedProduct)
                        {
                            await _genericAttributeService.SaveAttributeAsync<string?>(new Product(), ErpDataSchedulerDefaults.LastSyncedProductSkuFromErp, null);
                            await _genericAttributeService.SaveAttributeAsync<DateTime?>(new Product(), ErpDataSchedulerDefaults.LastSyncStartTimeBeforeDisruption, null);
                        }
                    }
                    else
                    {
                        await _erpSyncLogService.SyncLogSaveOnFileAsync(
                            ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                            ErpSyncLavel.Product,
                             $"Erp Product sync is partially or not successful");

                        if (erpDataSchedulersettings.StartProductSyncAfterLastSyncedProduct)
                        {
                            await _genericAttributeService.SaveAttributeAsync<string?>(new Product(), ErpDataSchedulerDefaults.LastSyncedProductSkuFromErp, $"{lastErpProductSynced?.Sku ?? previousStart}");
                            await _genericAttributeService.SaveAttributeAsync<DateTime?>(new Product(), ErpDataSchedulerDefaults.LastSyncStartTimeBeforeDisruption, syncStartTime);
                        }
                    }

                    await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                        ErpSyncLavel.Product,
                        (lastErpProductSynced is not null ? $"The last synced Erp Product: {lastErpProductSynced.Sku}. " : string.Empty) + $"Total synced in this session: {totalSyncedSoFar}");
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                        ErpSyncLavel.Product,
                        "Erp Product Sync ended.");

                return true;
            }
            catch (Exception ex)
            {
                if (erpDataSchedulersettings.StartProductSyncAfterLastSyncedProduct)
                {
                    await _genericAttributeService.SaveAttributeAsync<string?>(new Product(), ErpDataSchedulerDefaults.LastSyncedProductSkuFromErp, $"{lastErpProductSynced?.Sku ?? previousStart}");
                    await _genericAttributeService.SaveAttributeAsync<DateTime?>(new Product(), ErpDataSchedulerDefaults.LastSyncStartTimeBeforeDisruption, syncStartTime);
                }

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                        ErpSyncLavel.Product,
                        ex.Message,
                        ex.StackTrace);

                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                ErpDataSchedulerDefaults.ErpProductSyncTaskName,
                ErpSyncLavel.Product,
                        "Erp Product Sync ended.");

                return false;
            }
        }

        #endregion
    }
}