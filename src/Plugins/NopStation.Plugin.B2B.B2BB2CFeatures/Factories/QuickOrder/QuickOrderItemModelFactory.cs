using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using ClosedXML.Excel;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Localization;
using Nop.Core.Domain.Orders;
using Nop.Core.Events;
using Nop.Data;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Directory;
using Nop.Services.ExportImport;
using Nop.Services.ExportImport.Help;
using Nop.Services.Helpers;
using Nop.Services.Localization;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Web.Factories;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderItems;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpSpecificationAttributeService;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services.QuickOrderServices;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Factories.QuickOrder;

public class QuickOrderItemModelFactory : IQuickOrderItemModelFactory
{
    #region Fields

    private readonly IShoppingCartService _shoppingCartService;
    private readonly IProductService _productService;
    private readonly IQuickOrderItemService _quickOrderItemService;
    private readonly IOrderService _orderService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly ISettingService _settingService;
    private readonly ILocalizationService _localizationService;
    private readonly IPriceFormatter _priceFormatter;
    private readonly IUrlRecordService _urlRecordService;
    private readonly ICurrencyService _currencyService;
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IPermissionService _permissionService;
    private readonly ShoppingCartSettings _shoppingCartSettings;
    private readonly IErpAccountService _erpAccountService;
    private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
    private readonly IErpSpecificationAttributeService _erpSpecificationAttributeService;
    private readonly ILanguageService _languageService;
    private readonly CatalogSettings _catelogSettings;
    private readonly IProductAttributeFormatter _productAttributeFormatter;
    private readonly IQuickOrderTemplateService _quickOrderTemplateService;
    private readonly ICustomerService _customerService;
    private readonly IProductAttributeService _productAttributeService;
    protected readonly IRepository<ShoppingCartItem> _sciRepository;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;

    #endregion

    #region Ctor

    public QuickOrderItemModelFactory(IShoppingCartService shoppingCartService,
        IProductService productService,
        IQuickOrderItemService quickOrderItemService,
        IOrderService orderService,
        IDateTimeHelper dateTimeHelper,
        IWorkContext workContext,
        IStoreContext storeContext,
        IProductModelFactory productModelFactory,
        ISettingService settingService,
        ILocalizationService localizationService,
        IPriceFormatter priceFormatter,
        IPriceCalculationService priceCalculationService,
        IUrlRecordService urlRecordService,
        ICurrencyService currencyService,
        IGenericAttributeService genericAttributeService,
        IQuickOrderTemplateService quickOrderTemplateService,
        ICustomerService customerService,
        IPermissionService permissionService,
        ShoppingCartSettings shoppingCartSettings,
        IEventPublisher eventPublisher,
        IErpAccountService erpAccountService,
        IB2BB2CWorkContext b2BB2CWorkContext,
        IErpSpecialPriceService erpSpecialPriceService,
        IOverriddenOrderProcessingService overriddenOrderProcessingService,
        IErpSpecificationAttributeService erpSpecificationAttributeService,
        IProductAttributeService productAttributeService,
        ISpecificationAttributeService specificationAttributeService,
        ILanguageService languageService,
        IImportManager importManager,
        CatalogSettings catelogSettings,
        IProductAttributeFormatter productAttributeFormatter,
        IRepository<ShoppingCartItem> sciRepository,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings)
    {
        _shoppingCartService = shoppingCartService;
        _productService = productService;
        _quickOrderItemService = quickOrderItemService;
        _orderService = orderService;
        _workContext = workContext;
        _storeContext = storeContext;
        _settingService = settingService;
        _localizationService = localizationService;
        _priceFormatter = priceFormatter;
        _urlRecordService = urlRecordService;
        _currencyService = currencyService;
        _genericAttributeService = genericAttributeService;
        _permissionService = permissionService;
        _shoppingCartSettings = shoppingCartSettings;
        _erpAccountService = erpAccountService;
        _b2BB2CWorkContext = b2BB2CWorkContext;
        _erpSpecificationAttributeService = erpSpecificationAttributeService;
        _productAttributeService = productAttributeService;
        _quickOrderTemplateService = quickOrderTemplateService;
        _customerService = customerService;
        _languageService = languageService;
        _catelogSettings = catelogSettings;
        _productAttributeFormatter = productAttributeFormatter;
        _sciRepository = sciRepository;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
    }

    #endregion

    #region Utilities

    private static ExportedAttributeType GetTypeOfExportedAttribute(IXLWorksheet defaultWorksheet, List<IXLWorksheet> localizedWorksheets, PropertyManager<ImportProductMetadata, Language> productAttributeManager, int iRow)
    {
        productAttributeManager.ReadDefaultFromXlsx(defaultWorksheet, iRow, ExportProductAttribute.ProductAttributeCellOffset);

        foreach (var worksheet in localizedWorksheets)
            productAttributeManager.ReadLocalizedFromXlsx(worksheet, iRow, ExportProductAttribute.ProductAttributeCellOffset);

        return ExportedAttributeType.ProductAttribute;
    }

    protected virtual async Task SetOutLineForProductAttributeRowAsync(object cellValue, IXLWorksheet worksheet, int endRow)
    {
        try
        {
            var aid = Convert.ToInt32(cellValue ?? -1);

            var productAttribute = await _productAttributeService.GetProductAttributeByIdAsync(aid);

            if (productAttribute != null)
                worksheet.Row(endRow).OutlineLevel = 1;
        }
        catch
        {
            if ((cellValue ?? string.Empty).ToString() == "AttributeId")
                worksheet.Row(endRow).OutlineLevel = 1;
        }
    }

    private async Task<(PropertyManager<Product, Language> Manager, IList<PropertyByName<Product, Language>> Properties, PropertyManager<ImportProductMetadata, Language> ProductAttributeManager)> PrepareImportDataAsync(IXLWorkbook workbook, IXLWorksheet worksheet)
    {
        var languages = await _languageService.GetAllLanguagesAsync(showHidden: true);
        //get metadata
        var metadata = ImportManager.GetWorkbookMetadata<Product>(workbook, languages);
        var defaultWorksheet = metadata.DefaultWorksheet;
        //get properties
        var properties = metadata.DefaultProperties;

        var manager = new PropertyManager<Product, Language>(properties, _catelogSettings);

        var productAttributeProperties = new[]
        {
            new PropertyByName<ImportProductMetadata, Language>("AttributeId"),
            new PropertyByName<ImportProductMetadata, Language>("ProductAttributeName"),
            new PropertyByName<ImportProductMetadata, Language>("ProductAttributeValueName"),
            new PropertyByName<ImportProductMetadata, Language>("ProductAttributeValueId")
        };

        var productAttributeLocalizedProperties = new[]
        {
            new PropertyByName<ImportProductMetadata, Language>("DefaultValue"),
            new PropertyByName<ImportProductMetadata, Language>("AttributeTextPrompt"),
            new PropertyByName<ImportProductMetadata, Language>("ValueName")
        };

        var productAttributeManager = new PropertyManager<ImportProductMetadata, Language>(productAttributeProperties, _catelogSettings, productAttributeLocalizedProperties, languages);

        var endRow = 2;

        var allSkuCells = new List<string>();

        var tempProperty = manager.GetDefaultProperty("SKU");
        var skuCellNum = tempProperty?.PropertyOrderPosition ?? -1;

        var allQunatities = new List<string>();
        tempProperty = manager.GetDefaultProperty("Quantity");
        var quantityCellNum = tempProperty?.PropertyOrderPosition ?? -1;

        var allAttributeIds = new List<string>();
        var allAttributeNames = new List<string>();
        var allAttributeValueIds = new List<string>();
        var allAttributeValueNames = new List<string>();

        var attributeIdCellNum = 1 + ExportProductAttribute.ProductAttributeCellOffset;
        var productsInFile = new List<int>();
        var typeOfExportedAttribute = ExportedAttributeType.NotSpecified;

        while (true)
        {
            var allColumnsAreEmpty = metadata.DefaultProperties.Select(property => worksheet.Row(endRow).Cell(property.PropertyOrderPosition))
                .All(cell => string.IsNullOrEmpty(cell?.Value.ToString()));

            if (allColumnsAreEmpty)
                break;

            if (new[] { 1, 2 }.Select(cellNum => defaultWorksheet.Row(endRow).Cell(cellNum))
                .All(cell => string.IsNullOrEmpty(cell?.Value.ToString())) && defaultWorksheet.Row(endRow).OutlineLevel == 0)
            {
                var cellValue = defaultWorksheet.Row(endRow).Cell(attributeIdCellNum).Value;
                await SetOutLineForProductAttributeRowAsync(cellValue, defaultWorksheet, endRow);
            }

            if (defaultWorksheet.Row(endRow).OutlineLevel != 0)
            {
                var newTypeOfExportedAttribute = GetTypeOfExportedAttribute(defaultWorksheet, metadata.LocalizedWorksheets, productAttributeManager, endRow);

                //skip caption row
                if (newTypeOfExportedAttribute != ExportedAttributeType.NotSpecified && newTypeOfExportedAttribute != typeOfExportedAttribute)
                {
                    typeOfExportedAttribute = newTypeOfExportedAttribute;
                    endRow++;
                    continue;
                }

                switch (typeOfExportedAttribute)
                {
                    case ExportedAttributeType.ProductAttribute:
                        productAttributeManager.ReadDefaultFromXlsx(defaultWorksheet, endRow, ExportProductAttribute.ProductAttributeCellOffset);

                        if (int.TryParse((defaultWorksheet.Row(endRow).Cell(attributeIdCellNum).Value).ToString(), out var aid))
                        {
                            allAttributeIds.Add(aid.ToString());
                            var attributeName = (defaultWorksheet.Row(endRow).Cell(attributeIdCellNum + 1).Value).ToString();
                            allAttributeNames.Add(attributeName.ToString());
                            var attributeValueName = (defaultWorksheet.Row(endRow).Cell(attributeIdCellNum + 2).Value).ToString();
                            allAttributeValueNames.Add(attributeValueName.ToString());
                            var attributeValueNameId = (defaultWorksheet.Row(endRow).Cell(attributeIdCellNum + 3).Value).ToString();
                            allAttributeValueIds.Add(attributeValueNameId.ToString());
                        }
                        break;
                }

                endRow++;
                continue;
            }

            if (skuCellNum > 0)
            {
                var skuCellName = worksheet.Row(endRow).Cell(skuCellNum).Value.ToString() ?? string.Empty;

                if (!string.IsNullOrEmpty(skuCellName))
                    allSkuCells.Add(skuCellName);
            }

            if (quantityCellNum > 0)
            {
                var quantityCellName = worksheet.Row(endRow).Cell(quantityCellNum).Value.ToString() ??
                                      string.Empty;
                if (!string.IsNullOrEmpty(quantityCellName))
                    allQunatities.AddRange(quantityCellName
                        .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()));
            }

            productsInFile.Add(endRow);

            endRow++;
        }

        return (manager, properties, productAttributeManager);
    }

    #endregion

    #region Methods

    public async Task<string> GetValidationResultAsync(Product product, int quantity, string attribute)
    {
        if (product == null)
        {
            //no product found
            return "No product found with the specified SKU";
        }

        var warnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, product,
         (await _storeContext.GetCurrentStoreAsync()).Id, attribute, decimal.Zero, quantity: quantity, addRequiredProducts: false);

        if (warnings.Any())
            return warnings.FirstOrDefault();

        return "OK";
    }

    public async Task<QuickOrderItemListModel> PrepareQuickOrderItemListModelAsync(QuickOrderItemSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        // get QuickOrderItems
        var quickOrderItems = await _quickOrderItemService.GetAllQuickOrderItemsPagedAsync(productSku: searchModel.ProductSku, quickOrderTemplateId: searchModel.QuickOrderTemplateId,
            pageIndex: searchModel.Page - 1, pageSize: searchModel.PageSize);

        var store = await _storeContext.GetCurrentStoreAsync();
        var erpCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();

        //customer currency
        var currencyTmp = await _currencyService.GetCurrencyByIdAsync(
          await _genericAttributeService.GetAttributeAsync<int>(erpCustomer, erpCustomer.CustomCustomerAttributesXML, store.Id));
        var customerCurrency = currencyTmp != null && currencyTmp.Published ? currencyTmp : await _workContext.GetWorkingCurrencyAsync();
        var customerCurrencyCode = customerCurrency.CurrencyCode;

        //prepare list model
        var model = await new QuickOrderItemListModel().PrepareToGridAsync(searchModel, quickOrderItems, () =>
        {
            return quickOrderItems.SelectAwait(async quickOrder =>
            {
                var product = await _productService.GetProductBySkuAsync(quickOrder.ProductSku);

                //fill in model values from the entity
                var quickOrderItem = new QuickOrderItemModel
                {
                    Id = quickOrder.Id,
                    ProductSku = quickOrder.ProductSku,
                    Quantity = quickOrder.Quantity,
                    QuickOrderTemplateId = quickOrder.QuickOrderTemplateId
                };

                quickOrderItem.AttributesInfo = await _productAttributeFormatter.FormatAttributesAsync(product, quickOrder.AttributesXml);

                if (product != null)
                {
                    var (finalPrice, _, _) = await _shoppingCartService.GetUnitPriceAsync(product,
                        erpCustomer,
                        store,
                        ShoppingCartType.ShoppingCart, 1, quickOrder.AttributesXml, 0, null, null, true);

                    // set values
                    quickOrderItem.ProductId = product.Id;
                    quickOrderItem.Name = await _localizationService.GetLocalizedAsync(product, x => x.Name);
                    quickOrderItem.SeName = await _urlRecordService.GetSeNameAsync(product);
                    quickOrderItem.StockAvailability = await _productService.FormatStockMessageAsync(product, string.Empty);

                    // Alternate Method Could not find , Taking empty string -1188
                    quickOrderItem.PricingNotes = "";

                    // Change UnitOfMeasureSpecificationAttributeId to PreFilterFacetSpecificationAttributeId -- need to check the relativity
                    quickOrderItem.UOM = await _erpSpecificationAttributeService.GetProductUOMByProductIdAndSpecificationAttributeId(product.Id,
                        _b2BB2CFeaturesSettings.UnitOfMeasureSpecificationAttributeId) ?? string.Empty;

                    var language = await _workContext.GetWorkingLanguageAsync();

                    quickOrderItem.Price = await _priceFormatter.FormatPriceAsync(finalPrice, true, customerCurrencyCode, language.Id, true);
                    quickOrderItem.PriceValue = finalPrice;
                }

                if (searchModel.Validate)
                    quickOrderItem.ValidationResult = await GetValidationResultAsync(product, quickOrder.Quantity, quickOrder.AttributesXml);

                return quickOrderItem;
            });
        });

        return model;
    }

    public async Task<QuickOrderItemModel> PrepareQuickOrderItemModelAsync(QuickOrderItemModel model, QuickOrderItem quickOrderItem)
    {
        if (quickOrderItem != null)
        {
            model = model ?? new QuickOrderItemModel();
            model.Id = quickOrderItem.Id;
            model.ProductSku = quickOrderItem.ProductSku;
            model.Quantity = quickOrderItem.Quantity;
            model.QuickOrderTemplateId = quickOrderItem.QuickOrderTemplateId;
        }

        return model;
    }

    public async Task<QuickOrderItemSearchModel> PrepareQuickOrderItemSearchModelAsync(QuickOrderItemSearchModel searchModel)
    {
        if (searchModel == null)
            throw new ArgumentNullException(nameof(searchModel));

        // set always validate
        searchModel.Validate = true;

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<IList<string>> ImportQuickOrderItemsFromXlsxAsync(int templateId, Stream stream)
    {
        var warnings = new List<string>();
        try
        {
            var quickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(templateId);
            if (quickOrderTemplate == null)
            {
                warnings.Add(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderTemplate.TemplateNotFound"));
                return warnings;
            }

            using var workbook = new XLWorkbook(stream);

            var worksheet = workbook.Worksheets.FirstOrDefault();
            if (worksheet == null)
                throw new NopException("No worksheet found");

            var metadata = await PrepareImportDataAsync(workbook, worksheet);

            var iRow = 2;

            var languages = await _languageService.GetAllLanguagesAsync();
            var metadatas = ImportManager.GetWorkbookMetadata<Product>(workbook, languages);
            var defaultWorksheets = metadatas.DefaultWorksheet;

            var attributeIdCellNum = 1 + ExportProductAttribute.ProductAttributeCellOffset;

            while (true)
            {
                var allColumnsAreEmpty = metadata.Manager.GetDefaultProperties
                    .Select(property => worksheet.Row(iRow).Cell(property.PropertyOrderPosition))
                    .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                if (allColumnsAreEmpty)
                    break;

                if (new[] { 1, 2 }.Select(cellNum => defaultWorksheets.Row(iRow).Cell(cellNum))
                .All(cell => string.IsNullOrEmpty(cell?.Value.ToString())) &&
                     defaultWorksheets.Row(iRow).OutlineLevel == 0)
                {
                    var cellValue = defaultWorksheets.Row(iRow).Cell(attributeIdCellNum).Value;
                    await SetOutLineForProductAttributeRowAsync(cellValue, defaultWorksheets, iRow);

                }

                metadata.Manager.ReadDefaultFromXlsx(worksheet, iRow);

                var quickOrderItem = new QuickOrderItem();

                foreach (var property in metadata.Manager.GetDefaultProperties)
                {
                    switch (property.PropertyName)
                    {
                        case "SKU":
                            quickOrderItem.ProductSku = property.StringValue;
                            break;
                        case "Sku":
                            quickOrderItem.ProductSku = property.StringValue;
                            break;
                        case "QUANTITY":
                            quickOrderItem.Quantity = property.IntValue;
                            break;
                        case "Quantity":
                            quickOrderItem.Quantity = property.IntValue;
                            break;

                    }
                }
                var attrbutesIds = new List<int>();
                var attrbuteValuesIds = new List<int>();


                var checkIfAttributeRowExists = metadata.ProductAttributeManager.GetDefaultProperties.Where(property => property.PropertyValue != null).Any();
                if (checkIfAttributeRowExists)
                {
                    var innerRow = iRow + 2;
                    while (true)
                    {
                        checkIfAttributeRowExists = metadata.ProductAttributeManager.GetDefaultProperties.Where(property => property.PropertyValue != null).Any();

                        if (!checkIfAttributeRowExists)
                            break;

                        var allAttrbuteColumnsAreEmpty = metadata.ProductAttributeManager.GetDefaultProperties
                           .Select(property => worksheet.Row(innerRow).Cell(property.PropertyOrderPosition))
                           .All(cell => cell?.Value == null || string.IsNullOrEmpty(cell.Value.ToString()));

                        if (allColumnsAreEmpty)
                            break;

                        if (allAttrbuteColumnsAreEmpty)
                            break;

                        if (new[] { 1, 2 }.Select(cellNum => defaultWorksheets.Row(innerRow).Cell(cellNum))
                            .All(cell => string.IsNullOrEmpty(cell?.Value.ToString())) &&
                       defaultWorksheets.Row(innerRow).OutlineLevel == 0)
                        {
                            break;
                        }

                        metadata.ProductAttributeManager.ReadDefaultFromXlsx(worksheet, innerRow, 2);

                        foreach (var property in metadata.ProductAttributeManager.GetDefaultProperties)
                        {
                            if (property.PropertyName.Equals("AttributeId"))
                            {
                                var attributeId = 0;
                                if (!string.IsNullOrEmpty(property.PropertyValue.ToString()))
                                    attributeId = int.Parse(property.PropertyValue.ToString());
                                var attribute = await _productAttributeService.GetProductAttributeByIdAsync(attributeId);
                                if (attribute != null)
                                {
                                    var productAttributeMapping = await _quickOrderItemService.GetProductAttributeMapping(attribute.Id);
                                    if (productAttributeMapping != null)
                                    {
                                        attrbutesIds.Add(productAttributeMapping.Id);
                                    }
                                }
                            }

                            if (property.PropertyName.Equals("ProductAttributeName"))
                            {
                                var attributeName = property.PropertyValue.ToString();
                                var attributeByName = await _quickOrderItemService.GetProductAttributeByName(attributeName);
                                if (attributeByName != null)
                                {
                                    var productAttributeMapping = await _quickOrderItemService.GetProductAttributeMapping(attributeByName.Id);
                                    if (productAttributeMapping != null)
                                    {
                                        attrbutesIds.Add(productAttributeMapping.Id);
                                    }
                                }
                            }

                            if (property.PropertyName.Equals("ProductAttributeValueId"))
                            {
                                var attributeValueId = 0;
                                if (!string.IsNullOrEmpty(property.PropertyValue.ToString()))
                                    attributeValueId = int.Parse(property.PropertyValue.ToString());
                                var attributeValue = await _productAttributeService.GetProductAttributeValueByIdAsync(attributeValueId);
                                if (attributeValue != null)
                                {
                                    attrbuteValuesIds.Add(attributeValueId);
                                }
                            }

                            if (property.PropertyName.Equals("ProductAttributeValueName"))
                            {
                                var attributeValueName = property.PropertyValue.ToString();
                                var attributeValueByName = await _quickOrderItemService.GetAttributeValueByNameAsync(attributeValueName);
                                if (attributeValueByName != null)
                                {
                                    attrbuteValuesIds.Add(attributeValueByName.Id);
                                }
                            }
                        }

                        innerRow++;
                    }

                    if (iRow == innerRow)
                        iRow++;
                    else
                        iRow = innerRow;
                }
                else
                {
                    iRow++;
                }

                var xmlDocumentToString = string.Empty;

                try
                {
                    if (attrbutesIds.Count > 0 && attrbuteValuesIds.Count > 0)
                    {
                        var xmlDocument = new XDocument(
                             new XElement("Attributes",
                                 from index in Enumerable.Range(0, attrbutesIds.Count)
                                 select new XElement("ProductAttribute",
                                     new XAttribute("ID", attrbutesIds[index]),
                                     new XElement("ProductAttributeValue",
                                         new XElement("Value", attrbuteValuesIds[index])
                                        )
                                    )
                                )
                            );

                        xmlDocumentToString = xmlDocument.ToString();
                    }

                    var product = await _productService.GetProductBySkuAsync(quickOrderItem.ProductSku);
                    var shoppingCartType = ShoppingCartType.ShoppingCart;
                    var customer = await _workContext.GetCurrentCustomerAsync();
                    var store = await _storeContext.GetCurrentStoreAsync();
                    var attributesXml = xmlDocumentToString;
                    decimal customerEnteredPrice = decimal.Zero;

                    if (product != null)
                    {
                        var warning = await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer: customer, shoppingCartType: shoppingCartType, product: product,
                           storeId: store.Id, attributesXml: attributesXml, customerEnteredPrice,
                           quantity: quickOrderItem.Quantity);

                        if (warning.Any())
                        {
                            warning.Add(String.Concat("For Product's Sku ", product.Sku));
                            warnings.AddRange(warning);

                        }

                        if (!warning.Any())
                        {
                            var template = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(templateId);

                            quickOrderItem.AttributesXml = attributesXml;
                            quickOrderItem.QuickOrderTemplateId = templateId;

                            var alreadyExistsQuickOrderItem = (await _quickOrderItemService
                                .GetQuickOrderItemByTemplateIdAndSkuAsync(templateId, quickOrderItem.ProductSku, attributesXml)) ?? new QuickOrderItem();

                            alreadyExistsQuickOrderItem.QuickOrderTemplate = template;

                            if (alreadyExistsQuickOrderItem.Id == 0)
                            {
                                await _quickOrderItemService.InsertQuickOrderItemAsync(quickOrderItem);
                            }
                            else
                            {
                                alreadyExistsQuickOrderItem.Quantity += quickOrderItem.Quantity;
                                await _quickOrderItemService.UpdateQuickOrderItemAsync(alreadyExistsQuickOrderItem);
                            }

                            template.EditedOnUtc = DateTime.UtcNow;
                            template.LastPriceCalculatedOnUtc = DateTime.UtcNow;
                            await _quickOrderTemplateService.UpdateQuickOrderTemplateAsync(template);
                        }
                    }
                }
                catch (Exception e)
                {
                    warnings.Add(e.Message);
                }

            }
        }
        catch (Exception ex)
        {
            warnings.Add(ex.Message);
        }

        return warnings;
    }

    public async Task<bool> CreateQuickOrderItemsFromShoppingCartAsync(int templateId)
    {
        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer != null && !customer.HasShoppingCartItems)
        {
            return false;
        }

        var cartItems = await _shoppingCartService.GetShoppingCartAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);

        if (cartItems.Count == 0)
        {
            return false;
        }

        foreach (var cartItem in cartItems)
        {
            var product = await _productService.GetProductByIdAsync(cartItem.ProductId);
            var sku = product?.Sku;
            if (string.IsNullOrEmpty(sku))
                continue;

            var quickOrderItem = new QuickOrderItem
            {
                QuickOrderTemplateId = templateId,
                ProductSku = product.Sku,
                Quantity = cartItem.Quantity,
                AttributesXml = cartItem.AttributesXml ?? string.Empty,
            };

            quickOrderItem.QuickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(templateId);

            await _quickOrderItemService.InsertQuickOrderItemAsync(quickOrderItem);
        }

        return true;
    }

    public async Task<bool> CreateQuickOrderItemsFromOrderAsync(int templateId, int orderId)
    {
        var orderItems = await _orderService.GetOrderItemsAsync(orderId);

        if (orderItems.Count == 0)
        {
            return false;
        }

        foreach (var orderItem in orderItems)
        {
            var product = await _productService.GetProductByIdAsync(orderItem.ProductId);
            var sku = product?.Sku;
            if (string.IsNullOrEmpty(sku))
                continue;

            var quickOrderItem = new QuickOrderItem
            {
                QuickOrderTemplateId = templateId,
                ProductSku = product.Sku,
                Quantity = orderItem.Quantity,
                AttributesXml = orderItem.AttributesXml ?? string.Empty,
            };

            await _quickOrderItemService.InsertQuickOrderItemAsync(quickOrderItem);
        }

        return true;
    }

    public async Task<string> AddToCartAllItemByTemplateAsync(QuickOrderTemplate quickOrderTemplate)
    {
        int added = 0, failed = 0;
        var customer = await _workContext.GetCurrentCustomerAsync();
        var storeId = (await _storeContext.GetCurrentStoreAsync()).Id;
        await _customerService.ResetCheckoutDataAsync(customer, storeId);

        var cart = await _shoppingCartService.GetShoppingCartAsync(customer, ShoppingCartType.ShoppingCart, (await _storeContext.GetCurrentStoreAsync()).Id);
        var quickOrderItems = await _quickOrderItemService.GetAllQuickOrderItemsPagedAsync(quickOrderTemplateId: quickOrderTemplate.Id);

        var skus = quickOrderItems.Select(x => x.ProductSku).ToArray();

        var products = await _productService.GetProductsBySkuAsync(skus);

        foreach (var item in quickOrderItems)
        {
            var product = products.FirstOrDefault(p => p.Sku == item.ProductSku);
            if (product != null && item.Quantity > 0)
            {
                var addToCartWarnings = await AddToCartQuickOrderTempleteAsync(customer, product, ShoppingCartType.ShoppingCart,
                    (List<ShoppingCartItem>)cart, (await _storeContext.GetCurrentStoreAsync()).Id, attributesXml: item.AttributesXml,
                    quantity: item.Quantity, addRequiredProducts: false);

                if (addToCartWarnings.Any())
                {
                    failed++;
                }
                else
                {
                    added++;
                }
            }
            else
            {
                failed++;
            }
        }

        customer.HasShoppingCartItems = cart.Any() || added > 0;

        await _customerService.UpdateCustomerAsync(customer);

        return string.Format(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.QuickOrderTemplate.AddToCartResult"), quickOrderItems.Count, added, failed);
    }

    private async Task<bool> ValidateDataAndTypeAsync(string sku, string quantity)
    {
        var value = true;
        try
        {
            if (string.IsNullOrEmpty(sku) || string.IsNullOrEmpty(quantity))
                return false;

            var itemQuantity = Convert.ToInt32(quantity);
            if (itemQuantity == 0)
                return false;

            var product = await _productService.GetProductBySkuAsync(sku);
            if (product == null)
                return false;
        }
        catch
        {
            value = false;
        }

        return value;
    }

    public virtual async Task<IList<string>> AddToCartQuickOrderTempleteAsync(Customer customer, Product product,
        ShoppingCartType shoppingCartType, List<ShoppingCartItem> cart, int storeId, string attributesXml = null,
        decimal customerEnteredPrice = decimal.Zero,
        DateTime? rentalStartDate = null, DateTime? rentalEndDate = null,
        int quantity = 1, bool addRequiredProducts = true)
    {
        if (customer == null)
            throw new ArgumentNullException(nameof(customer));

        if (product == null)
            throw new ArgumentNullException(nameof(product));

        var warnings = new List<string>();
        if (shoppingCartType == ShoppingCartType.ShoppingCart && !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableShoppingCart, customer))
        {
            warnings.Add(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.QuickOrderTemplate.Warning.ShoppingCartIsDisabled"));
            return warnings;
        }

        if (shoppingCartType == ShoppingCartType.Wishlist && !await _permissionService.AuthorizeAsync(StandardPermissionProvider.EnableWishlist, customer))
        {
            warnings.Add(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.QuickOrderTemplate.Warning.WishlistIsDisabled"));
            return warnings;
        }

        if (customer.IsSearchEngineAccount())
        {
            warnings.Add(await _localizationService.GetResourceAsync("NopStation.Plugin.B2B.B2BB2CFeatures.QuickOrderTemplate.Warning.SearchEngineCannotAddToCart"));
            return warnings;
        }

        if (quantity <= 0)
        {
            warnings.Add(await _localizationService.GetResourceAsync("ShoppingCart.QuantityShouldPositive"));
            return warnings;
        }

        var shoppingCartItem = await _shoppingCartService.FindShoppingCartItemInTheCartAsync(cart,
            shoppingCartType, product, attributesXml, customerEnteredPrice,
            rentalStartDate, rentalEndDate);

        if (shoppingCartItem != null)
        {
            //update existing shopping cart item
            var newQuantity = shoppingCartItem.Quantity + quantity;
            warnings.AddRange(await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product,
                storeId, attributesXml,
                customerEnteredPrice, rentalStartDate, rentalEndDate,
                newQuantity, addRequiredProducts, shoppingCartItem.Id));

            if (warnings.Count != 0)
                return warnings;

            shoppingCartItem.AttributesXml = attributesXml;
            shoppingCartItem.Quantity = newQuantity;
            shoppingCartItem.UpdatedOnUtc = DateTime.UtcNow;

            //event notification
            await _shoppingCartService.UpdateShoppingCartItemAsync(customer, shoppingCartItem.Id, shoppingCartItem.AttributesXml, shoppingCartItem.CustomerEnteredPrice, rentalStartDate, rentalEndDate, shoppingCartItem.Quantity);
        }
        else
        {
            //new shopping cart item
            warnings.AddRange(await _shoppingCartService.GetShoppingCartItemWarningsAsync(customer, shoppingCartType, product,
                storeId, attributesXml, customerEnteredPrice,
                rentalStartDate, rentalEndDate,
                quantity, addRequiredProducts));

            if (warnings.Any())
                return warnings;

            //maximum items validation
            switch (shoppingCartType)
            {
                case ShoppingCartType.ShoppingCart:
                    if (cart.Count >= _shoppingCartSettings.MaximumShoppingCartItems)
                    {
                        warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumShoppingCartItems"), _shoppingCartSettings.MaximumShoppingCartItems));
                        return warnings;
                    }
                    break;
                case ShoppingCartType.Wishlist:
                    if (cart.Count >= _shoppingCartSettings.MaximumWishlistItems)
                    {
                        warnings.Add(string.Format(await _localizationService.GetResourceAsync("ShoppingCart.MaximumWishlistItems"), _shoppingCartSettings.MaximumWishlistItems));
                        return warnings;
                    }
                    break;
                default:
                    break;
            }

            await _shoppingCartService.AddToCartAsync(customer, product, shoppingCartType, storeId, attributesXml, customerEnteredPrice, rentalStartDate, rentalEndDate, quantity, addRequiredProducts);
        }

        return warnings;
    }

    #endregion
}