using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpLogs;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers;

public class ErpLogsController : NopStationAdminController
{
    #region Fields

    private readonly IPermissionService _permissionService;
    private readonly IErpLogsService _erpLogsService;
    private readonly IErpLogsModelFactory _erpLogsModelFactory;
    private readonly INotificationService _notificationService;
    private readonly ILocalizationService _localizationService;
    private readonly ICustomerActivityService _customerActivityService;

    #endregion

    #region Ctor

    public ErpLogsController(IPermissionService permissionService,
        IErpLogsService erpLogsService,
        IErpLogsModelFactory erpLogsModelFactory,
        INotificationService notificationService,
        ILocalizationService localizationService,
        ICustomerActivityService customerActivityService)
    {
        _permissionService = permissionService;
        _erpLogsService = erpLogsService;
        _erpLogsModelFactory = erpLogsModelFactory;
        _notificationService = notificationService;
        _localizationService = localizationService;
        _customerActivityService = customerActivityService;
    }

    #endregion

    #region Methods

    public async Task<IActionResult> List()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _erpLogsModelFactory.PrepareErpLogsSearchModelAsync(new ErpLogsSearchModel());

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> ErpLogsListAsync(ErpLogsSearchModel searchModel)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        //prepare model
        var model = await _erpLogsModelFactory.PrepareErpLogsListModelAsync(searchModel);

        return Json(model);
    }

    public virtual async Task<IActionResult> View(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSystemLog))
            return AccessDeniedView();

        //try to get a log with the specified id
        var log = await _erpLogsService.GetErpLogByIdAsync(id);
        if (log == null)
            return RedirectToAction("List");

        //prepare model
        var model = await _erpLogsModelFactory.PrepareErpLogsModelAsync(null, log);

        return View(model);
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteFromList(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var erpActivityLog = await _erpLogsService.GetErpLogByIdAsync(id);
        if (erpActivityLog == null)
            return RedirectToAction("List");

        await _erpLogsService.DeleteErpLogByIdAsync(id);

        return new NullJsonResult();
    }

    [HttpPost]
    public virtual async Task<IActionResult> Delete(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
            return await AccessDeniedDataTablesJson();

        var erpActivityLog = await _erpLogsService.GetErpLogByIdAsync(id);
        if (erpActivityLog == null)
            return RedirectToAction("List");

        await _erpLogsService.DeleteErpLogByIdAsync(id);

        var successMsg = "An Erp Log is Deleted!";
        _notificationService.SuccessNotification(successMsg);

        return RedirectToAction("List");
    }

    [HttpPost, ActionName("List")]
    [FormValueRequired("clearall")]
    public virtual async Task<IActionResult> ClearAll()
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSystemLog))
            return AccessDeniedView();

        await _erpLogsService.ClearLogAsync();

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteErpActivityLog", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpActivityLog.DeleteErpActivityLog"));

        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpActivityLog.Cleared"));

        return RedirectToAction("List");
    }

    [HttpPost]
    public virtual async Task<IActionResult> DeleteSelected(ICollection<int> selectedIds)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageSystemLog))
            return AccessDeniedView();

        if (selectedIds == null || selectedIds.Count == 0)
            return NoContent();

        await _erpLogsService.DeleteErpLogsAsync((await _erpLogsService.GetErpLogsByIdsAsync(selectedIds.ToArray())).ToList());

        //activity log
        await _customerActivityService.InsertActivityAsync("DeleteErpActivityLogs", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ERPIntegrationCore.ErpActivityLog.DeleteErpActivityLogs"));

        return Json(new { Result = true });
    }

    #endregion
}
