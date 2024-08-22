using System;
using System.Linq;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.EMMA;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Services.Common;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpNopUser;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers
{
    public class SalesRepUsersController : NopStationAdminController
    {
        #region Fields

        private readonly IWorkContext _workContext;
        private readonly IErpSalesRepService _erpSalesRepService;
        private readonly ISalesRepUserModelFactory _salesRepUserModelFactory;
        private readonly IErpNopUserService _erpNopUserService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly IGenericAttributeService _genericAttributeService;
        private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
        private readonly IPermissionService _permissionService;
        private readonly IErpLogsService _erpLogsService;
        private readonly IErpAccountModelFactory _erpAccountModelFactory;
        private readonly IErpAccountService _erpAccountService;
        private readonly IErpActivityLogsService _erpActivityLogsService;
        private const string ADMINISTRATOR_SYSTEM_NAME = "Administrators";

        #endregion

        #region Ctor

        public SalesRepUsersController(IWorkContext workContext,
            IErpSalesRepService erpSalesRepService,
            ISalesRepUserModelFactory salesRepUserModelFactory,
            IErpNopUserService erpNopUserService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            ICustomerActivityService customerActivityService,
            IGenericAttributeService genericAttributeService,
            IB2BB2CWorkContext b2BB2CWorkContext,
            IPermissionService permissionService,
            IErpLogsService erpLogsService,
            IErpAccountModelFactory erpAccountModelFactory,
            IErpAccountService erpAccountService,
            IErpActivityLogsService erpActivityLogsService)
        {
            _workContext = workContext;
            _erpSalesRepService = erpSalesRepService;
            _salesRepUserModelFactory = salesRepUserModelFactory;
            _erpNopUserService = erpNopUserService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _customerService = customerService;
            _customerActivityService = customerActivityService;
            _genericAttributeService = genericAttributeService;
            _b2BB2CWorkContext = b2BB2CWorkContext;
            _permissionService = permissionService;
            _erpLogsService = erpLogsService;
            _erpAccountModelFactory = erpAccountModelFactory;
            _erpAccountService = erpAccountService;
            _erpActivityLogsService = erpActivityLogsService;
        }

        #endregion

        #region Utilities

        protected async Task<bool> HasB2BSalesRepRoleAsync()
        {
            var salesRepRoles = await _customerService.GetCustomerRolesAsync((await _b2BB2CWorkContext.GetCurrentERPCustomerAsync()).Customer);
            if (!salesRepRoles.Any())
            {
                return false;
            }
            var salesRepRole = salesRepRoles.FirstOrDefault(r => r.SystemName == ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName);

            if (salesRepRole == null)
            {
                if (!salesRepRoles.Any(r => r.SystemName == ADMINISTRATOR_SYSTEM_NAME))
                    return false;
            }

            return true;
        }

        #endregion

        #region B2B User For Sales Rep

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return AccessDeniedView();
            var customer = await _workContext.GetCurrentCustomerAsync();

            var salesRep = (await _erpSalesRepService.GetErpSalesRepsByNopCustomerIdAsync(customer.Id)).FirstOrDefault();
            if (salesRep == null || !salesRep.IsActive || salesRep.IsDeleted)
                return AccessDeniedView();

            var model = await _salesRepUserModelFactory.PrepareSalesRepUserSearchModelAsync(new SalesRepUserSearchModel());

            return View(model);
        }

        [HttpPost]
        public async virtual Task<IActionResult> SalesRepUsersList(SalesRepUserSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            var customer = await _workContext.GetCurrentCustomerAsync();

            var salesRep = (await _erpSalesRepService.GetErpSalesRepsByNopCustomerIdAsync(customer.Id)).FirstOrDefault();
            if (salesRep == null || !salesRep.IsActive || salesRep.IsDeleted)
                return await AccessDeniedDataTablesJson();

            var model = await _salesRepUserModelFactory.PrepareSalesRepUserListModelForSalesRep(searchModel, salesRep);

            return Json(model);
        }

        [HttpPost]
        public async virtual Task<IActionResult> MultiBuyerList(ErpAccountSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            var salesRepId = Convert.ToInt32(searchModel.ErpAccountId);

            var salesRep = (await _erpSalesRepService.GetErpSalesRepByIdAsync(salesRepId));
            if (salesRep == null || !salesRep.IsActive || salesRep.IsDeleted)
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _salesRepUserModelFactory.PrepareSalesRepErpUserListModelForSalesRep(searchModel, salesRep);

            return Json(model);
        }

        public virtual async Task<IActionResult> ErpSalesRepErpAccountAddPopup(int erpSalesRepId)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            var model = new ErpAccountSearchModel();
            model = await _erpAccountModelFactory.PrepareErpAccountSearchModelAsync(searchModel: model);

            return View(model);
        }

        [HttpPost]
        [FormValueRequired("save")]
        public virtual async Task<IActionResult> ErpSalesRepErpAccountAddPopup(SalesRepERPAccountMapModel model)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            var selectedErpAccounts = await _erpAccountService.GetAllErpAccountsByIdsAsync(0, int.MaxValue, false, false, model.SelectedErpAccountIds.ToList(), "");
            if (selectedErpAccounts.Count > 0)
            {
                foreach (var selectedAccount in selectedErpAccounts)
                {
                    var record = await _erpAccountService.GetErpSalesRepErpAccountMapByIdAsync(model.SalesRepId, selectedAccount.Id);
                    if (record is not null)
                        continue;

                    await _erpAccountService.InsertSalesRepErpAccountAsync(
                        new ErpSalesRepErpAccountMap
                        {
                            ErpAccountId = selectedAccount.Id,
                            ErpSalesRepId = model.SalesRepId
                        });

                    //erp activity log
                    await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpAccountForErpSalesRepMap",
                        string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpAccountForErpSalesRepMap"),
                        model.SalesRepId, selectedAccount.Id), new ErpSalesRepErpAccountMap());
                }
            }

            ViewBag.RefreshPage = true;
            return View(new ErpAccountSearchModel());
        }

        [HttpPost]
        public virtual async Task<IActionResult> ErpSalesRepErpAccountAddPopupList(ErpAccountSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _erpAccountModelFactory.PrepareErpAccountListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ErpSalesRepErpAccountDelete(int id, int salesRepId)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            //try to get a related product with the specified id
            var erpSalesRepErpAccountMap = await _erpAccountService.GetErpSalesRepErpAccountMapByIdAsync(salesRepId, id);

            if (erpSalesRepErpAccountMap != null)
            {
                await _erpAccountService.DeleteErpSalesRepErpAccountMapAsync(erpSalesRepErpAccountMap);

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteErpAccountForErpSalesRepMap",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpAccountForErpSalesRepMap"),
                    salesRepId, id), new ErpSalesRepErpAccountMap());
            }

            return new NullJsonResult();
        }

        public async virtual Task<IActionResult> Impersonate(int id)
        {
            if (!await HasB2BSalesRepRoleAsync())
                return AccessDeniedView();

            //try to get a erpNopUser with the specified id
            var erpNopUser = await _erpNopUserService.GetErpNopUserByIdAsync(id);
            if (erpNopUser == null || !erpNopUser.IsActive || erpNopUser.IsDeleted)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.InActiveOrDeleted"));
                return RedirectToAction("List");
            }

            //try to get a customer with the specified id
            var customer = await _customerService.GetCustomerByIdAsync(erpNopUser.NopCustomerId);
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            if (customer == null)
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeaturesErpNopUser.NoCustomerAssociated"));
                return RedirectToAction("List");
            }

            if (!customer.Active)
            {
                _notificationService.WarningNotification(
                    await _localizationService.GetResourceAsync("Admin.Customers.Customers.Impersonate.Inactive"));
                return RedirectToAction("List");
            }

            //ensure that a non-admin user cannot impersonate as an administrator
            //otherwise, that user can simply impersonate as an administrator and gain additional administrative privileges
            if (await _customerService.IsAdminAsync(customer))
            {
                _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Admin.Customers.Customers.NonAdminNotImpersonateAsAdminError"));
                return RedirectToAction("List");
            }

            //ensure login is not required
            customer.RequireReLogin = false;
            await _customerService.UpdateCustomerAsync(customer);
            await _genericAttributeService.SaveAttributeAsync<int?>(currentCustomer, NopCustomerDefaults.ImpersonatedCustomerIdAttribute, customer.Id);

            var successMsg = await _localizationService.GetResourceAsync("ActivityLog.Impersonation.Started.Customer");
            await _erpLogsService.InformationAsync($"{successMsg}. Impersonated Customer Id: {customer.Id}. Original Customer Id: {currentCustomer.Id}", ErpSyncLevel.SalesOrg, customer: await _b2BB2CWorkContext.GetCurrentCustomerAsync());

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync(customer, "Erp_CustomerImpersonationStart",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.CustomerImpersonationStarted"),
                customer.Email, customer.Id, currentCustomer.Email, currentCustomer.Id), customer);

            return RedirectToAction("Index", "Home", new { area = string.Empty });
        }

        #endregion
    }
}