using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Services.Catalog;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Factories;
using Nop.Web.Areas.Admin.Models.Catalog;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers
{
    public class ErpProductPricingController : NopStationAdminController
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly IProductAttributeService _productAttributeService;
        private readonly IPermissionService _permissionService;
        private readonly IProductModelFactory _productModelFactory;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IWorkContext _workContext;
        private readonly IErpPriceGroupProductPricingModelFactory _erpPriceGroupProductPricingModelFactory;
        private readonly IErpGroupPriceService _erpGroupPriceService;
        private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
        private readonly IErpSpecialPriceService _erpSpecialPriceService;
        private readonly IErpSpecialPriceModelFactory _erpSpecialPriceModelFactory;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
        private readonly IErpLogsService _erpLogsService;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
        private readonly IErpActivityLogsService _erpActivityLogsService;

        #endregion

        #region Ctor

        public ErpProductPricingController(IProductService productService,
            IProductAttributeService productAttributeService,
            IPermissionService permissionService,
            IProductModelFactory productModelFactory,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService,
            IWorkContext workContext,
            IErpPriceGroupProductPricingModelFactory erpPriceGroupProductPricingModelFactory,
            IErpGroupPriceService erpGroupPriceService,
            IErpGroupPriceCodeService erpGroupPriceCodeService,
            IErpSpecialPriceService erpSpecialPriceService,
            IErpSpecialPriceModelFactory erpSpecialPriceModelFactory,
            IB2BB2CWorkContext b2BB2CWorkContext,
            IErpLogsService erpLogsService,
            IStaticCacheManager staticCacheManager,
            B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
            IErpActivityLogsService erpActivityLogsService)
        {
            _productService = productService;
            _productAttributeService = productAttributeService;
            _permissionService = permissionService;
            _productModelFactory = productModelFactory;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
            _workContext = workContext;
            _erpPriceGroupProductPricingModelFactory = erpPriceGroupProductPricingModelFactory;
            _erpGroupPriceService = erpGroupPriceService;
            _erpGroupPriceCodeService = erpGroupPriceCodeService;
            _erpSpecialPriceService = erpSpecialPriceService;
            _erpSpecialPriceModelFactory = erpSpecialPriceModelFactory;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _erpLogsService = erpLogsService;
            _staticCacheManager = staticCacheManager;
            _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
            _erpActivityLogsService = erpActivityLogsService;
        }

        #endregion

        #region Methods

        [HttpPost, ActionName("List")]
        [FormValueRequired("go-to-product-by-sku")]
        public async Task<IActionResult> GoToSku(ProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var product = await _productService.GetProductBySkuAsync(searchModel.GoDirectlyToSku);
            if (product == null)
            {
                var productCom = await _productAttributeService.GetProductAttributeCombinationBySkuAsync(searchModel.GoDirectlyToSku);
                if (productCom != null)
                {
                    var productId = productCom.ProductId;
                    product = await _productService.GetProductByIdAsync(productId);
                }
            }

            if (product == null)
                return RedirectToAction("AllProductList");

            return RedirectToAction("ProductPricingByProduct", "ErpProductPricing", new { id = product.Id });
        }

        public async Task<IActionResult> AllProductList()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var model = await _productModelFactory.PrepareProductSearchModelAsync(new ProductSearchModel());
            return View(model);
        }

        public async Task<IActionResult> ProductPricingByProduct(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
                return RedirectToAction("AllProductList");

            var lang = await _workContext.GetWorkingLanguageAsync();
            var model = new ErpProductPricingModel
            {
                ProductId = id,
                ProductName = await _localizationService.GetLocalizedAsync(product, x => x.Name, lang.Id),
                ProductSku = product.Sku,
                ProductPrice = product.Price
            };

            //Preparing Special Price search model
            await _erpSpecialPriceModelFactory.PrepareErpProductPricingSearchModel(model.ErpSpecialPriceSearchModel, id);

            //Preparing Group Price search model
            await _erpPriceGroupProductPricingModelFactory.PrepareErpProductPricingSearchModel(model.ErpPriceGroupProductPricingSearchModel, id);

            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/ErpProductPricing/ProductPricing.cshtml", model);
        }

        #region Special Price Product Pricing

        public async Task<IActionResult> ProductPricingList(ErpSpecialPriceSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var model = await _erpSpecialPriceModelFactory.PrepareErpProductPricingListModel(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> SpecialPriceCreatePopUp(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //prepare model
            var model = await _erpSpecialPriceModelFactory.PrepareErpProductPricingModel(new ErpSpecialPriceModel(), null);
            model.ProductId = productId;

            return View("_SpecialPriceCreatePopUp", model);
        }

        [HttpPost]
        public async Task<IActionResult> SpecialPriceCreatePopUp(ErpSpecialPriceModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (await _erpSpecialPriceService.CheckAnySpecialPriceExistWithAccountIdAndProductId(model.ErpAccountId, model.ProductId))
            {
                ModelState.AddModelError("ErpAccountId", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Warning.AlreadyExist"));
            }

            if (ModelState.IsValid)
            {
                var erpProductPricing = new ErpSpecialPrice
                {
                    NopProductId = model.ProductId,
                    ErpAccountId = model.ErpAccountId,
                    Price = model.Price,
                    DiscountPerc = model.DiscountPerc,
                    PricingNote = model.PricingNote,
                    PercentageOfAllocatedStock = _b2BB2CFeaturesSettings.PercentageOfStockAllowed,
                    PercentageOfAllocatedStockResetTimeUtc = model.PercentageOfAllocatedStockResetTimeUtc,
                    ListPrice = model.ListPrice,
                    VolumeDiscount = model.VolumeDiscount
                };
                await _erpSpecialPriceService.InsertErpSpecialPriceAsync(erpProductPricing);

                var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.ActivityLog.Insert");
                //_notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync($"{successMsg}. Erp Account Id: {erpProductPricing.ErpAccountId}. Product Id: {erpProductPricing.NopProductId}. Erp Special Price Id: {erpProductPricing.Id}", ErpSyncLevel.Product, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewSpecialPrice",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpSpecialPrice"),
                    erpProductPricing.Id, erpProductPricing.ErpAccountId, erpProductPricing.NopProductId),
                    erpProductPricing);

                ViewBag.RefreshPage = true;

                return View("_SpecialPriceCreatePopUp", model);
            }

            return View("_SpecialPriceCreatePopUp", model);
        }

        public async Task<IActionResult> SpecialPriceEditPopUp(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //try to get a erpSpecialPrice with the specified id
            var erpProductPricing = await _erpSpecialPriceService.GetErpSpecialPriceByIdAsync(id);
            if (erpProductPricing == null)
                return RedirectToAction("AllProductList");

            var model = await _erpSpecialPriceModelFactory.PrepareErpProductPricingModel(null, erpProductPricing);

            return View("_SpecialPriceEditPopUp", model);
        }

        [HttpPost]
        public async Task<IActionResult> SpecialPriceEditPopUp(ErpSpecialPriceModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var erpProductPricing = await _erpSpecialPriceService.GetErpSpecialPriceByIdAsync(model.Id);
            if (erpProductPricing == null)
                return RedirectToAction("ProductPricingByProduct", new { id = model.ProductId });

            if (erpProductPricing.ErpAccountId != model.ErpAccountId && (await _erpSpecialPriceService.CheckAnySpecialPriceExistWithAccountIdAndProductId(model.ErpAccountId, model.ProductId)))
            {
                ModelState.AddModelError("B2BAccountId", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.Warning.AlreadyExist"));
            }

            if (ModelState.IsValid)
            {
                erpProductPricing.ErpAccountId = model.ErpAccountId;
                erpProductPricing.Price = model.Price;
                erpProductPricing.PricingNote = model.PricingNote;
                erpProductPricing.ListPrice = model.ListPrice;
                erpProductPricing.VolumeDiscount = model.VolumeDiscount;
                await _erpSpecialPriceService.UpdateErpSpecialPriceAsync(erpProductPricing);

                await _staticCacheManager.RemoveByPrefixAsync(ERPIntegrationCoreDefaults.ErpProductPricingCommonPrefix);
                await _staticCacheManager.RemoveAsync(_staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<ErpSpecialPrice>.ByIdCacheKey, erpProductPricing.Id));

                ViewBag.RefreshPage = true;

                var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.ActivityLog.Update");
                //_notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync($"{successMsg}. Erp Account Id: {erpProductPricing.ErpAccountId}. Product Id: {erpProductPricing.NopProductId}. Erp Special Price Id: {erpProductPricing.Id}", ErpSyncLevel.Product, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_EditSpecialPrice",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.EditErpSpecialPrice"),
                    erpProductPricing.Id, erpProductPricing.ErpAccountId, erpProductPricing.NopProductId),
                    erpProductPricing);

                return View("_SpecialPriceEditPopUp", model);
            }

            return View("_SpecialPriceEditPopUp", model);
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSpecialPrice(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var erpProductPricing = await _erpSpecialPriceService.GetErpSpecialPriceByIdAsync(id);
            if (erpProductPricing == null)
                return RedirectToAction("AllProductList");

            await _staticCacheManager.RemoveAsync(_staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<ErpSpecialPrice>.ByIdCacheKey, erpProductPricing.Id));
            await _erpSpecialPriceService.DeleteErpSpecialPriceByIdAsync(erpProductPricing.Id);

            var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSpecialPrice.ActivityLog.Delete");
            //_notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync($"{successMsg}. Erp Special Price Id: {erpProductPricing.Id}", ErpSyncLevel.Product, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteSpecialPrice",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpSpecialPrice"),
                erpProductPricing.Id, erpProductPricing.ErpAccountId, erpProductPricing.NopProductId),
                erpProductPricing);

            return new NullJsonResult();
        }

        #endregion

        #region Price Group Product Pricing

        public async Task<IActionResult> PriceGroupProductPricingList(ErpPriceGroupProductPricingSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if ((await _productService.GetProductByIdAsync(searchModel.ProductId) is null))
                throw new ArgumentException("No product found with the specified id");

            var model = await _erpPriceGroupProductPricingModelFactory.PrepareErpProductPricingListModel(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> CreatePriceGroupProductPricing(int productId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var model = await _erpPriceGroupProductPricingModelFactory.PrepareErpProductPricingModel(new ErpPriceGroupProductPricingModel(), null);
            model.ProductId = productId;

            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/ErpProductPricing/Create.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePriceGroupProductPricing(int productId, ErpPriceGroupProductPricingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (await _erpGroupPriceService.CheckAnyPriceGroupProductPricingExistWithProductIdAndPriceGroupCodeId(model.ProductId, model.ErpGroupPriceCodeId))
            {
                ModelState.AddModelError("ErpGroupPriceCodeId", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.ActivityLog.ErpPriceGroupProductPricing.AlreadyExist"));
            }

            if (ModelState.IsValid)
            {
                var customer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
                var erpGroupPriceCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByIdAsync(model.ErpGroupPriceCodeId);
                var erpProductPricing = new ErpGroupPrice
                {
                    NopProductId = productId,
                    ErpNopGroupPriceCodeId = model.ErpGroupPriceCodeId,
                    ErpNopGroupPriceCode = erpGroupPriceCode,
                    Price = model.Price,
                    IsActive = true,
                    CreatedById = customer.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                };
                await _erpGroupPriceService.InsertErpGroupPriceAsync(erpProductPricing);

                var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.ActivityLog.ErpPriceGroupProductPricing.Insert");
                //_notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync($"{successMsg}. Group Price Code Id: {erpProductPricing.ErpNopGroupPriceCodeId}. Product Id: {erpProductPricing.NopProductId}. Group Price Id: {erpProductPricing.Id}", ErpSyncLevel.Product, customer: customer);

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpGroupPrice",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpGroupPrice"),
                    erpProductPricing.Id, erpProductPricing.ErpNopGroupPriceCodeId, erpProductPricing.NopProductId),
                    erpProductPricing);
            }
            else
            {
                return ErrorJson(ModelState.SerializeErrors());
            }
            return Json(new { Result = true });
        }

        public async Task<IActionResult> EditPriceGroupProductPricing(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var erpProductPricing = await _erpGroupPriceService.GetErpGroupPriceByIdWithActiveAsync(id);
            if (erpProductPricing == null)
                return RedirectToAction("AllProductList");

            var model = await _erpPriceGroupProductPricingModelFactory.PrepareErpProductPricingModel(null, erpProductPricing);
            return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/ErpProductPricing/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> EditPriceGroupProductPricing(ErpPriceGroupProductPricingModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var erpProductPricing = await _erpGroupPriceService.GetErpGroupPriceByIdWithActiveAsync(model.Id);
            if (erpProductPricing == null)
                return RedirectToAction("ProductPricingByProduct", new { id = model.ProductId });

            if (!ModelState.IsValid)
            {
                return ErrorJson(ModelState.SerializeErrors());
            }
            erpProductPricing.Price = model.Price;
            erpProductPricing.UpdatedOnUtc = DateTime.UtcNow;
            erpProductPricing.UpdatedById = (await _b2BB2CWorkContext.GetCurrentCustomerAsync()).Id;

            await _erpGroupPriceService.UpdateErpGroupPriceAsync(erpProductPricing);

            await _staticCacheManager.RemoveAsync(_staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<ErpGroupPrice>.ByIdCacheKey, erpProductPricing.Id));

            var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.ActivityLog.ErpPriceGroupProductPricing.Update");
            //_notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync($"{successMsg}. Group Price Code Id: {erpProductPricing.ErpNopGroupPriceCodeId}. Product Id: {erpProductPricing.NopProductId}. Group Price Id: {erpProductPricing.Id}", ErpSyncLevel.Product, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_EditErpGroupPrice",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.EditErpGroupPrice"),
                erpProductPricing.Id, erpProductPricing.ErpNopGroupPriceCodeId, erpProductPricing.NopProductId),
                erpProductPricing);


            return new NullJsonResult();
        }

        [HttpPost]
        public async Task<IActionResult> DeletePriceGroupProductPricing(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var erpProductPricing = await _erpGroupPriceService.GetErpGroupPriceByIdWithActiveAsync(id);
            if (erpProductPricing == null)
                return RedirectToAction("AllProductList");

            await _staticCacheManager.RemoveAsync(_staticCacheManager.PrepareKeyForDefaultCache(NopEntityCacheDefaults<ErpGroupPrice>.ByIdCacheKey, erpProductPricing.Id));

            await _erpGroupPriceService.DeleteErpGroupPriceByIdAsync(erpProductPricing.Id);

            var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPrice.ActivityLog.ErpPriceGroupProductPricing.Delete");
            //_notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync($"{successMsg}. Group Price Code Id: {erpProductPricing.ErpNopGroupPriceCodeId}. Product Id: {erpProductPricing.NopProductId}. Group Price Id: {erpProductPricing.Id}", ErpSyncLevel.Product, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteErpGroupPrice",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpGroupPrice"),
                erpProductPricing.Id, erpProductPricing.ErpNopGroupPriceCodeId, erpProductPricing.NopProductId),
                erpProductPricing);

            return new NullJsonResult();
        }

        #endregion

        #region Export-Import Product Pricing

        //[HttpPost]
        //public async Task<IActionResult> PriceGroupProductPricingSelected(string selectedIds)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
        //        return AccessDeniedView();

        //    if (selectedIds != null)
        //    {
        //        var ids = selectedIds
        //            .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
        //            .Select(x => Convert.ToInt32(x))
        //            .ToArray();
        //        var bytes = await _erpPriceGroupProductPricingModelFactory.ExportErpPriceGroupProductPricingToXlsx(ids.ToList());
        //        var currentDate = DateTime.Now.ToString("g");
        //        return File(bytes, MimeTypes.TextXlsx, $"B2B_Price_Group_Product_Pricing_{currentDate}.xlsx");
        //    }
        //    //_notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.Export.Error"));
        //    return RedirectToAction("AllProductList");

        //}

        //[HttpPost, ActionName("List")]
        //[FormValueRequired("b2bPriceGroupProductPricing-exportexcel-all")]
        //public async Task<IActionResult> PriceGroupProductPricingAll(ProductSearchModel searchModel)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
        //        return AccessDeniedView();

        //    try
        //    {
        //        var bytes = await _erpPriceGroupProductPricingModelFactory.ExportErpPriceGroupProductPricingToXlsxAll(searchModel);
        //        var currentDate = DateTime.Now.ToString("g");
        //        return File(bytes, MimeTypes.TextXlsx, $"B2B_Price_Group_Product_Pricing_{currentDate}.xlsx");
        //    }
        //    catch (Exception exc)
        //    {
        //        //_notificationService.ErrorNotification(exc.Message);
        //        return RedirectToAction("AllProductList");
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> PriceGroupProductPricingImportExcel(IFormFile importexcelfile)
        //{
        //    if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
        //        return AccessDeniedView();

        //    try
        //    {
        //        if (importexcelfile != null && importexcelfile.Length > 0)
        //        {
        //            await _erpPriceGroupProductPricingModelFactory.ImportErpPriceGroupProductPricingFromXlsx(importexcelfile.OpenReadStream());
        //        }
        //        else
        //        {
        //            //_notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Common.UploadFile"));
        //            return RedirectToAction("AllProductList");
        //        }

        //        //_notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugins.Payments.B2BCustomerAccount.PriceGroupProductPricing.ImportFromExcel.Success"));
        //        return RedirectToAction("AllProductList");
        //    }
        //    catch (Exception exc)
        //    {
        //        //_notificationService.ErrorNotification(exc.Message);
        //        return RedirectToAction("AllProductList");
        //    }
        //}

        #endregion

        #endregion
    }
}
