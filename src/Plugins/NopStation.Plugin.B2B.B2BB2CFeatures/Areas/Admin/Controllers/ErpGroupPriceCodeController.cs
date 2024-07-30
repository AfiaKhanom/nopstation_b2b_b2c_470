using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers
{
    public class ErpGroupPriceCodeController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IErpGroupPriceCodeService _erpGroupPriceCodeService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly IErpGroupPriceCodeModelFactory _erpGroupPriceCodeModelFactory;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
        private readonly IErpLogsService _erpLogsService;
        private readonly IErpActivityLogsService _erpActivityLogsService;

        #endregion

        #region Ctor

        public ErpGroupPriceCodeController(
            IPermissionService permissionService,
            IErpGroupPriceCodeService erpGroupPriceCodeService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            IErpGroupPriceCodeModelFactory erpGroupPriceCodeModelFactory,
            IB2BB2CWorkContext b2BB2CWorkContext,
            IErpLogsService erpLogsService,
            IErpActivityLogsService erpActivityLogsService)
        {
            _permissionService = permissionService;
            _erpGroupPriceCodeService = erpGroupPriceCodeService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _erpGroupPriceCodeModelFactory = erpGroupPriceCodeModelFactory;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _erpLogsService = erpLogsService;
            _erpActivityLogsService = erpActivityLogsService;
        }

        #endregion

        #region Methods

        public async Task<IActionResult> Index()
        {
            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var model = new ErpGroupPriceCodeSearchModel();
            model = await _erpGroupPriceCodeModelFactory.PrepareErpGroupPriceCodeSearchModelAsync(searchModel: model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ErpGroupPriceCodeList(ErpGroupPriceCodeSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var model = await _erpGroupPriceCodeModelFactory.PrepareErpGroupPriceCodeListModelAsync(searchModel);
            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //prepare model
            var model = await _erpGroupPriceCodeModelFactory.PrepareErpGroupPriceCodeModelAsync(new ErpGroupPriceCodeModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Create(ErpGroupPriceCodeModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (await _erpGroupPriceCodeService.CheckAnyErpGroupPriceCodeExistByCode(model.GroupPriceCode))
            {
                ModelState.AddModelError("GroupPriceCode", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.AlreadyExist"));
            }

            if (ModelState.IsValid)
            {
                var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
                var erpGroupPriceCode = new ErpGroupPriceCode
                {
                    Code = model.GroupPriceCode,
                    LastUpdateTime = DateTime.UtcNow,
                    CreatedOnUtc = DateTime.UtcNow,
                    CreatedById = currentCustomer.Id,
                    IsActive = model.IsActive,
                };
                await _erpGroupPriceCodeService.InsertErpGroupPriceCodeAsync(erpGroupPriceCode);

                var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Added");
                _notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync($"{successMsg}. Group Price Code Id: {erpGroupPriceCode.Id}", ErpSyncLavel.GroupPrice, customer: currentCustomer);

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync(currentCustomer, "Erp_AddNewErpGroupPriceCode",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpGroupPriceCode"),
                    erpGroupPriceCode.Id),
                    erpGroupPriceCode);

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = erpGroupPriceCode.Id });
            }
            //prepare model
            model = await _erpGroupPriceCodeModelFactory.PrepareErpGroupPriceCodeModelAsync(model, null);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var erpGroupPriceCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByIdAsync(id);
            if (erpGroupPriceCode == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _erpGroupPriceCodeModelFactory.PrepareErpGroupPriceCodeModelAsync(null, erpGroupPriceCode);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> Edit(ErpGroupPriceCodeModel model, bool continueEditing, IFormCollection form)
        {
            var erpGroupPriceCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByIdAsync(model.Id);
            if (erpGroupPriceCode == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                try
                {
                    var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();

                    if (erpGroupPriceCode.Code != model.GroupPriceCode && (await _erpGroupPriceCodeService.CheckAnyErpGroupPriceCodeExistByCode(model.GroupPriceCode)))
                    {
                        ModelState.AddModelError("GroupPriceCode", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.AlreadyExist"));
                    }

                    if (ModelState.IsValid)
                    {
                        erpGroupPriceCode.Code = model.GroupPriceCode;
                        erpGroupPriceCode.IsActive = model.IsActive;
                        erpGroupPriceCode.UpdatedOnUtc = DateTime.UtcNow;
                        erpGroupPriceCode.LastUpdateTime = DateTime.UtcNow;
                        erpGroupPriceCode.UpdatedById = currentCustomer.Id;

                        await _erpGroupPriceCodeService.UpdateErpGroupPriceCodeAsync(erpGroupPriceCode);

                        var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Updated");
                        _notificationService.SuccessNotification(successMsg);

                        await _erpLogsService.InformationAsync($"{successMsg}. Group Price Code Id: {erpGroupPriceCode.Id}", ErpSyncLavel.GroupPrice, customer: currentCustomer);

                        //erp activity log
                        await _erpActivityLogsService.InsertErpActivityAsync(currentCustomer, "Erp_EditErpGroupPriceCode",
                            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.EditErpGroupPriceCode"),
                            erpGroupPriceCode.Id),
                            erpGroupPriceCode);

                        if (!continueEditing)
                            return RedirectToAction("List");

                        return RedirectToAction("Edit", new { id = erpGroupPriceCode.Id });
                    }
                }
                catch (Exception ex)
                {
                    _notificationService.ErrorNotification(ex.Message);
                    await _erpLogsService.ErrorAsync(ex.Message + " Code Id: " + erpGroupPriceCode.Id, ErpSyncLavel.GroupPrice, ex, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());
                }
            }

            //prepare model
            model = await _erpGroupPriceCodeModelFactory.PrepareErpGroupPriceCodeModelAsync(model, erpGroupPriceCode);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var erpPriceGroupCode = await _erpGroupPriceCodeService.GetErpGroupPriceCodeByIdAsync(id);
            if (erpPriceGroupCode == null)
                return RedirectToAction("List");

            await _erpGroupPriceCodeService.DeleteErpGroupPriceCodeByIdAsync(erpPriceGroupCode.Id);

            var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
            var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpGroupPriceCode.Deleted");
            _notificationService.SuccessNotification(successMsg);
            
            await _erpLogsService.InformationAsync($"{successMsg}. Group Price Code Id:  {erpPriceGroupCode.Id}", ErpSyncLavel.GroupPrice, customer: currentCustomer);

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync(currentCustomer, "Erp_DeleteErpGroupPriceCode",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpGroupPriceCode"), id),
                erpPriceGroupCode);

            return RedirectToAction("List");
        }

        #endregion
    }
}
