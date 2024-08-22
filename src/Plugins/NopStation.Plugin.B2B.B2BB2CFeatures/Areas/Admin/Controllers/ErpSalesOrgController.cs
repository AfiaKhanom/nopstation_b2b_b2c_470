using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.Filters;
using Nop.Web.Framework.Mvc.ModelBinding;
using Nop.Web.Framework.Validators;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.ErpCustomerFunctionality;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers
{
    public class ErpSalesOrgController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IErpSalesOrgModelFactory _erpSalesOrgModelFactory;
        private readonly IErpSalesOrgService _erpSalesOrgService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IAddressService _addressService;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
        private readonly IErpLogsService _erpLogsService;
        private readonly IErpCustomerFunctionalityService _erpCustomerFunctionalityService;
        private readonly IErpWarehouseAdditionalDataService _erpWarehouseAdditionalDataService;
        private readonly IErpWarehouseSalesOrgMapService _erpWarehouseSalesOrgMapService;
        private readonly IErpActivityLogsService _erpActivityLogsService;

        #endregion

        #region Ctor

        public ErpSalesOrgController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext,
            IErpSalesOrgModelFactory erpSalesOrgModelFactory,
            IErpSalesOrgService erpSalesOrgService,
            ICustomerActivityService customerActivityService,
            IAddressService addressService,
            IB2BB2CWorkContext b2BB2CWorkContext,
            IErpLogsService erpLogsService,
            IErpCustomerFunctionalityService erpCustomerFunctionalityService,
            IErpWarehouseAdditionalDataService erpWarehouseAdditionalDataService,
            IErpWarehouseSalesOrgMapService erpWarehouseSalesOrgMapService,
            IErpActivityLogsService erpActivityLogsService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
            _erpSalesOrgModelFactory = erpSalesOrgModelFactory;
            _erpSalesOrgService = erpSalesOrgService;
            _customerActivityService = customerActivityService;
            _addressService = addressService;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _erpLogsService = erpLogsService;
            _erpCustomerFunctionalityService = erpCustomerFunctionalityService;
            _erpWarehouseAdditionalDataService = erpWarehouseAdditionalDataService;
            _erpWarehouseSalesOrgMapService = erpWarehouseSalesOrgMapService;
            _erpActivityLogsService = erpActivityLogsService;
        }

        #endregion

        #region Methods

        #region ErpSalesOrg

        public async Task<IActionResult> Index()
        {
            return RedirectToAction("List");
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var model = new ErpSalesOrgSearchModel();
            model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgSearchModelAsync(searchModel: model);

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ErpSalesOrgList(ErpSalesOrgSearchModel erpSalesOrgSearchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            var model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgListModelAsync(erpSalesOrgSearchModel);

            return Json(model);
        }

        public async Task<IActionResult> CreateErpSalesOrg()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //prepare model
            var model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgModelAsync(new ErpSalesOrgModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> CreateErpSalesOrg(ErpSalesOrgModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
            if (!CommonHelper.IsValidEmail(model.Email))
            {
                var errMsg = await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole");

                ModelState.AddModelError(string.Empty, errMsg);
                _notificationService.ErrorNotification(errMsg);
                await _erpLogsService.ErrorAsync(errMsg, ErpSyncLevel.SalesOrg, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());
            }

            if (ModelState.IsValid)
            {
                //fill entity from model
                var erpSalesOrg = model.ToEntity<ErpSalesOrg>();
                erpSalesOrg.CreatedOnUtc = DateTime.UtcNow;
                erpSalesOrg.CreatedById = (await _b2BB2CWorkContext.GetCurrentCustomerAsync()).Id;

                await _erpSalesOrgService.InsertErpSalesOrgAsync(erpSalesOrg);

                //address
                var address = model.Address.ToEntity<Address>();
                address.CreatedOnUtc = DateTime.UtcNow;

                //some validation
                if (address.CountryId == 0)
                    address.CountryId = null;
                if (address.StateProvinceId == 0)
                    address.StateProvinceId = null;
                await _addressService.InsertAddressAsync(address);
                erpSalesOrg.AddressId = address.Id;

                await _erpSalesOrgService.UpdateErpSalesOrgAsync(erpSalesOrg);

                var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Added");
                _notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync($"{successMsg}. Erp Sales Org Id: {erpSalesOrg.Id}", ErpSyncLevel.SalesOrg, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpSalesOrg",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpSalesOrg"),
                    erpSalesOrg.Id),
                    erpSalesOrg);

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("ErpSalesOrgEdit", new { id = erpSalesOrg.Id });
            }

            //prepare model
            model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgModelAsync(model, null);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> ErpSalesOrgEdit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(id);
            if (erpSalesOrg == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgModelAsync(null, erpSalesOrg);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> ErpSalesOrgEdit(ErpSalesOrgModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //try to get a erpSalesOrg with the specified id
            var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(model.Id);
            if (erpSalesOrg == null)
                return RedirectToAction("List");

            // Ensure that valid email address is entered if Registered role is checked to avoid registered customers with empty email address
            if (!CommonHelper.IsValidEmail(model.Email))
            {
                var errMsg = await _localizationService.GetResourceAsync("Admin.Customers.Customers.ValidEmailRequiredRegisteredRole");

                ModelState.AddModelError(string.Empty, errMsg);
                _notificationService.ErrorNotification(errMsg);
                await _erpLogsService.ErrorAsync(errMsg, ErpSyncLevel.SalesOrg, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());
            }

            if (ModelState.IsValid)
            {
                try
                {
                    erpSalesOrg.Email = model.Email;
                    erpSalesOrg.Code = model.Code;
                    erpSalesOrg.IntegrationClientId = model.IntegrationClientId;
                    erpSalesOrg.Name = model.Name;
                    erpSalesOrg.IsActive = model.IsActive;
                    erpSalesOrg.AuthenticationKey = model.AuthenticationKey;
                    erpSalesOrg.UpdatedOnUtc = DateTime.UtcNow;
                    erpSalesOrg.UpdatedById = (await _b2BB2CWorkContext.GetCurrentCustomerAsync()).Id;

                    await _erpSalesOrgService.UpdateErpSalesOrgAsync(erpSalesOrg);

                    //address
                    var address = await _addressService.GetAddressByIdAsync(erpSalesOrg.AddressId);
                    if (address == null)
                    {
                        address = model.Address.ToEntity<Address>();
                        address.CreatedOnUtc = DateTime.UtcNow;

                        //some validation
                        if (address.CountryId == 0)
                            address.CountryId = null;
                        if (address.StateProvinceId == 0)
                            address.StateProvinceId = null;

                        await _addressService.InsertAddressAsync(address);

                        erpSalesOrg.AddressId = address.Id;
                        await _erpSalesOrgService.UpdateErpSalesOrgAsync(erpSalesOrg);
                    }
                    else
                    {
                        address = model.Address.ToEntity(address);

                        //some validation
                        if (address.CountryId == 0)
                            address.CountryId = null;
                        if (address.StateProvinceId == 0)
                            address.StateProvinceId = null;

                        await _addressService.UpdateAddressAsync(address);
                    }

                    var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Updated");
                    _notificationService.SuccessNotification(successMsg);

                    await _erpLogsService.InformationAsync($"{successMsg}. Erp Sales Org Id: {erpSalesOrg.Id}", ErpSyncLevel.SalesOrg, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

                    //erp activity log
                    await _erpActivityLogsService.InsertErpActivityAsync("Erp_EditErpSalesOrg",
                        string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.EditErpSalesOrg"),
                        erpSalesOrg.Id),
                        erpSalesOrg);

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("ErpSalesOrgEdit", new { id = erpSalesOrg.Id });
                }
                catch (Exception exc)
                {
                    _notificationService.ErrorNotification(exc.Message);
                    await _erpLogsService.ErrorAsync(exc.Message + " Sales Org Id: " + erpSalesOrg.Id, ErpSyncLevel.SalesOrg, exc, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());
                }
            }

            //prepare model
            model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgModelAsync(model, erpSalesOrg);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a erpSalesOrg with the specified id
            var erpSalesOrg = await _erpSalesOrgService.GetErpSalesOrgByIdAsync(id);
            if (erpSalesOrg == null)
                return RedirectToAction("List");

            //delete a erpSalesOrg
            await _erpSalesOrgService.DeleteErpSalesOrgByIdAsync(erpSalesOrg.Id);

            var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesOrg.Deleted");
            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync($"{successMsg}. Erp Sales Org Id: {erpSalesOrg.Id}", ErpSyncLevel.SalesOrg, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteErpSalesOrg",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpSalesOrg"),
                erpSalesOrg.Id),
                erpSalesOrg);

            return Json(new { response = true });
        }

        public async Task<IActionResult> IsMappedWithAnyERPAccount(int erpSalesOrgId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            var isMapped = await _erpSalesOrgService.IsMappedWithAnyERPAccountAsync(erpSalesOrgId);
            return Json(new { isMapped });
        }

        #endregion

        #region Warehouse

        [HttpPost]
        public virtual async Task<IActionResult> ErpSalesOrgWareHouseList(ErpSalesOrgWarehouseSearchModel searchModel)
        {
            if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
                return AccessDeniedView();

            var b2BFeatureSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);
            if (!b2BFeatureSettings.EnableWarehouse)
            {
                return Json(new ErpSalesOrgWarehouseListModel());
            }

            var model = await _erpSalesOrgModelFactory.PrepareErpSalesOrgWarehouseListModel(searchModel);
            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> CreateErpSalesOrgWareHouse(int salesOrgId, [Validate] ErpSalesOrgWarehouseModel model)
        {
            if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
                return AccessDeniedView();

            var b2BFeatureSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);
            if (!b2BFeatureSettings.EnableWarehouse)
            {
                return Json(new { Result = false });
            }

            if (salesOrgId == 0)
            {
                return RedirectToAction("List");
            }

            if (await _erpWarehouseSalesOrgMapService.CheckAnyErpSalesOrgWarehouseExistBySalesOrgIdAndNopWarehouseId(salesOrgId, model.WarehouseId))
                return ErrorJson(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpSalesOrgWarehouse.AlreadyExist"));

            if (model.WarehouseId > 0 && string.IsNullOrEmpty(model.ErpWarehouseCode))
                return ErrorJson(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpSalesOrgWarehouse.ErpWarehouseCode.Required"));

            if (ModelState.IsValid)
            {
                try
                {
                    var salesOrgWarehouse = new ErpWarehouseAdditionalData
                    {
                        Code = model.ErpWarehouseCode,
                        IsActive = true,
                        CreatedById = (await _b2BB2CWorkContext.GetCurrentCustomerAsync()).Id,
                        CreatedOnUtc = DateTime.UtcNow,
                    };

                    await _erpWarehouseAdditionalDataService.InsertErpWarehouseAdditionalDataAsync(salesOrgWarehouse);

                    var salesOrgWarehouseMap = new ErpWarehouseSalesOrgMap
                    {
                        NopWarehouseId = model.WarehouseId,
                        ErpWarehouseId = salesOrgWarehouse.Id,
                        ErpSalesOrgId = salesOrgId
                    };

                    await _erpWarehouseSalesOrgMapService.InsertErpWarehouseSalesOrgMapAsync(salesOrgWarehouseMap);

                    //erp activity log
                    await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpSalesOrgWarehouse",
                        string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpSalesOrgWarehouse"),
                        salesOrgWarehouseMap.ErpSalesOrgId, salesOrgWarehouseMap.NopWarehouseId),
                        salesOrgWarehouseMap);
                }
                catch (Exception ex)
                {
                    return ErrorJson(ex.Message);
                }
            }
            else
            {
                return ErrorJson(ModelState.SerializeErrors());
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public virtual async Task<IActionResult> EditErpSalesOrgWareHouse(int salesOrgId, ErpSalesOrgWarehouseModel model)
        {
            if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
                return AccessDeniedView();

            var b2BFeatureSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);
            if (!b2BFeatureSettings.EnableWarehouse)
            {
                return new NullJsonResult();
            }

            var erpSalesOrgWarehouseMap = await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapByIdAsync(model.Id);

            if (erpSalesOrgWarehouseMap != null)
            {
                var erpSalesOrgWarehouse = await _erpWarehouseAdditionalDataService.GetErpWarehouseAdditionalDataByIdAsync(erpSalesOrgWarehouseMap.ErpWarehouseId);

                if (erpSalesOrgWarehouse == null)
                    return RedirectToAction("Edit", new { id = model.ErpSalesOrgId });

                if (!ModelState.IsValid)
                {
                    return ErrorJson(ModelState.SerializeErrors());
                }

                erpSalesOrgWarehouse.Code = model.ErpWarehouseCode;
                erpSalesOrgWarehouse.UpdatedById = (await _b2BB2CWorkContext.GetCurrentCustomerAsync()).Id;
                erpSalesOrgWarehouse.UpdatedOnUtc = DateTime.UtcNow;
                erpSalesOrgWarehouse.LastUpdateTime = DateTime.UtcNow;

                await _erpWarehouseAdditionalDataService.UpdateErpWarehouseAdditionalDataAsync(erpSalesOrgWarehouse);

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_EditErpSalesOrgWarehouse",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.EditErpSalesOrgWarehouse"),
                    erpSalesOrgWarehouseMap.ErpSalesOrgId, erpSalesOrgWarehouseMap.NopWarehouseId),
                    erpSalesOrgWarehouseMap);
            }

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteErpSalesOrgWareHouse(int id)
        {
            if (!await _erpCustomerFunctionalityService.IsCurrentCustomerInAdministratorRoleAsync())
                return AccessDeniedView();

            var b2BFeatureSettings = _settingService.LoadSetting<B2BB2CFeaturesSettings>((await _storeContext.GetCurrentStoreAsync()).Id);
            if (!b2BFeatureSettings.EnableWarehouse)
            {
                return new NullJsonResult();
            }

            var erpWarehouseMap = await _erpWarehouseSalesOrgMapService.GetErpWarehouseSalesOrgMapByIdAsync(id)
                ?? throw new ArgumentException("No ERP Sales Org Warehouse found with the specified id", nameof(id));

            await _erpWarehouseSalesOrgMapService.DeleteErpWarehouseSalesOrgMapByIdAsync(erpWarehouseMap.Id);

            var erpSalesOrgWarehouse = await _erpWarehouseAdditionalDataService.GetErpWarehouseAdditionalDataByIdAsync(erpWarehouseMap.ErpWarehouseId);

            await _erpWarehouseAdditionalDataService.DeleteErpWarehouseAdditionalDataByIdAsync(erpSalesOrgWarehouse.Id);

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteErpSalesOrgWarehouse",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpSalesOrgWarehouse"),
                erpWarehouseMap.ErpSalesOrgId, erpWarehouseMap.NopWarehouseId),
                erpWarehouseMap);

            return new NullJsonResult();
        }

        #endregion

        #endregion
    }
}