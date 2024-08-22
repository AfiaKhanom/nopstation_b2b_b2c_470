using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
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
    public class ErpNopUserController : NopStationAdminController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly IWorkContext _workContext;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IErpNopUserModelFactory _erpNopUserModelFactory;
        private readonly IErpNopUserService _erpNopUserService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ICustomerService _customerService;
        private readonly IErpNopUserAccountMapService _erpNopUserAccountMapService;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
        private readonly IErpLogsService _erpLogsService;
        private readonly IWebHelper _webHelper;
        private readonly IErpActivityLogsService _erpActivityLogsService;

        #endregion

        #region Ctor

        public ErpNopUserController(
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            IWorkContext workContext,
            IErpNopUserModelFactory erpNopUserModelFactory,
            IErpNopUserService erpNopUserService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            ICustomerService customerService,
            IErpNopUserAccountMapService erpNopUserAccountMapService,
            IB2BB2CWorkContext b2BB2CWorkContext,
            IErpLogsService erpLogsService,
            IWebHelper webHelper,
            IErpActivityLogsService erpActivityLogsService)
        {
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _workContext = workContext;
            _erpNopUserModelFactory = erpNopUserModelFactory;
            _genericAttributeService = genericAttributeService;
            _erpNopUserService = erpNopUserService;
            _customerActivityService = customerActivityService;
            _customerService = customerService;
            _erpNopUserAccountMapService = erpNopUserAccountMapService;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _erpLogsService = erpLogsService;
            _webHelper = webHelper;
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

            var model = new ErpNopUserSearchModel();
            model = await _erpNopUserModelFactory.PrepareErpNopUserSearchModelAsync(searchModel: model);

            return View(model);
        }

        public async Task<IActionResult> ShipToAddressDropdownList(int erpAccountId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            var availableShipToAddresses = await _erpNopUserModelFactory.PrepareShipToAddressDropdownAsync(erpAccountId);

            return Json(availableShipToAddresses);
        }

        [HttpPost]
        public async Task<IActionResult> ErpNopUserList(ErpNopUserSearchModel erpNopUserSearchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            var model = await _erpNopUserModelFactory.PrepareErpNopUserListModelAsync(erpNopUserSearchModel);

            return Json(model);
        }

        public async Task<IActionResult> CreateErpNopUser()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //prepare model
            var model = await _erpNopUserModelFactory.PrepareErpNopUserModelAsync(new ErpNopUserModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> CreateErpNopUser(ErpNopUserModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                //fill entity from model
                var erpNopUser = model.ToEntity<ErpNopUser>();

                erpNopUser.CreatedOnUtc = DateTime.UtcNow;
                erpNopUser.CreatedById = (await _b2BB2CWorkContext.GetCurrentCustomerAsync()).Id;
                erpNopUser.ErpUserTypeId = model.ErpUserTypeId;

                await _erpNopUserService.InsertErpNopUserAsync(erpNopUser);

                var erpNopUserMap = new ErpNopUserAccountMap
                {
                    ErpUserId = erpNopUser.Id,
                    ErpAccountId = erpNopUser.ErpAccountId,
                    CustomerRolesIds = string.Join(",", model.SelectedCustomerRoleIds)
                };

                await _erpNopUserAccountMapService.InsertErpNopUserAccountMapAsync(erpNopUserMap);

                var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Added");
                _notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync($"{successMsg}. Erp Nop User Id: {erpNopUser.Id}", ErpSyncLevel.ErpNopUser, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpNopUser",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpNopUser"),
                    erpNopUser.Id),
                    erpNopUser);

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("ErpNopUserEdit", new { id = erpNopUser.Id });
            }

            //prepare model
            model = await _erpNopUserModelFactory.PrepareErpNopUserModelAsync(model, null);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public async Task<IActionResult> ErpNopUserEdit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var erpNopUser = await _erpNopUserService.GetErpNopUserByIdAsync(id);
            if (erpNopUser == null)
                return RedirectToAction("List");

            //prepare model
            var model = await _erpNopUserModelFactory.PrepareErpNopUserModelAsync(null, erpNopUser);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public async Task<IActionResult> ErpNopUserEdit(ErpNopUserModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //try to get a erpNopUser with the specified id
            var erpNopUser = await _erpNopUserService.GetErpNopUserByIdAsync(model.Id);
            if (erpNopUser == null)
                return RedirectToAction("List");

            var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();

            if (ModelState.IsValid)
            {
                try
                {
                    var currCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
                    erpNopUser.ErpShipToAddressId = model.ErpShipToAddressId;
                    erpNopUser.BillingErpShipToAddressId = model.BillingErpShipToAddressId;
                    erpNopUser.ShippingErpShipToAddressId = model.ShippingErpShipToAddressId;
                    erpNopUser.ErpUserTypeId = model.ErpUserTypeId;
                    erpNopUser.IsActive = model.IsActive;
                    erpNopUser.UpdatedOnUtc = DateTime.UtcNow;
                    erpNopUser.UpdatedById = currCustomer.Id;

                    await _erpNopUserService.UpdateErpNopUserAsync(erpNopUser);

                    var map = await _erpNopUserAccountMapService.GetErpNopUserAccountMapByAccountAndUserIdAsync(accountId: model.ErpAccountId, userId: model.Id);

                    var erpNopUserMap = new ErpNopUserAccountMap();
                    erpNopUserMap.ErpUserId = model.Id;
                    erpNopUserMap.ErpAccountId = model.ErpAccountId;
                    erpNopUserMap.CustomerRolesIds = string.Join(",", model.SelectedCustomerRoleIds);

                    if (map != null)
                    {
                        erpNopUserMap.Id = map.Id;
                        await _erpNopUserAccountMapService.UpdateErpNopUserAccountMapAsync(erpNopUserMap);
                    }
                    else
                        await _erpNopUserAccountMapService.InsertErpNopUserAccountMapAsync(erpNopUserMap);

                    var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Updated");
                    _notificationService.SuccessNotification(successMsg);

                    await _erpLogsService.InformationAsync($"{successMsg}. Erp Nop User Id: {erpNopUser.Id}", ErpSyncLevel.Account, customer: currentCustomer);

                    //erp activity log
                    await _erpActivityLogsService.InsertErpActivityAsync("Erp_EditErpNopUser",
                        string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.EditErpNopUser"),
                        erpNopUser.Id),
                        erpNopUser);

                    if (!continueEditing)
                        return RedirectToAction("List");

                    return RedirectToAction("ErpNopUserEdit", new { id = erpNopUser.Id });
                }
                catch (Exception exc)
                {
                    _notificationService.ErrorNotification(exc.Message);
                    await _erpLogsService.InformationAsync($"{exc.Message}. Erp Nop User Id: {erpNopUser.Id}", ErpSyncLevel.Account, exc, customer: currentCustomer);
                }
            }

            //prepare model
            model = await _erpNopUserModelFactory.PrepareErpNopUserModelAsync(model, erpNopUser);

            //if we got this far, something failed, redisplay form
            return View(model);
        }

        public virtual async Task<IActionResult> Impersonate(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AllowCustomerImpersonation))
                return AccessDeniedView();

            //try to get a customer with the specified id
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
                return RedirectToAction("List");

            var erpNopUser = await _erpNopUserService.GetErpNopUserByCustomerIdAsync(id);

            if (!customer.Active)
            {
                _notificationService.WarningNotification(
                    await _localizationService.GetResourceAsync("Admin.Customers.Customers.Impersonate.Inactive"));
                return RedirectToAction("Edit", erpNopUser.Id);
            }

            //ensure that a non-admin user cannot impersonate as an administrator
            //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (!await _customerService.IsAdminAsync(currentCustomer) && await _customerService.IsAdminAsync(customer))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.NonAdminNotImpersonateAsAdminError"));
                return RedirectToAction("Edit", erpNopUser.Id);
            }

            //ensure login is not required
            customer.RequireReLogin = false;
            await _customerService.UpdateCustomerAsync(customer);
            await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, customer.Id);
            await _genericAttributeService.SaveAttributeAsync<string?>(currentCustomer, NopCustomerDefaults.LastVisitedPageAttribute, $"{_webHelper.GetStoreLocation().TrimEnd('/')}/Admin/ErpNopUser/ErpNopUserEdit/{erpNopUser.Id}");

            var successMsg = await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.Customer");
            await _erpLogsService.InformationAsync($"{successMsg}. Impersonated Customer Id: {customer.Id}. Original Customer Id: {currentCustomer.Id}", ErpSyncLevel.SalesOrg, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync(customer, "Erp_CustomerImpersonationStart",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.CustomerImpersonationStarted"),
                customer.Email, customer.Id, currentCustomer.Email, currentCustomer.Id), customer);

            return RedirectToAction("Index", "Home", new { area = string.Empty });
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
                return AccessDeniedView();

            //try to get a erpNopUser with the specified id
            var erpNopUser = await _erpNopUserService.GetErpNopUserByIdAsync(id);
            if (erpNopUser == null)
                return RedirectToAction("List");

            //delete a erpNopUser
            await _erpNopUserService.DeleteErpNopUserByIdAsync(erpNopUser.Id);

            var nopUserAccountMaps = await _erpNopUserAccountMapService.GetAllErpNopUserAccountMapsByUserIdAsync(erpNopUser.Id);

            //delete nopUser-account maps from mapping table
            foreach (var map in nopUserAccountMaps)
            {
                await _erpNopUserAccountMapService.DeleteErpNopUserAccountMapByIdAsync(map.Id);
            }

            var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Deleted");
            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.ErrorAsync($"{successMsg}. Erp Nop User Id: {erpNopUser.Id}", ErpSyncLevel.Account, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteErpNopUser",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpNopUser"), id), erpNopUser);

            return RedirectToAction("List");
        }

        #endregion

        #region ErpAccount List of Nop User

        [HttpPost]
        public async Task<IActionResult> NopUsersErpAccountList(ErpNopUserSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            var model = await _erpNopUserModelFactory.PrepareErpNopUserAccountListModelAsync(searchModel.NopUserId, searchModel);

            return Json(model);
        }

        public async Task<IActionResult> ErpNopUserAccountAddPopUp(int erpNopUserId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            //prepare model
            var model = await _erpNopUserModelFactory.PrepareErpNopUserModelAsync(new ErpNopUserAccountMapModel(), null);
            model.ErpUserId = erpNopUserId;
            return View("_ErpNopUserAccountAddPopUp", model);
        }

        [HttpPost]
        public async Task<IActionResult> ErpNopUserAccountAddPopUp(ErpNopUserAccountMapModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            if (await _erpNopUserAccountMapService.CheckAnyErpNopUserAccountMapExistWithAccountIdAndUserIdAsync(model.ErpAccountId, model.ErpUserId))
            {
                ModelState.AddModelError("ErpUserId", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUserAccountMap.Warning.AlreadyExist"));
            }

            if (ModelState.IsValid)
            {
                var erpNopUserAccountMap = new ErpNopUserAccountMap
                {
                    ErpUserId = model.ErpUserId,
                    ErpAccountId = model.ErpAccountId,
                    CustomerRolesIds = string.Join(",", model.SelectedCustomerRoleIds),
                };
                await _erpNopUserAccountMapService.InsertErpNopUserAccountMapAsync(erpNopUserAccountMap);

                var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpNopUser.Account.Added");
                await _erpLogsService.InformationAsync($"{successMsg}. Erp Account Id: {model.ErpAccountId}. Erp Nop User Id: {model.ErpUserId}", ErpSyncLevel.Account, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpNopUserAccountMap",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpNopUserAccountMap"),
                    erpNopUserAccountMap.Id),
                    erpNopUserAccountMap);

                ViewBag.RefreshPage = true;
                return View("_ErpNopUserAccountAddPopUp", model);
            }

            var erpNpUserAccountMap = new ErpNopUserAccountMap();
            erpNpUserAccountMap.ErpAccountId = model.ErpAccountId;
            erpNpUserAccountMap.ErpUserId = model.ErpUserId;
            model = await _erpNopUserModelFactory.PrepareErpNopUserModelAsync(new ErpNopUserAccountMapModel(), erpNpUserAccountMap);
            return View("_ErpNopUserAccountAddPopUp", model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ErpNopUserAccountDelete(int erpAccountId, int nopUserId)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return AccessDeniedView();

            var defaultErpAccountId = (await _erpNopUserService.GetErpNopUserByIdAsync(nopUserId)).ErpAccountId;

            if (defaultErpAccountId == erpAccountId)
                return AccessDeniedView();

            //try to get a erpNopUserAccount with the specified id
            var erpNopUserAccountMap = await _erpNopUserAccountMapService.GetErpNopUserAccountMapByAccountAndUserIdAsync(erpAccountId, nopUserId);
            
            if (erpNopUserAccountMap == null)
                return Json(new { Result = false });

            await _erpNopUserAccountMapService.DeleteErpNopUserAccountMapByIdAsync(erpNopUserAccountMap.Id);

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteErpNopUserAccountMap",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpNopUserAccountMap"),
                erpNopUserAccountMap.Id),
                erpNopUserAccountMap);

            return Json(new { Result = true });
        }

        #endregion
    }
}