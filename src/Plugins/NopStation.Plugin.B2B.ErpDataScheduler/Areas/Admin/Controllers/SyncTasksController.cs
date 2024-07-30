using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Factories;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncTaskScheduler;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Controllers
{
    public class SyncTasksController : NopStationAdminController
    {
        #region Fields

        private readonly IErpActivityLogsService _erpActivityLogsService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISyncTaskModelFactory _taskModelFactory;
        private readonly ISyncTaskService _syncTaskService;
        private readonly ISyncTaskRunner _taskRunner;
        private readonly ISyncLogService _erpSyncLogService;
        private readonly INopFileProvider _fileProvider;
        private readonly ILogger _logger;
        private const string EDIT_TASK_SYSTEM_KEYWORD = "Erp_EditSyncTask";

        #endregion

        #region Ctor

        public SyncTasksController(IErpActivityLogsService erpActivityLogsService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISyncTaskModelFactory taskModelFactory,
            ISyncTaskService taskService,
            ISyncTaskRunner taskRunner,
            ISyncLogService erpSyncLogService,
            INopFileProvider nopFileProvider,
            ILogger logger)
        {
            _erpActivityLogsService = erpActivityLogsService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _taskModelFactory = taskModelFactory;
            _syncTaskService = taskService;
            _taskRunner = taskRunner;
            _erpSyncLogService = erpSyncLogService;
            _fileProvider = nopFileProvider;
            _logger = logger;
        }

        #endregion

        #region Methods

        #region Sync Tasks

        public virtual IActionResult Index()
        {
            return RedirectToAction("List");
        }

        [HttpGet]
        public virtual async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            //prepare model
            var model = await _taskModelFactory.PrepareTaskSearchModelAsync(new SyncTaskSearchModel());

            return View(model);
        }

        [HttpPost]
        public virtual async Task<IActionResult> List(SyncTaskSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
                return await AccessDeniedDataTablesJson();

            //prepare model
            var model = await _taskModelFactory.PrepareTaskListModelAsync(searchModel);

            return Ok(model);
        }

        [HttpGet]
        public virtual async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
                return await AccessDeniedDataTablesJson();

            //try to get a custom scheduler task with the specified id
            var task = await _syncTaskService.GetTaskByIdAsync(id);

            if (task is null)
                return RedirectToAction("List");

            //prepare model
            var taskModel = await _taskModelFactory.PrepareTaskModelByIdAsync(task.Id);

            return View(taskModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> Edit(int id, SyncTaskDataModel data)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            //try to get a category with the specified id
            var task = await _syncTaskService.GetTaskByIdAsync(id);

            if (task is null)
                return RedirectToAction("List");

            task.DayTimeSlots = string.Empty;

            if (data.DayOfWeekData is not null && data.DayOfWeekData[0].IsSelected)
            {
                task.DayTimeSlots = JsonConvert.SerializeObject(data.DayOfWeekData);
            }

            await _syncTaskService.UpdateTaskAsync(task);

            //activity log
            await _erpActivityLogsService.InsertErpActivityAsync(EDIT_TASK_SYSTEM_KEYWORD, string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.ErpActivityLogs.EditSyncTask"), task.Id, task.Name), task);

            if (ModelState.IsValid)
            {
                if (!data?.ContinueEditing ?? false)
                    return NoContent();

                return RedirectToAction("Edit", new { id = task.Id });
            }

            //prepare model
            var taskSlotModel = await _taskModelFactory.PrepareTaskModelByIdAsync(task.Id);

            //if we got this far, something failed, redisplay form
            return View(taskSlotModel);
        }

        [HttpPost]
        public virtual async Task<IActionResult> TaskUpdate(SyncTaskModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            //try to get a schedule task with the specified id
            var task = await _syncTaskService.GetTaskByIdAsync(model.Id);

            //To prevent inject the XSS payload in Schedule tasks ('Name' field), we must disable editing this field, 
            //but since it is required, we need to get its value before updating the entity.
            if (!string.IsNullOrEmpty(task.Name))
            {
                model.Name = task.Name;
                model.SyncLogSearchModel.SyncTaskName = task.Name;
                model.SyncLogSearchModel.SyncTaskId = task.Id;
                ModelState.Remove(nameof(model.Name));
                ModelState.Remove(nameof(model.SyncLogSearchModel.SyncTaskName));
            }

            if (!ModelState.IsValid)
                return BadRequest(ModelState.SerializeErrors());

            if (!task.Enabled && model.Enabled)
                task.LastEnabledUtc = DateTime.UtcNow;

            task = model.ToEntity(task);

            await _syncTaskService.UpdateTaskAsync(task);

            //activity log
            await _erpActivityLogsService.InsertErpActivityAsync(EDIT_TASK_SYSTEM_KEYWORD, string.Format(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.ErpActivityLogs.EditSyncTask"), task.Id, task.Name), task);

            return NoContent();
        }

        public virtual async Task<IActionResult> RunNow(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            //try to get a schedule task with the specified id
            var task = await _syncTaskService.GetTaskByIdAsync(id);

            try
            {
                await _taskRunner.ExecuteAsync(task, true, true, true);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.RunNow.Done"));
            }
            catch (Exception exc)
            {
                await _erpSyncLogService.SyncLogSaveOnFileAsync(
                        task.Name,
                        0,
                        exc.Message,
                        exc.StackTrace);

                await _notificationService.ErrorNotificationAsync(exc);
            }

            return RedirectToAction("List");
        }

        #endregion

        #region Sync Logs

        [HttpPost]
        public async Task<IActionResult> SyncLogList(SyncLogSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            //prepare model
            var model = await _taskModelFactory.PrepareSyncLogListModelAsync(searchModel);

            return Json(model);
        }

        public async Task<IActionResult> DownloadSyncLogFile(string id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            var data = id.Split('~');
            var fileName = data[1];
            var syncTaskId = int.Parse(data[0]);

            try
            {
                var filePath = _erpSyncLogService.GetSyncLogFilePath(fileName);

                if (_fileProvider.FileExists(filePath))
                {
                    return PhysicalFile(filePath, "application/txt", fileName);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
            _notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileNotFound"));

            return RedirectToAction("Edit", new { id = syncTaskId });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteSyncLogFile(int syncTaskId, string id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
                return AccessDeniedView();

            try
            {
                var filePath = _erpSyncLogService.GetSyncLogFilePath(id);

                if (_fileProvider.FileExists(filePath))
                {
                    _fileProvider.DeleteFile(filePath);
                }
            }
            catch (Exception ex)
            {
                await _logger.ErrorAsync(ex.Message, ex);
            }
            //_notificationService.ErrorNotification(await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.SyncLogs.FileCouldNotBeDeleted"));

            return new NullJsonResult();
        }

        #endregion

        #endregion
    }
}