using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Vendors;
using Nop.Core.Infrastructure;
using Nop.Services.Catalog;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Discounts;
using Nop.Services.ExportImport;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Media;
using Nop.Services.Messages;
using Nop.Services.Orders;
using Nop.Services.Security;
using Nop.Services.Seo;
using Nop.Services.Shipping;
using Nop.Web.Areas.Admin.Controllers;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class OverridenProductController : ProductController
{
    #region Fields

    private readonly IErpSpecialPriceService _erpSpecialPriceService;
    private readonly IErpGroupPriceService _erpGroupPriceService;

    #endregion

    #region Ctor

    public OverridenProductController(IAclService aclService,
        IBackInStockSubscriptionService backInStockSubscriptionService,
        ICategoryService categoryService,
        ICopyProductService copyProductService,
        ICustomerActivityService customerActivityService,
        ICustomerService customerService,
        IDiscountService discountService,
        IDownloadService downloadService,
        IExportManager exportManager,
        IGenericAttributeService genericAttributeService,
        IHttpClientFactory httpClientFactory,
        IImportManager importManager,
        ILanguageService languageService,
        ILocalizationService localizationService,
        ILocalizedEntityService localizedEntityService,
        IManufacturerService manufacturerService,
        INopFileProvider fileProvider,
        INotificationService notificationService,
        IPdfService pdfService,
        IPermissionService permissionService,
        IPictureService pictureService,
        IProductAttributeFormatter productAttributeFormatter,
        IProductAttributeParser productAttributeParser,
        IProductAttributeService productAttributeService,
        IProductModelFactory productModelFactory,
        IProductService productService,
        IProductTagService productTagService,
        ISettingService settingService,
        IShippingService shippingService,
        IShoppingCartService shoppingCartService,
        ISpecificationAttributeService specificationAttributeService,
        IStoreContext storeContext,
        IUrlRecordService urlRecordService,
        IVideoService videoService,
        IWebHelper webHelper,
        IWorkContext workContext,
        VendorSettings vendorSettings,
        IErpSpecialPriceService erpSpecialPriceService,
        IErpGroupPriceService erpGroupPriceService) : base(aclService,
            backInStockSubscriptionService,
            categoryService,
            copyProductService,
            customerActivityService,
            customerService,
            discountService,
            downloadService,
            exportManager,
            genericAttributeService,
            httpClientFactory,
            importManager,
            languageService,
            localizationService,
            localizedEntityService,
            manufacturerService,
            fileProvider,
            notificationService,
            pdfService,
            permissionService,
            pictureService,
            productAttributeFormatter,
            productAttributeParser,
            productAttributeService,
            productModelFactory,
            productService,
            productTagService,
            settingService,
            shippingService,
            shoppingCartService,
            specificationAttributeService,
            storeContext,
            urlRecordService,
            videoService,
            webHelper,
            workContext,
            vendorSettings)
    {
        _erpSpecialPriceService = erpSpecialPriceService;
        _erpGroupPriceService = erpGroupPriceService;
    }

    #endregion

    #region Methods

    #region Product List / Create / Edit / Delete

    public override async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AccessDeniedView();

        //prepare model
        var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());

        return View(model);
    }

    [HttpPost]
    public override async Task<IActionResult> ProductList(ProductSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _productModelFactory.PrepareProductListModelAsync(searchModel);

        foreach (var item in model.Data)
        {
            var erpProductPricings = await _erpSpecialPriceService.GetErpSpecialPricesByNopProductIdAsync(item.Id);
            var erpgroupPricings = await _erpGroupPriceService.GetErpGroupPriceByProductIdAsync(item.Id);
            item.CustomProperties.Add("specialPriceCount", erpProductPricings.Count.ToString());
            item.CustomProperties.Add("groupPriceCount", erpgroupPricings.Count.ToString());
        }

        return Json(model);
    }

    public override async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageProducts))
            return AccessDeniedView();

        //try to get a product with the specified id
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null || product.Deleted)
            return RedirectToAction("List");

        //a vendor should have access only to his products
        var currentVendor = await _workContext.GetCurrentVendorAsync();
        if (currentVendor != null && product.VendorId != currentVendor.Id)
            return RedirectToAction("List");

        //prepare model
        var model = await _productModelFactory.PrepareProductModelAsync(null, product);

        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/Overriden/Edit.cshtml", model);
    }

    #endregion

    #endregion
}