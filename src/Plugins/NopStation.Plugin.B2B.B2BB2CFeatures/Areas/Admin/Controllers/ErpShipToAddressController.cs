using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Common;
using Nop.Services.Common;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpShipToAddress;
using NopStation.Plugin.B2B.ERPIntegrationCore.Domain;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class ErpShipToAddressController : NopStationAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IErpShipToAddressModelFactory _erpShipToAddressModelFactory;
    private readonly IErpShipToAddressService _erpShipToAddressService;
    private readonly IAddressService _addressService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpAccountService _accountService;
    private readonly IWorkContext _workContext;
    private readonly IErpActivityLogsService _erpActivityLogsService;

    #endregion

    #region Ctor

    public ErpShipToAddressController(ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IErpShipToAddressModelFactory erpShipToAddressModelFactory,
        IErpShipToAddressService erpShipToAddressService,
        IAddressService addressService,
        IErpActivityLogsService erpActivityLogsService,
        IErpLogsService erpLogsService,
        IErpAccountService erpAccountService,
        IWorkContext workContext)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _erpShipToAddressModelFactory = erpShipToAddressModelFactory;
        _erpShipToAddressService = erpShipToAddressService;
        _addressService = addressService;
        _erpLogsService = erpLogsService;
        _erpActivityLogsService = erpActivityLogsService;
        _accountService = erpAccountService;
        _workContext = workContext;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> Index()
    {
        return RedirectToAction("List");
    }

    public virtual async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //prepare model
        var model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressSearchModelAsync(new ErpShipToAddressSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> List(ErpShipToAddressSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> Create()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //prepare model
        var model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressModelAsync(new ErpShipToAddressModel(), null);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    [FormValueRequired("save", "save-continue")]
    public virtual async Task<IActionResult> Create(ErpShipToAddressModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        if (ModelState.IsValid)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();

            var address = model.AddressModel.ToEntity<Address>();
            await _addressService.InsertAddressAsync(address);
            var erpShipToAddress = model.ToEntity<ErpShipToAddress>();
            erpShipToAddress.AddressId = address.Id;
            erpShipToAddress.CreatedOnUtc = DateTime.UtcNow;
            erpShipToAddress.CreatedById = currentCustomer.Id;
            await _erpShipToAddressService.InsertErpShipToAddressAsync(erpShipToAddress);
            var erpAccount = await _accountService.GetErpAccountByIdAsync(model.ErpAccountId);
            await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(erpAccount, erpShipToAddress, ErpShipToAddressCreatedByType.Admin);

            var successMsg = await _localizationService.GetResourceAsync("Admin.ErpShipToAddresss.Added");
            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync($"{successMsg}. Erp Ship to Address Id: {erpShipToAddress.Id}. Erp Account Id: {erpAccount.Id}", ErpSyncLevel.ShipToAddress, customer: currentCustomer);

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_AddNewErpShipToAddress",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.AddNewErpShipToAddress"),
                erpShipToAddress.Id, erpAccount.Id),
                erpShipToAddress);

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = erpShipToAddress.Id });
        }

        //prepare model
        model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressModelAsync(model, null, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    public virtual async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //try to get a erpShipToAddress with the specified id
        var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(id);
        if (erpShipToAddress == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressModelAsync(null, erpShipToAddress);

        return View(model);
    }

    [HttpPost, ParameterBasedOnFormName("save-continue", "continueEditing")]
    public virtual async Task<IActionResult> Edit(ErpShipToAddressModel model, bool continueEditing, IFormCollection form)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //try to get a erpShipToAddress with the specified id
        var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(model.Id);
        if (erpShipToAddress == null)
            return RedirectToAction("List");

        if (ModelState.IsValid)
        {
            var currentCustomer = await _workContext.GetCurrentCustomerAsync();
            var address = model.AddressModel.ToEntity<Address>();
            await _addressService.UpdateAddressAsync(address);

            erpShipToAddress = model.ToEntity(erpShipToAddress);
            erpShipToAddress.AddressId = address.Id;
            erpShipToAddress.UpdatedById = currentCustomer.Id;
            erpShipToAddress.UpdatedOnUtc = DateTime.UtcNow;
            await _erpShipToAddressService.UpdateErpShipToAddressAsync(erpShipToAddress);

            if (await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(erpShipToAddress.Id) == null)
            {
                await _erpShipToAddressService.InsertErpShipToAddressErpAccountMapAsync(await _accountService.GetErpAccountByIdAsync(model.ErpAccountId), erpShipToAddress, ErpShipToAddressCreatedByType.User);
            }

            var shipToAddressErpAccountMap = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(erpShipToAddress.Id);

            var successMsg = await _localizationService.GetResourceAsync("Admin.ErpShipToAddresss.Updated");
            _notificationService.SuccessNotification(successMsg);

            await _erpLogsService.InformationAsync($"{successMsg}. Erp Ship to Address Id: {erpShipToAddress.Id}. Erp Account Id: {shipToAddressErpAccountMap.ErpAccountId}", ErpSyncLevel.ShipToAddress, customer: currentCustomer);

            //erp activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_EditErpShipToAddress",
                string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.EditErpShipToAddress"),
                erpShipToAddress.Id, shipToAddressErpAccountMap.ErpAccountId),
                erpShipToAddress);

            if (!continueEditing)
                return RedirectToAction("List");

            return RedirectToAction("Edit", new { id = erpShipToAddress.Id });
        }

        //prepare model
        model = await _erpShipToAddressModelFactory.PrepareErpShipToAddressModelAsync(model, erpShipToAddress, true);

        //if we got this far, something failed, redisplay form
        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManagePlugins))
            return AccessDeniedView();

        //try to get a erpShipToAddress with the specified id
        var erpShipToAddress = await _erpShipToAddressService.GetErpShipToAddressByIdAsync(id);
        if (erpShipToAddress == null)
            return RedirectToAction("List");

        var shipToAddressErpAccountMap = await _erpShipToAddressService.GetErpShipToAddressErpAccountMapByErpShipToAddressIdAsync(erpShipToAddress.Id);

        //delete a erpShipToAddress
        await _erpShipToAddressService.DeleteErpShipToAddressAsync(erpShipToAddress);

        var successMsg = await _localizationService.GetResourceAsync("Admin.ErpShipToAddresss.Deleted");
        _notificationService.SuccessNotification(successMsg);

        await _erpLogsService.InformationAsync($"{successMsg}. Erp Ship to Address Id: {erpShipToAddress.Id}. Erp Account Id: {shipToAddressErpAccountMap.ErpAccountId}", ErpSyncLevel.ShipToAddress, customer: await _workContext.GetCurrentCustomerAsync());

        //erp activity log
        await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteErpShipToAddress",
            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpShipToAddress"),
            erpShipToAddress.Id, shipToAddressErpAccountMap.ErpAccountId),
            erpShipToAddress);

        return RedirectToAction("List");
    }

    #endregion
}