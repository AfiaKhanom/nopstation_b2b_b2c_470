using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Core.Domain.Customers;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpSalesRep;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.ERPIntegrationCore;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers
{
    public class SalesRepresentativeController : NopStationAdminController
    {
        #region Fields

        private readonly IErpSalesRepService _erpSalesRepService;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;
        private readonly IErpSalesRepSalesOrgMapService _erpSalesRepSalesOrgMapService;
        private readonly IErpSalesRepModelFactory _erpSalesRepModelFactory;
        private readonly IStaticCacheManager _staticCacheManager;
        private readonly IErpLogsService _erpLogsService;
        private readonly IErpActivityLogsService _erpActivityLogsService;

        #endregion

        #region Ctor

        public SalesRepresentativeController(
            IErpSalesRepService erpSalesRepService,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ICustomerService customerService,
            IPermissionService permissionService,
            IErpSalesRepSalesOrgMapService erpSalesRepSalesOrgMapService,
            IErpSalesRepModelFactory erpSalesRepModelFactory,
            IStaticCacheManager staticCacheManager,
            IErpLogsService erpLogsService,
            IErpActivityLogsService erpActivityLogsService,
            IWorkContext workContext)
        {
            _erpSalesRepService = erpSalesRepService;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _customerService = customerService;
            _permissionService = permissionService;
            _erpSalesRepSalesOrgMapService = erpSalesRepSalesOrgMapService;
            _erpSalesRepModelFactory = erpSalesRepModelFactory;
            _staticCacheManager = staticCacheManager;
            _erpLogsService = erpLogsService;
            _erpActivityLogsService = erpActivityLogsService;
            _workContext = workContext;
        }

        #endregion

        #region Utilities

        protected async Task SetSalesRepCustomerRoleAsync(int customerId)
        {
            var customer = await _customerService.GetCustomerByIdAsync(customerId);
            if (!await _customerService.IsInCustomerRoleAsync(customer, ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName))
            {
                var salesRepRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName);
                if (salesRepRole != null)
                {
                    await _customerService.AddCustomerRoleMappingAsync(new CustomerCustomerRoleMapping
                    {
                        CustomerId = customerId,
                        CustomerRoleId = salesRepRole.Id
                    });
                }
            }
        }

        protected async Task UpdateErpSalesRepSalesOrgMapAsync(ErpSalesRepModel model, ErpSalesRep salesRep)
        {
            if ((SalesRepType)model.SalesRepTypeId == SalesRepType.BySalesOrganisation)
            {
                var salesRepMap = await _erpSalesRepSalesOrgMapService.GetErpSalesRepSalesOrgMapsByErpSalesRepIdAsync(salesRep.Id);
                foreach (var orgId in model.SalesOrgIds)
                {
                    if (salesRepMap == null || !salesRepMap.Any(a => a.ErpSalesOrgId == orgId))
                    {
                        var salesRepSalesOrgMap = new ErpSalesRepSalesOrgMap
                        {
                            ErpSalesRepId = salesRep.Id,
                            ErpSalesOrgId = orgId
                        };
                        await _erpSalesRepSalesOrgMapService.InsertErpSalesRepSalesOrgMapAsync(salesRepSalesOrgMap);
                    }
                }

                if (model.SalesOrgIds.Any())
                    await _staticCacheManager.RemoveByPrefixAsync(ERPIntegrationCoreDefaults.SalesRepOrgBySalesRepPrefix, salesRep.Id);

                await RemoveSalesRepMapsAsync(salesRep.Id, model.SalesOrgIds);
            }
            else
            {
                await RemoveSalesRepMapsAsync(salesRep.Id);
            }

        }

        protected async Task RemoveSalesRepMapsAsync(int salesRepId, IList<int> salesOrgIds = null)
        {
            var salesRepMapsToBeRemoved = await _erpSalesRepSalesOrgMapService.GetErpSalesRepSalesOrgMapsByErpSalesRepIdAsync(salesRepId);

            if (salesOrgIds != null && salesOrgIds.Any())
            {
                salesRepMapsToBeRemoved = salesRepMapsToBeRemoved.Where(m => !salesOrgIds.Contains(m.ErpSalesOrgId)).ToList();
            }

            if (salesRepMapsToBeRemoved.Any())
            {
                foreach (var map in salesRepMapsToBeRemoved)
                {
                    await _erpSalesRepSalesOrgMapService.DeleteErpSalesRepSalesOrgMapAsync(map);
                }

                await _staticCacheManager.RemoveByPrefixAsync(ERPIntegrationCoreDefaults.SalesRepOrgBySalesRepPrefix, salesRepId);
            }
        }

        #endregion

        #region B2B User For Sales Rep

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return AccessDeniedView();

            //prepare model
            var model = await _erpSalesRepModelFactory.PrepareErpSalesRepSearchModelAsync(new ErpSalesRepSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> SalesRepList(ErpSalesRepSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            var model = await _erpSalesRepModelFactory.PrepareErpSalesRepListModelAsync(searchModel);

            return Json(model);
        }

        public virtual async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            var model = await _erpSalesRepModelFactory.PrepareErpSalesRepModelAsync(new ErpSalesRepModel(), null);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        [FormValueRequired("save", "save-continue")]
        public virtual async Task<IActionResult> Create(ErpSalesRepModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            if (ModelState.IsValid)
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();

                var existingErpSalesRep = (await _erpSalesRepService.GetErpSalesRepsByNopCustomerIdAsync(model.NopCustomerId, true)).Any();
                var salesRep = model.ToEntity<ErpSalesRep>();
                salesRep.SalesRepTypeId = model.SalesRepTypeId;
                salesRep.CreatedOnUtc = DateTime.UtcNow;
                salesRep.CreatedById = currentCustomer.Id;

                if (existingErpSalesRep)
                {
                    ModelState.AddModelError("ErpSalesRepId", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesRep.Warning.AlreadyExist"));
                }
                try
                {
                    await _erpSalesRepService.InsertErpSalesRepAsync(salesRep);
                    await SetSalesRepCustomerRoleAsync(model.NopCustomerId);

                    await UpdateErpSalesRepSalesOrgMapAsync(model, salesRep);

                    var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesRepSalesOrgMap.ActivityLog.Updated");
                    _notificationService.SuccessNotification(successMsg);

                    await _erpLogsService.InformationAsync($"{successMsg}. Erp Sales Rep Id: {salesRep.Id}", ErpSyncLevel.SalesRep, customer: currentCustomer);

                    //erp activity log
                    await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpSalesRep",
                        string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpSalesRep"),
                        salesRep.Id),
                        salesRep);
                }
                catch (Exception ex)
                {
                    _notificationService.ErrorNotification(ex.Message);
                    await _erpLogsService.ErrorAsync($"{ex.Message}. Erp Sales Rep Id: {salesRep.Id}", ErpSyncLevel.SalesRep, ex, customer: currentCustomer);
                }

                if (!continueEditing)
                    return RedirectToAction("List");

                return RedirectToAction("Edit", new { id = salesRep.Id });
            }

            model = await _erpSalesRepModelFactory.PrepareErpSalesRepModelAsync(model, null);

            return View(model);
        }

        [HttpGet]
        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            var salesRep = await _erpSalesRepService.GetErpSalesRepByIdAsync(id);
            if (salesRep == null)
            {
                return RedirectToAction("List");
            }

            var model = await _erpSalesRepModelFactory.PrepareErpSalesRepModelAsync(null, salesRep);

            return View(model);
        }

        [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
        public virtual async Task<IActionResult> Edit(ErpSalesRepModel model, bool continueEditing, IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            var salesRep = await _erpSalesRepService.GetErpSalesRepByIdAsync(model.Id);

            if (salesRep == null)
            {
                ModelState.AddModelError(nameof(ErpSalesRepModel.Id), await _localizationService.GetResourceAsync("B2BB2CFeatures.SalesRepresentatives.NotFound"));
            }

            if (ModelState.IsValid)
            {
                var currentCustomer = await _workContext.GetCurrentCustomerAsync();

                salesRep.NopCustomerId = model.NopCustomerId;
                salesRep.SalesRepTypeId = model.SalesRepTypeId;
                salesRep.UpdatedOnUtc = DateTime.UtcNow;
                salesRep.UpdatedById = currentCustomer.Id;
                salesRep.IsActive = model.IsActive;

                try
                {
                    await _erpSalesRepService.UpdateErpSalesRepAsync(salesRep);
                    await UpdateErpSalesRepSalesOrgMapAsync(model, salesRep);
                    await SetSalesRepCustomerRoleAsync(model.NopCustomerId);

                    var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesRep.Updated");
                    _notificationService.SuccessNotification(successMsg);

                    await _erpLogsService.InformationAsync($"{successMsg}. Erp Sales Rep Id: {salesRep.Id}", ErpSyncLevel.SalesRep, customer: currentCustomer);

                    //erp activity log
                    await _erpActivityLogsService.InsertErpActivityAsync("Erp_EditErpSalesRep",
                        string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.EditErpSalesRep"),
                        salesRep.Id),
                        salesRep);

                    if (!continueEditing)
                        return RedirectToAction("List");

                }
                catch (Exception ex)
                {
                    _notificationService.ErrorNotification(ex.Message);
                }
            }

            model = await _erpSalesRepModelFactory.PrepareErpSalesRepModelAsync(model, salesRep);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            //try to get a salesRep with the specified id
            var salesRep = await _erpSalesRepService.GetErpSalesRepByIdAsync(id);
            if (salesRep == null)
                return RedirectToAction("List");

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            try
            {
                //customerRole delete from role mapping table
                var customer = await _customerService.GetCustomerByIdAsync(salesRep.NopCustomerId);
                var customerRole = await _customerService.GetCustomerRoleBySystemNameAsync(ERPIntegrationCoreDefaults.B2BSalesRepRoleSystemName); // actually it's erpSalesRep role

                if (customer != null && customerRole != null)
                {
                    //remove 'SalesRep' Role from this customer
                    await _customerService.RemoveCustomerRoleMappingAsync(customer, customerRole);
                }

                //delete from salesRep table
                await _erpSalesRepService.DeleteErpSalesRepByIdAsync(id);

                var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesRep.Deleted");
                _notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync($"{successMsg}. Erp Sales Rep Id: {salesRep.Id}", ErpSyncLevel.SalesRep, customer: currentCustomer);

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteErpSalesRep",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpSalesRep"),
                    salesRep.Id),
                    salesRep);

                return RedirectToAction("List");
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                await _erpLogsService.ErrorAsync($"{exc.Message}. Erp Sales Rep Id: {salesRep.Id}", ErpSyncLevel.SalesRep, exc, customer: currentCustomer);
                return RedirectToAction("Edit", new { id = salesRep.Id });
            }
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
        {
            if (!await _permissionService.AuthorizeAsync(B2BB2CPermissionProvider.ManageSalesRepresantatives))
                return await AccessDeniedDataTablesJson();

            if (selectedIds == null || selectedIds.Count == 0)
                return NoContent();

            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            try
            {
                var salesReps = await _erpSalesRepService.GetErpSalesRepByIdsAsync(selectedIds.ToArray());

                await _erpSalesRepService.DeleteErpSalesRepsAsync(salesReps);

                var successMsg = await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpSalesRep.Deleted");
                _notificationService.SuccessNotification(successMsg);

                await _erpLogsService.InformationAsync($"{successMsg}. Erp Sales Rep Ids: {string.Join(",", selectedIds)}", ErpSyncLevel.SalesRep, customer: currentCustomer);

                //erp activity log
                await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteErpSalesRep",
                    string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpSalesRep"),
                    string.Join(",", selectedIds)),
                    new ErpSalesRep());

                return Json(new { Result = true });
            }
            catch (Exception exc)
            {
                _notificationService.ErrorNotification(exc.Message);
                await _erpLogsService.ErrorAsync($"{exc.Message}. Erp Sales Rep Ids: {string.Join(",", selectedIds)}", ErpSyncLevel.SalesRep, exc, customer: currentCustomer);
            }
            return Json(new { Result = false });
        }

        #endregion
    }
}
