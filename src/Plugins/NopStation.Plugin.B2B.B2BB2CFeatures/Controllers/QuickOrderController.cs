using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Orders;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Web.Controllers;
using Nop.Web.Factories;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.B2B.B2BB2CFeatures.Factories.QuickOrder;
using NopStation.Plugin.B2B.B2BB2CFeatures.Infrastructure;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderItems;
using NopStation.Plugin.B2B.B2BB2CFeatures.Model.QuickOrderModels.QuickOrderTemplates;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services.QuickOrderServices;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Controllers;

public class QuickOrderController : BasePublicController
{
    #region Fields

    private readonly IQuickOrderTemplateModelFactory _quickOrderTemplateModelFactory;
    private readonly IQuickOrderItemModelFactory _quickOrderItemModelFactory;
    private readonly IQuickOrderTemplateService _quickOrderTemplateService;
    private readonly IQuickOrderItemService _quickOrderItemService;
    private readonly IWorkContext _workContext;
    private readonly IStoreContext _storeContext;
    private readonly INotificationService _notificationService;
    private readonly IProductService _productService;
    private readonly IShoppingCartService _shoppingCartService;
    private readonly ILocalizationService _localizationService;
    private readonly IPermissionService _permissionService;
    private readonly ILogger _logger;
    private readonly IProductModelFactory _productModelFactory;
    private readonly IProductAttributeParser _productAttributeParser;
    private readonly IErpActivityLogsService _erpActivityLogsService;

    #endregion

    #region Ctor

    public QuickOrderController(IQuickOrderTemplateModelFactory quickOrderTemplateModelFactory,
        IQuickOrderItemModelFactory quickOrderItemModelFactory,
        IQuickOrderTemplateService quickOrderTemplateService,
        IQuickOrderItemService quickOrderItemService,
        IWorkContext workContext,
        IStoreContext storeContext,
        INotificationService notificationService,
        IProductService productService,
        IShoppingCartService shoppingCartService,
        ILocalizationService localizationService,
        IPermissionService permissionService,
        ILogger logger,
        IProductModelFactory productModelFactory,
        IProductAttributeParser productAttributeParser,
        IErpActivityLogsService erpActivityLogsService)
    {
        _quickOrderTemplateModelFactory = quickOrderTemplateModelFactory;
        _quickOrderItemModelFactory = quickOrderItemModelFactory;
        _quickOrderTemplateService = quickOrderTemplateService;
        _quickOrderItemService = quickOrderItemService;
        _workContext = workContext;
        _storeContext = storeContext;
        _notificationService = notificationService;
        _productService = productService;
        _shoppingCartService = shoppingCartService;
        _localizationService = localizationService;
        _permissionService = permissionService;
        _logger = logger;
        _productModelFactory = productModelFactory;
        _productAttributeParser = productAttributeParser;
        _erpActivityLogsService = erpActivityLogsService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> QuickOrderTemplateList()
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        //prepare model
        var model = await _quickOrderTemplateModelFactory.PrepareQuickOrderTemplateSearchModelAsync(new QuickOrderTemplateSearchModel());

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> LoadQuickOrderTemplateList(QuickOrderTemplateSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer != null)
            searchModel.SearchCustomerId = customer.Id;

        //prepare model
        var model = await _quickOrderTemplateModelFactory.PrepareQuickOrderTemplateListModelAsync(searchModel);

        return Json(model);
    }

    public async Task<IActionResult> CreateQuickOrder()
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        //prepare model
        var model = await _quickOrderTemplateModelFactory.PrepareQuickOrderTemplateModelAsync(new QuickOrderTemplateModel(), null);
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> CreateQuickOrder(QuickOrderTemplateModel model)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        if (string.IsNullOrEmpty(model.Name?.Trim()))
        {
            ModelState.AddModelError("Name", await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.Name.Required"));
        }

        if (ModelState.IsValid)
        {
            var customer = await _workContext.GetCurrentCustomerAsync();
            //add new quick order
            var quickOrderTemplate = new QuickOrderTemplate
            {
                Name = model.Name,
                CustomerId = customer.Id,
                CreatedOnUtc = DateTime.UtcNow
            };

            await _quickOrderTemplateService.InsertQuickOrderTemplateAsync(quickOrderTemplate);

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_CreateQuickOrder",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.QuickOrderCreated"),
                quickOrderTemplate.Id, customer.Id),
                quickOrderTemplate);

            return RedirectToAction("QuickOrderTemplateDetails", new { id = quickOrderTemplate.Id });
        }

        //prepare model
        model = await _quickOrderTemplateModelFactory.PrepareQuickOrderTemplateModelAsync(model, null);
        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public async Task<IActionResult> QuickOrderTemplateDetails(int id)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        var quickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(id);
        if (quickOrderTemplate == null)
        {
            return RedirectToAction("QuickOrderTemplateList");
        }

        //prepare model
        var model = await _quickOrderTemplateModelFactory.PrepareQuickOrderTemplateModelAsync(new QuickOrderTemplateModel(), quickOrderTemplate);

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> QuickOrderTemplateDetails(QuickOrderTemplateModel model)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        var quickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(model.Id);
        if (quickOrderTemplate != null)
        {
            if (!string.IsNullOrEmpty(model.Name?.Trim()))
            {
                quickOrderTemplate.Name = model.Name.Trim();
                quickOrderTemplate.EditedOnUtc = DateTime.UtcNow;
                await _quickOrderTemplateService.UpdateQuickOrderTemplateAsync(quickOrderTemplate);
            }

            return RedirectToAction("QuickOrderTemplateDetails", new { id = model.Id });
        }
        else
        {
            return RedirectToAction("QuickOrderTemplateList");
        }
    }

    [HttpPost]
    public async Task<IActionResult> QuickOrderDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        var quickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(id);
        if (quickOrderTemplate == null)
        {
            return RedirectToAction("QuickOrderTemplateList");
        }

        //delete quick order
        await _quickOrderTemplateService.DeleteQuickOrderTemplateAsync(quickOrderTemplate);

        //erp activity log
        await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteQuickOrder",
            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.QuickOrderDeleted"),
            quickOrderTemplate.Id, (await _workContext.GetCurrentCustomerAsync()).Id),
            quickOrderTemplate);

        return new NullJsonResult();
    }

    public async Task<IActionResult> QuickOrderAddToCart(int templateId)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        try
        {
            var quickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(templateId);
            if (quickOrderTemplate != null)
            {
                var response = await _quickOrderItemModelFactory.AddToCartAllItemByTemplateAsync(quickOrderTemplate);
                quickOrderTemplate.LastOrderDate = DateTime.UtcNow;
                await _quickOrderTemplateService.UpdateQuickOrderTemplateAsync(quickOrderTemplate);

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_QuickOrderToOrderConvert",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.QuickOrderToOrderConvert"),
                    quickOrderTemplate.Id, (await _workContext.GetCurrentCustomerAsync()).Id),
                    quickOrderTemplate);

                _notificationService.SuccessNotification(response);
            }
            else
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.AddToCartError"));
            }
        }
        catch (Exception ex)
        {
            _logger.Error(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.AddToCartError") + " " + ex.Message, ex);
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.AddToCartError"));
        }

        return RedirectToRoute("ShoppingCart");
    }

    [HttpPost]
    public async Task<IActionResult> CreateUsingCart(string templateName)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        if (string.IsNullOrEmpty(templateName?.Trim()))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.CreateUsingCart.NameError"));
            return RedirectToRoute("ShoppingCart");
        }

        var customer = await _workContext.GetCurrentCustomerAsync();
        //add new quick order
        var quickOrderTemplate = new QuickOrderTemplate
        {
            Name = templateName.Trim(),
            CustomerId = customer.Id,
            CreatedOnUtc = DateTime.UtcNow,
            // EditedOnUtc = DateTime.UtcNow
        };

        await _quickOrderTemplateService.InsertQuickOrderTemplateAsync(quickOrderTemplate);

        var result = await _quickOrderItemModelFactory.CreateQuickOrderItemsFromShoppingCartAsync(quickOrderTemplate.Id);

        if (result)
        {
            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_CreateQuickOrder",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.QuickOrderCreated"),
                quickOrderTemplate.Id, customer.Id),
                quickOrderTemplate);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.CreatedUsingCart"));
        }
        else
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.CreateUsingCartError"));
        }

        return RedirectToRoute("ShoppingCart");
    }

    [HttpPost]
    public async Task<IActionResult> CreateUsingOrder(string templateName, int orderId)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        if (string.IsNullOrEmpty(templateName?.Trim()))
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.CreateUsingCart.NameError"));
            return RedirectToRoute("ShoppingCart");
        }
        var customer = await _workContext.GetCurrentCustomerAsync();
        //add new quick order
        var quickOrderTemplate = new QuickOrderTemplate
        {
            Name = templateName.Trim(),
            CustomerId = customer.Id,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _quickOrderTemplateService.InsertQuickOrderTemplateAsync(quickOrderTemplate);

        var result = await _quickOrderItemModelFactory.CreateQuickOrderItemsFromOrderAsync(quickOrderTemplate.Id, orderId);

        if (result)
        {
            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_CreateQuickOrder",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.QuickOrderCreated"),
                quickOrderTemplate.Id, customer.Id),
                quickOrderTemplate);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.CreatedUsingCart"));
        }
        else
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.CreateUsingCartError"));
        }

        return RedirectToRoute("CheckoutCompleted", new { orderId = orderId });
    }

    #region Order Item

    [HttpPost]
    public async Task<IActionResult> LoadQuickOrderItemList(QuickOrderItemSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        //prepare model
        var model = await _quickOrderItemModelFactory.PrepareQuickOrderItemListModelAsync(searchModel);

        return Json(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ProductDetails_AttributeChange(int templateId, int productId, int quantity, bool validateAttributeConditions, 
        bool loadPicture, IFormCollection form)
    {
        var product = await _productService.GetProductByIdAsync(productId);
        if (product == null)
        {
            return Json(new { Result = false, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.ProductNotFoundError") });
        }

        var errors = new List<string>();
        var attributeXml = await _productAttributeParser.ParseProductAttributesAsync(product, form, errors);

        if (quantity < 1)
        {
            return Json(new { Result = false, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.QuantityError") });
        }

        var quickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(templateId);
        if (quickOrderTemplate == null)
        {
            return RedirectToAction("QuickOrderTemplateList");
        }

        var warnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(await _workContext.GetCurrentCustomerAsync(), ShoppingCartType.Wishlist, product,
                                (await _storeContext.GetCurrentStoreAsync()).Id, attributeXml, decimal.Zero, quantity: quantity, addRequiredProducts: false);
        // render cart details here 
        if (warnings.Any())
        {
            return Json(new { Result = false, Msg = string.Join(",", warnings.ToList()) });
        }

        //check if any quick Order Item exist or not with this sku for this template
        var quickOrderItem = await _quickOrderItemService.GetQuickOrderItemByTemplateIdAndSkuAsync(templateId, product.Sku, attributeXml);
        if (quickOrderItem != null)
        {
            // item already exist
            quickOrderItem.Quantity += quantity;
            quickOrderItem.QuickOrderTemplate = quickOrderTemplate;
            await _quickOrderItemService.UpdateQuickOrderItemAsync(quickOrderItem);

            return Json(new { Result = true, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.ItemUpdatedSuccessfully") });
        }

        quickOrderItem = new QuickOrderItem
        {
            QuickOrderTemplateId = templateId,
            ProductSku = product.Sku,
            Quantity = quantity,
            AttributesXml = attributeXml
        };

        await _quickOrderItemService.InsertQuickOrderItemAsync(quickOrderItem);

        return Json(new { Result = true, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.ItemAddedSuccessfully") });
    }

    [HttpPost]
    public async Task<IActionResult> QuickOrderItemCreate(QuickOrderItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        if (model.ProductSku != null)
        {
            model.ProductSku = model.ProductSku.Trim();
        }
        else
        {
            return Json(new { Result = false, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.ProductSkuEmptyError") });
        }

        if (model.Quantity < 1)
        {
            return Json(new { Result = false, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.QuantityError") });
        }

        if (ModelState.IsValid)
        {
            var quickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(model.QuickOrderTemplateId);
            if (quickOrderTemplate == null)
            {
                return RedirectToAction("QuickOrderTemplateList");
            }

            var product = await _productService.GetProductBySkuAsync(model.ProductSku);
            if (product == null)
            {
                return Json(new { Result = false, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.ProductNotFoundError") });
            }

            var warnings = await _shoppingCartService.GetShoppingCartItemWarningsAsync(
                await _workContext.GetCurrentCustomerAsync(), 
                ShoppingCartType.Wishlist, 
                product,
                (await _storeContext.GetCurrentStoreAsync()).Id, 
                null, 
                decimal.Zero, 
                quantity: model.Quantity, 
                addRequiredProducts: false);
            
            if (warnings.Any())
            {
                var productDeatils = await _productModelFactory.PrepareProductDetailsModelAsync(product);
                model.ProductDetailsModel = productDeatils;
                var partialView = await RenderPartialViewToStringAsync("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Views/QuickOrder/_ProductTemplate.Simple.cshtml", model);
                return Json(new { Result = false, Msg = string.Join(",", warnings.ToList()), ProductDetailsModel = partialView });
            }

            //check if any quick Order Item exist or not with this sku for this template
            var quickOrderItem = await _quickOrderItemService.GetQuickOrderItemByTemplateIdAndSkuAsync(model.QuickOrderTemplateId, model.ProductSku, string.Empty);
            if (quickOrderItem != null)
            {
                quickOrderItem.Quantity += model.Quantity;
                quickOrderItem.QuickOrderTemplate = quickOrderTemplate;
                await _quickOrderItemService.UpdateQuickOrderItemAsync(quickOrderItem);

                return Json(new { Result = true, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.ItemUpdatedSuccessfully") });
            }

            quickOrderItem = new QuickOrderItem
            {
                QuickOrderTemplateId = model.QuickOrderTemplateId,
                ProductSku = model.ProductSku,
                Quantity = model.Quantity,
                AttributesXml = string.Empty
            };

            await _quickOrderItemService.InsertQuickOrderItemAsync(quickOrderItem);

            return Json(new { Result = true, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.ItemAddedSuccessfully") });
        }

        return Json(new { Result = false, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.InvalidData") });
    }

    [HttpPost]
    public async Task<IActionResult> QuickOrderItemUpdate(QuickOrderItemModel model)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        if (model.ProductSku != null)
        {
            model.ProductSku = model.ProductSku.Trim();
        }
        else
        {
            ModelState.AddModelError("ProductSku", (await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.ProductSkuEmptyError")));
        }

        if (model.Quantity < 1)
            ModelState.AddModelError("Quantity", (await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.QuantityError")));

        if (!ModelState.IsValid)
        {
            return ErrorJson(ModelState.SerializeErrors());
        }

        var item = await _quickOrderItemService.GetQuickOrderItemByIdAsync(model.Id);

        if (item != null)
        {
            item.ProductSku = model.ProductSku;
            item.Quantity = model.Quantity;
            item.QuickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(item.QuickOrderTemplateId);
            await _quickOrderItemService.UpdateQuickOrderItemAsync(item);
        }

        return new NullJsonResult();
    }

    [HttpPost]
    public async Task<IActionResult> QuickOrderItemDelete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        //try to get a Quick Order Item with the specified id
        var item = await _quickOrderItemService.GetQuickOrderItemByIdAsync(id)
            ?? throw new ArgumentException("No item found with the specified id", nameof(id));

        await _quickOrderItemService.DeleteQuickOrderItemAsync(item);

        return new NullJsonResult();
    }

    [HttpPost]
    public async Task<IActionResult> QuickOrderItemQuantityUpdate(string actionType, int itemId, int quantity)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        //try to get a Quick Order Item with the specified id
        var item = await _quickOrderItemService.GetQuickOrderItemByIdAsync(itemId)
            ?? throw new ArgumentException("No item found with the specified id", nameof(itemId));

        if (actionType.ToLower().Equals("plus"))
        {
            item.Quantity += 1;
        }
        else if (actionType.ToLower().Equals("minus"))
        {
            if (item.Quantity <= 1)
            {
                return Json(new { Result = false, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.QuantityError") });
            }

            item.Quantity -= 1;
        }
        else if (actionType.ToLower().Equals("update"))
        {
            if (quantity < 1)
            {
                return Json(new { Result = false, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItemCreate.QuantityError") });
            }

            item.Quantity = quantity;
        }

        item.QuickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(item.QuickOrderTemplateId);
        await _quickOrderItemService.UpdateQuickOrderItemAsync(item);       

        return Json(new { Result = true, Msg = await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.QuantityUpdated") });
    }

    [HttpPost]
    public async Task<IActionResult> ImportExcel(int templateId, IFormFile importexcelfile)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
        {
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.ExclImport.AccessDenied"));
            return AccessDeniedView();
        }

        if (importexcelfile != null && importexcelfile.Length > 0 && templateId > 0)
        {
            var warnings = await _quickOrderItemModelFactory.ImportQuickOrderItemsFromXlsxAsync(templateId, importexcelfile.OpenReadStream());

            if (!warnings.Any())
            {
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.Imported"));
                return RedirectToAction("QuickOrderTemplateDetails", new { id = templateId });
            }
            else
            {
                _notificationService.ErrorNotification(warnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"));
                return RedirectToAction("QuickOrderTemplateDetails", new { id = templateId });
            }
        }
        else
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.Import.UploadFile"));
            return RedirectToAction("QuickOrderTemplateDetails", new { id = templateId });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateWithImportExcel(string templateName, IFormFile importexcelfile)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
        {
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.ExclImport.AccessDenied"));
            return AccessDeniedView();
        }

        if (importexcelfile == null || importexcelfile.Length <= 0)
        {
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderList.ImportExcelError"));
            return RedirectToAction("Index");
        }
        var customer = await _workContext.GetCurrentCustomerAsync();
        //add new quick order
        var quickOrderTemplate = new QuickOrderTemplate
        {
            Name = templateName?.Trim() ?? "Capture List Name",
            CustomerId = customer.Id,
            CreatedOnUtc = DateTime.UtcNow
        };

        await _quickOrderTemplateService.InsertQuickOrderTemplateAsync(quickOrderTemplate);

        //erp activity log
        await _erpActivityLogsService.InsertErpActivityAsync("Erp_CreateQuickOrder",
            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.QuickOrderCreated"),
            quickOrderTemplate.Id, customer.Id),
            quickOrderTemplate);

        var warnings = await _quickOrderItemModelFactory.ImportQuickOrderItemsFromXlsxAsync(quickOrderTemplate.Id, importexcelfile.OpenReadStream());

        if (!warnings.Any())
        {
            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("NopStation.B2BB2CFeatures.QuickOrderItem.Imported"));
            return RedirectToAction("QuickOrderTemplateDetails", new { id = quickOrderTemplate.Id });
        }
        else
        {
            _notificationService.ErrorNotification(warnings.Aggregate(string.Empty, (current, next) => $"{current}{next};"), true);
            return RedirectToAction("QuickOrderTemplateDetails", new { id = quickOrderTemplate.Id });
        }
    }

    public virtual async Task<IActionResult> ProductSearchAutoComplete(string term)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        const int searchTermMinimumLength = 3;
        if (string.IsNullOrWhiteSpace(term) || term.Length < searchTermMinimumLength)
            return Content(string.Empty);

        //a vendor should have access only to his products
        var vendorId = 0;
        if ((await _workContext.GetCurrentVendorAsync()) != null)
        {
            vendorId = (await _workContext.GetCurrentVendorAsync()).Id;
        }

        //products
        const int productNumber = 15;
        var products = await _productService.SearchProductsAsync(
            vendorId: vendorId,
            keywords: term,
            searchSku: true,
            pageSize: productNumber,
            showHidden: true);

        var result = 
            (from p in products
                select new
                {
                    label = p.Name + " (" + p.Sku + ")",
                    productid = p.Id,
                    productsku = p.Sku
                }
            ).ToList();
        return Json(result);
    }

    #endregion

    [HttpPost]
    public async Task<IActionResult> UpdateQuickOrderTemplateName(int quickOrderTemplateId, string quickOrderTemplateName)
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        //prepare model
        try
        {
            var quickOrderTemplate = await _quickOrderTemplateService.GetQuickOrderTemplateByIdAsync(quickOrderTemplateId);
            if (quickOrderTemplate != null)
            {
                quickOrderTemplate.Name = quickOrderTemplateName;
                quickOrderTemplate.EditedOnUtc = DateTime.UtcNow;
                await _quickOrderTemplateService.UpdateQuickOrderTemplateAsync(quickOrderTemplate);

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_UpdateQuickOrder",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.QuickOrderUpdated"),
                    quickOrderTemplate.Id, (await _workContext.GetCurrentCustomerAsync()).Id),
                    quickOrderTemplate);
            }
            return Json(new
            {
                success = false
            });
        }
        catch (Exception ex)
        {
            await _notificationService.ErrorNotificationAsync(ex);
            return Json(new
            {
                success = false
            });
        }
    }

    [HttpPost]
    public async Task<IActionResult> ClearCart()
    {
        if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.EnableQuickOrder))
            return AccessDeniedView();

        var customer = await _workContext.GetCurrentCustomerAsync();

        if (customer == null)
            return AccessDeniedView();

        var response = await _quickOrderItemService.ClearCartAsync(customer.Id);

        return Json(new
        {
            success = response
        });
    }

    #endregion
}