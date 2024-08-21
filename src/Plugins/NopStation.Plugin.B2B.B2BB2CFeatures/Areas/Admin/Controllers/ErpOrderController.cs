using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.B2BB2CFeatures.Contexts;
using NopStation.Plugin.B2B.B2BB2CFeatures.Services.Overriden;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class ErpOrderController : NopStationAdminController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly IErpOrderAdditionalDataService _erpOrderAdditionalDataService;
    private readonly IErpOrderModelFactory _erpOrderModelFactory;
    private readonly IOverriddenOrderProcessingService _overriddenOrderProcessingService;
    private readonly B2BB2CFeaturesSettings _b2BB2CFeaturesSettings;
    private readonly IErpLogsService _erpLogsService;
    private readonly IB2BB2CWorkContext _b2BB2CWorkContext;
    private readonly IErpActivityLogsService _erpActivityLogsService;

    #endregion

    #region Ctor

    public ErpOrderController(
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        IErpOrderAdditionalDataService erpOrderAdditionalDataService,
        IErpOrderModelFactory erpOrderModelFactory,
        IOverriddenOrderProcessingService overriddenOrderProcessingService,
        B2BB2CFeaturesSettings b2BB2CFeaturesSettings,
        IErpLogsService erpLogsService,
        IB2BB2CWorkContext b2BB2CWorkContext,
        IErpActivityLogsService erpActivityLogsService)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _erpOrderAdditionalDataService = erpOrderAdditionalDataService;
        _erpOrderModelFactory = erpOrderModelFactory;
        _overriddenOrderProcessingService = overriddenOrderProcessingService;
        _b2BB2CFeaturesSettings = b2BB2CFeaturesSettings;
        _erpLogsService = erpLogsService;
        _b2BB2CWorkContext = b2BB2CWorkContext;
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
            return await AccessDeniedDataTablesJson();

        var model = await _erpOrderModelFactory.PrepareErpOrderPerAccountSearchModel(new ErpOrderAdditionalDataSearchModel());
        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/ErpOrder/List.cshtml", model);
    }

    [HttpPost]
    public async Task<IActionResult> ErpOrderList(ErpOrderAdditionalDataSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var model = await _erpOrderModelFactory.PrepareErpOrderPerAccountListModel(searchModel);
        return Json(model);
    }

    public async Task<IActionResult> Edit(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(id);
        if (erpOrder == null)
            return RedirectToAction("List");

        var model = await _erpOrderModelFactory.PrepareErpOrderPerAccountModel(null, erpOrder);
        return View("~/Plugins/NopStation.Plugin.B2B.B2BB2CFeatures/Areas/Admin/Views/ErpOrder/Edit.cshtml", model);
    }

    [HttpPost]
    [FormValueRequired("reprocess")]
    public async Task<IActionResult> Edit(ErpOrderModel model)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(model?.Id ?? 0);
        if (erpOrder == null)
            return RedirectToAction("List");

        var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
        if (erpOrder.IntegrationStatusType == IntegrationStatusType.Confirmed)
        {
            var msg = await _localizationService.GetResourceAsync("NopStation.Plugin.NopStation.B2BB2CFeatures.Order.AlreadyConfirmed");
            _notificationService.ErrorNotification(msg);

            await _erpLogsService.ErrorAsync($"{msg}. Order Id: {erpOrder.Id}", ErpSyncLavel.Order, customer: currentCustomer);

            return RedirectToAction("Edit", new { id = model.Id });
        }
        var isPlaced = false;
        var errorMsg = "Type of order not found!";

        if (erpOrder.ErpOrderType == ErpOrderType.B2BSalesOrder || erpOrder.ErpOrderType == ErpOrderType.B2BQuote)
            (isPlaced, errorMsg) = await _overriddenOrderProcessingService.RetryPlaceErpOrderAtErpAsync(erpOrder, _b2BB2CFeaturesSettings);
        else if (erpOrder.ErpOrderType == ErpOrderType.B2CSalesOrder || erpOrder.ErpOrderType == ErpOrderType.B2CQuote)
            (isPlaced, errorMsg) = await _overriddenOrderProcessingService.RetryPlaceErpOrderAtErpAsync(erpOrder, _b2BB2CFeaturesSettings);

        if (!isPlaced)
        {
            _notificationService.ErrorNotification(errorMsg);

            await _erpLogsService.ErrorAsync($"{errorMsg}. Order Id: {erpOrder.Id}", ErpSyncLavel.Order, customer: currentCustomer);

            return RedirectToAction("Edit", new { id = model.Id });
        }

        var successMsg = await _localizationService.GetResourceAsync("NopStation.Plugin.NopStation.B2BB2CFeatures.Order.Reprocessed");
        _notificationService.SuccessNotification(successMsg);

        await _erpLogsService.InformationAsync($"{successMsg}. Order Id: {erpOrder.Id}", ErpSyncLavel.Order, customer: currentCustomer);

        //erp activity log
        await _erpActivityLogsService.InsertErpActivityAsync("Erp_ReprocessErpOrder",
            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.ErpOrderReprocess"),
            erpOrder.Id),
            erpOrder);

        return RedirectToAction("Edit", new { id = model.Id });
    }

    public async Task<IActionResult> ReProcess(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var erpOrder = await _erpOrderAdditionalDataService.GetErpOrderAdditionalDataByIdAsync(id);
        if (erpOrder == null)
            return RedirectToAction("List");

        var currentCustomer = await _b2BB2CWorkContext.GetCurrentCustomerAsync();
        if (erpOrder.IntegrationStatusType == IntegrationStatusType.Confirmed)
        {
            var msg = await _localizationService.GetResourceAsync("NopStation.Plugin.NopStation.B2BB2CFeatures.Order.AlreadyConfirmed");
            _notificationService.ErrorNotification(msg);

            await _erpLogsService.ErrorAsync($"{msg}. Order Id: {erpOrder.Id}", ErpSyncLavel.Order, customer: currentCustomer);

            return RedirectToAction("List");
        }

        var isPlaced = false;
        var errorMsg = "Type of order not found!";

        if (erpOrder.ErpOrderType == ErpOrderType.B2BSalesOrder || erpOrder.ErpOrderType == ErpOrderType.B2BQuote)
            (isPlaced, errorMsg) = await _overriddenOrderProcessingService.RetryPlaceErpOrderAtErpAsync(erpOrder, _b2BB2CFeaturesSettings);
        else if (erpOrder.ErpOrderType == ErpOrderType.B2CSalesOrder || erpOrder.ErpOrderType == ErpOrderType.B2CQuote)
            (isPlaced, errorMsg) = await _overriddenOrderProcessingService.RetryPlaceErpOrderAtErpAsync(erpOrder, _b2BB2CFeaturesSettings);

        if (!isPlaced)
        {
            _notificationService.ErrorNotification(errorMsg);

            await _erpLogsService.ErrorAsync($"{errorMsg}. Order Id: {erpOrder.Id}", ErpSyncLavel.Order, customer: currentCustomer);

            return RedirectToAction("List");
        }

        var successMsg = await _localizationService.GetResourceAsync("NopStation.Plugin.NopStation.B2BB2CFeatures.Order.Reprocessed");
        _notificationService.SuccessNotification(successMsg);

        await _erpLogsService.InformationAsync($"{successMsg}. Order Id: {erpOrder.Id}", ErpSyncLavel.Order, customer: currentCustomer);

        //erp activity log
        await _erpActivityLogsService.InsertErpActivityAsync("Erp_ReprocessErpOrder",
            string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.ErpOrderReprocess"),
            erpOrder.Id),
            erpOrder);

        return RedirectToAction("List");
    }

    #endregion
}