using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Mvc;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Factories;
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models.ErpActivityLogs;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers
{
    public partial class ErpActivityLogsController : NopStationAdminController
    {
        #region Fields

        private readonly IErpActivityLogsModelFactory _erpActivityLogsModelFactory;
        private readonly IErpActivityLogsService _erpActivityLogsService;
        private readonly ICustomerActivityService _customerActivityService;
        private readonly ILocalizationService _localizationService;
        private readonly IPermissionService _permissionService;
        private readonly INotificationService _notificationService;

        #endregion

        #region Ctor

        public ErpActivityLogsController(IErpActivityLogsModelFactory erpActivityLogsModelFactory,
            IErpActivityLogsService erpActivityLogsService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ICustomerActivityService customerActivityService)
        {
            _erpActivityLogsModelFactory = erpActivityLogsModelFactory;
            _erpActivityLogsService = erpActivityLogsService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _customerActivityService = customerActivityService;
        }

        #endregion

        #region Methods

        public virtual async Task<IActionResult> ERPActivityTypes()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            //prepare model
            var model = await _erpActivityLogsModelFactory.PrepareErpActivityLogsTypeSearchModelAsync(new ErpActivityLogsTypeSearchModel());

            return View(model);
        }

        [HttpPost, ActionName("SaveTypes")]
        public virtual async Task<IActionResult> SaveTypes(IFormCollection form)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            //activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_UpdateErpActivityLogsTypes", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.UpdateErpActivityLogsTypes"));

            //get identifiers of selected activity types
            var selectedActivityTypesIds = form["checkbox_activity_types"]
                .SelectMany(value => value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                .Select(idString => int.TryParse(idString, out var id) ? id : 0)
                .Distinct().ToList();

            //update activity types
            var activityTypes = await _erpActivityLogsService.GetAllErpActivityTypesAsync();
            foreach (var activityType in activityTypes)
            {
                activityType.Enabled = selectedActivityTypesIds.Contains(activityType.Id);
                await _customerActivityService.UpdateActivityTypeAsync(activityType);
            }

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.UpdateErpActivityLogsTypes"));

            return RedirectToAction("ERPActivityTypes");
        }

        public virtual async Task<IActionResult> ERPActivityLogs()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            //prepare model
            var model = await _erpActivityLogsModelFactory.PrepareErpActivityLogsSearchModelAsync(new ErpActivityLogsSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ListLogs(ErpActivityLogsSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _erpActivityLogsModelFactory.PrepareErpActivityLogsListModelAsync(searchModel);

            return Json(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> ActivityLogDelete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            //try to get a log item with the specified id
            var logItem = await _erpActivityLogsService.GetErpActivityByIdAsync(id)
                ?? throw new ArgumentException("No erp activity log found with the specified id", nameof(id));

            await _erpActivityLogsService.DeleteErpActivityAsync(logItem);

            ////activity log
            //await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteErpActivityLog",
            //    await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteErpActivityLog"), logItem);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> ClearAll()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageActivityLog))
                return AccessDeniedView();

            await _erpActivityLogsService.ClearAllErpActivitiesAsync();

            //activity log
            await _erpActivityLogsService.InsertErpActivityAsync("Erp_DeleteAllErpActivityLog", await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.B2BB2CFeatures.ErpActivityLogs.DeleteAllErpActivityLog"));

            return RedirectToAction("ERPActivityLogs");
        }

        #endregion
    }
}