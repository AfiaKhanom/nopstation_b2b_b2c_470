using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using Nop.Services.Localization;
using Nop.Services.Logging;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework.Mvc;
using Nop.Web.Framework.Mvc.ModelBinding;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Factories;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;
using NopStation.Plugin.Misc.Core.Controllers;
using Quartz;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Controllers;

public class SyncTasksController : NopStationAdminController
{
    #region Fields

    private readonly IErpActivityLogsService _erpActivityLogsService;
    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly IPermissionService _permissionService;
    private readonly ISyncTaskModelFactory _taskModelFactory;
    private readonly ISyncTaskService _syncTaskService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly INopFileProvider _fileProvider;
    private readonly ILogger _logger;
    private readonly INopStationScheduler _nopStationScheduler;
    private const string EDIT_TASK_SYSTEM_KEYWORD = "Erp_EditSyncTask";

    #endregion

    #region Ctor

    public SyncTasksController(IErpActivityLogsService erpActivityLogsService,
        ILocalizationService localizationService,
        INotificationService notificationService,
        IPermissionService permissionService,
        ISyncTaskModelFactory taskModelFactory,
        ISyncTaskService taskService,
        ISyncLogService erpSyncLogService,
        INopFileProvider nopFileProvider,
        ILogger logger,
        INopStationScheduler nopStationScheduler)
    {
        _erpActivityLogsService = erpActivityLogsService;
        _localizationService = localizationService;
        _notificationService = notificationService;
        _permissionService = permissionService;
        _taskModelFactory = taskModelFactory;
        _syncTaskService = taskService;
        _erpSyncLogService = erpSyncLogService;
        _fileProvider = nopFileProvider;
        _logger = logger;
        _nopStationScheduler = nopStationScheduler;
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

        var oldSlot = task.DayTimeSlots;
        task.DayTimeSlots = string.Empty;

        if (data is not null && data.DayOfWeekData is not null && data.DayOfWeekData[0].IsSelected)
        {
            //Schedule triggers
            foreach (var dayData in data.DayOfWeekData)
            {
                if (!dayData.IsSelected)
                    continue;

                var dayofweek = dayData.DayOfWeek + 1;

                foreach (var timeData in dayData.TimeSlots)
                {
                    if (!timeData.IsSelected || timeData.TimeSlot is null)
                        continue;

                    await _nopStationScheduler.CreateTriggerAsync(data.QuartzJobName, dayofweek, timeData.TimeSlot);
                }
            }

            //Unschedule triggers
            var unscheduleSlots = await GetUnscheduledSlotsAsync(oldSlot, data.DayOfWeekData);

            foreach (var unscheduleSlot in unscheduleSlots)
            {
                if (!unscheduleSlot.IsSelected)
                    continue;

                var triggerKeys = new List<TriggerKey>();
                var dayofweek = unscheduleSlot.DayOfWeek + 1;

                foreach (var item in unscheduleSlot.TimeSlots)
                {
                    if (string.IsNullOrEmpty(item.TimeSlot) && !item.IsSelected)
                        continue;

                    var triggerKey = _nopStationScheduler.PrepareTriggerKey(data.QuartzJobName, dayofweek, item.TimeSlot!);

                    triggerKeys.Add(triggerKey);
                }

                await _nopStationScheduler.UnscheduleTriggerAsync(triggerKeys);
            }

            task.DayTimeSlots = JsonConvert.SerializeObject(data.DayOfWeekData);
        }

        await _syncTaskService.UpdateTaskAsync(task);

        //activity log
        await _erpActivityLogsService.InsertErpActivityAsync(EDIT_TASK_SYSTEM_KEYWORD, string.Format(
            await _localizationService.GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.ErpActivityLogs.EditSyncTask"),
            task.Id, task.Name), task);

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

    private static async Task<IList<SyncTaskDaySlotModel>> GetUnscheduledSlotsAsync(string dayTimeSlots, List<SyncTaskDaySlotModel> dayOfWeekData)
    {
        var unscheduleSlots = new List<SyncTaskDaySlotModel>();

        if (string.IsNullOrEmpty(dayTimeSlots))
        {
            return unscheduleSlots;
        }

        var oldSlots = JsonConvert.DeserializeObject<List<SyncTaskDaySlotModel>>(dayTimeSlots);

        if (oldSlots is null)
        {
            return unscheduleSlots;
        }

        foreach (var oldSlot in oldSlots)
        {
            var data = dayOfWeekData.Find(d => d.DayOfWeek == oldSlot.DayOfWeek && d.IsSelected);

            if (data is not null)
            {
                var missingSlots = await oldSlot.TimeSlots
                    .Where(t => !data.TimeSlots.Exists(t2 => t2.TimeSlot == t.TimeSlot))
                    .ToListAsync();

                if (missingSlots.Count > 0)
                {
                    unscheduleSlots.Add(new SyncTaskDaySlotModel
                    {
                        DayOfWeek = oldSlot.DayOfWeek,
                        TimeSlots = missingSlots,
                        IsSelected = oldSlot.IsSelected
                    });
                }
            }
        }

        return unscheduleSlots;
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
            ModelState.Remove(nameof(model.QuartzJobName));
        }

        if (!ModelState.IsValid)
            return BadRequest(ModelState.SerializeErrors());

        task.StopOnError = model.StopOnError;

        if (model.Enabled && !task.Enabled)
        {
            task.Enabled = true;
            await _nopStationScheduler.EnableScheduleJobAsync(task.QuartzJobName);
        }
        else if (!model.Enabled && task.Enabled)
        {
            task.Enabled = false;
            await _nopStationScheduler.DisableScheduleJobAsync(task.QuartzJobName);
        }

        if (model.IsIncremental && !task.IsIncremental)
        {
            task.IsIncremental = true;
        }
        else if (!model.IsIncremental && task.IsIncremental)
        {
            task.IsIncremental = false;
        }

        await _syncTaskService.UpdateTaskAsync(task);

        //activity log
        await _erpActivityLogsService.InsertErpActivityAsync(EDIT_TASK_SYSTEM_KEYWORD,
            string.Format(await _localizationService.GetResourceAsync(
                "Plugin.Misc.NopStation.ErpDataScheduler.ErpActivityLogs.EditSyncTask"),
                task.Id, task.Name), task);

        return NoContent();
    }

    public virtual async Task<IActionResult> Execute(int id)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
            return AccessDeniedView();

        //try to get a schedule task with the specified id
        var task = await _syncTaskService.GetTaskByIdAsync(id);

        if (task is not null)
        {
            try
            {
                task.LastStartUtc = DateTime.UtcNow;
                await _syncTaskService.UpdateTaskAsync(task);
                var jobDataMap = _nopStationScheduler.PrepareJobDataMap(new List<KeyValuePair<string, object>>());
                await _nopStationScheduler.ExecuteSchedulerAsync(task.QuartzJobName, jobDataMap);

                _notificationService.SuccessNotification(await _localizationService
                    .GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.RunNow.Progress"));
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                await _erpSyncLogService.SyncLogSaveOnFileAsync(task.Name, 0, exc.Message, exc.StackTrace ?? string.Empty);
            }
        }

        return RedirectToAction("List");
    }

    public virtual async Task<IActionResult> Terminate(int id, CancellationToken cancellationToken)
    {
        if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.ManageScheduleTasks))
            return AccessDeniedView();

        //try to get a schedule task with the specified id
        var task = await _syncTaskService.GetTaskByIdAsync(id);

        if (task is not null)
        {
            try
            {
                task.LastEndUtc = DateTime.UtcNow;
                await _syncTaskService.UpdateTaskAsync(task);
                await _nopStationScheduler.TerminateSchedulerAsync(task.QuartzJobName, cancellationToken);

                _notificationService.SuccessNotification(string.Format(await _localizationService
                    .GetResourceAsync("Plugin.Misc.NopStation.ErpDataScheduler.Tasks.Terminate"), task.Name));
            }
            catch (Exception exc)
            {
                await _notificationService.ErrorNotificationAsync(exc);
                await _erpSyncLogService.SyncLogSaveOnFileAsync(task.Name, 0, exc.Message, exc.StackTrace ?? string.Empty);
            }
        }

        return RedirectToAction("List");
    }

    [HttpGet]
    public async Task JobStatus(CancellationToken cancellationToken)
    {
        HttpContext.Response.Headers.Append(HeaderNames.ContentType, "text/event-stream");

        void onJobStatusChangedHandler(string status)
        {
            Response.WriteAsync($"data: {status}\n\n", cancellationToken: cancellationToken)
                .Wait(cancellationToken);

            Response.Body.FlushAsync(cancellationToken).Wait(cancellationToken);
        }

        NopStationJobListener.OnJobStatusChanged += onJobStatusChangedHandler;

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        finally
        {
            NopStationJobListener.OnJobStatusChanged -= onJobStatusChangedHandler;
        }
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

        return new NullJsonResult();
    }

    #endregion

    #endregion
}