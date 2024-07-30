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
using NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Models;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.B2BB2CFeatures.Areas.Admin.Controllers
{
    public class ErpActivityLogController : NopStationAdminController
    {
        #region Fields

        private readonly IPermissionService _permissionService;
        private readonly IErpLogsService _erpLogsService;
        private readonly IErpActivityLogModelFactory _erpActivityLogModelFactory;
        private readonly INotificationService _notificationService;
        private readonly ILocalizationService _localizationService;
        private readonly ICustomerActivityService _customerActivityService;

        #endregion

        #region Ctor

        public ErpActivityLogController(
            IPermissionService permissionService,
            IErpLogsService erpLogsService,
            IErpActivityLogModelFactory erpActivityLogModelFactory,
            INotificationService notificationService,
            ILocalizationService localizationService,
            ICustomerActivityService customerActivityService)
        {
            _permissionService = permissionService;
            _erpLogsService = erpLogsService;
            _erpActivityLogModelFactory = erpActivityLogModelFactory;
            _notificationService = notificationService;
            _localizationService = localizationService;
            _customerActivityService = customerActivityService;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Get list of B2B Activity Log, here is prepare search model
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _erpActivityLogModelFactory.PrepareErpActivityLogSearchModelAsync(new ErpActivityLogSearchModel());

            return View(model);
        }

        /// <summary>
        /// Get list of Erp Activity Log, here is prepare list model by search model
        /// </summary>
        /// <param name="searchModel">search model</param>
        /// <returns>b2b activity log list model</returns>
        [HttpPost]
        public virtual async Task<IActionResult> ErpActivityLogListAsync(ErpActivityLogSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _erpActivityLogModelFactory.PrepareErpActivityLogListModelAsync(searchModel);

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
            var model = await _erpActivityLogModelFactory.PrepareErpActivityLogModelAsync(null, log);

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> DeleteFromList(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            //try to get a B2B Activity Log Record with the specified id
            var erpActivityLog = await _erpLogsService.GetErpLogByIdAsync(id);
            if (erpActivityLog == null)
                return RedirectToAction("List");

            //after record item delete then b2b activity log record delete
            await _erpLogsService.DeleteErpLogByIdAsync(id);

            return new NullJsonResult();
        }

        [HttpPost]
        public virtual async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                return await AccessDeniedDataTablesJson();

            //try to get a B2B Activity Log Record with the specified id
            var erpActivityLog = await _erpLogsService.GetErpLogByIdAsync(id);
            if (erpActivityLog == null)
                return RedirectToAction("List");

            //after record item delete then b2b activity log record delete
            await _erpLogsService.DeleteErpLogByIdAsync(id);

            var successMsg = "An Erp Log is Deleted!";
            _notificationService.SuccessNotification(successMsg);

            return RedirectToAction("List");
        }

        [HttpPost, ActionName("List")]
        [FormValueRequired("clearall")]
        public virtual async Task<IActionResult> ClearAll()
        {
            //clear ERP activity log permission have to insert
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
}
