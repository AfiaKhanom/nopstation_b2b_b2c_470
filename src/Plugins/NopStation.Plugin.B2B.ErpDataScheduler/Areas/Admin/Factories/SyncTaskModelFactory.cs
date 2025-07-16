using LinqToDB.Common;
using Newtonsoft.Json;
using Nop.Core.Infrastructure;
using Nop.Services.Helpers;
using Nop.Web.Areas.Admin.Infrastructure.Mapper.Extensions;
using Nop.Web.Framework.Models.Extensions;
using NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Models;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.NopStationSyncServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.QuartzServices;
using NopStation.Plugin.B2B.ErpDataScheduler.Services.SyncLogServices;
using NopStation.Plugin.B2B.ERPIntegrationCore.Enums;
using NopStation.Plugin.B2B.ERPIntegrationCore.Services;

namespace NopStation.Plugin.B2B.ErpDataScheduler.Areas.Admin.Factories;

public partial class SyncTaskModelFactory : ISyncTaskModelFactory
{
    #region Fields

    private readonly IDateTimeHelper _dateTimeHelper;
    private readonly ISyncTaskService _syncTaskService;
    private readonly ISyncLogService _erpSyncLogService;
    private readonly INopFileProvider _fileProvider;
    private readonly IQuartzJobDetailService _quartzJobDetailService;
    private readonly IQrtzFiredTriggersService _qrtzFiredTriggersService;
    private readonly IErpLogsService _erpLogsService;

    #endregion

    #region Ctor

    public SyncTaskModelFactory(IDateTimeHelper dateTimeHelper,
        ISyncTaskService taskService,
        ISyncLogService erpSyncLogService,
        INopFileProvider fileProvider,
        IQuartzJobDetailService quartzJobDetailService,
        IQrtzFiredTriggersService qrtzFiredTriggersService,
        IErpLogsService erpLogsService)
    {
        _dateTimeHelper = dateTimeHelper;
        _syncTaskService = taskService;
        _erpSyncLogService = erpSyncLogService;
        _fileProvider = fileProvider;
        _quartzJobDetailService = quartzJobDetailService;
        _qrtzFiredTriggersService = qrtzFiredTriggersService;
        _erpLogsService = erpLogsService;
    }

    #endregion

    #region Utilities

    private async Task<ErpSyncLevel> GetErpSyncLevelBySyncTaskType(string syncTaskType)
    {
        if (syncTaskType == ErpDataSchedulerDefaults.ErpAccountSyncTask)
            return ErpSyncLevel.Account;
        if (syncTaskType == ErpDataSchedulerDefaults.ErpProductSyncTask)
            return ErpSyncLevel.Product;
        if (syncTaskType == ErpDataSchedulerDefaults.ErpOrderSyncTask)
            return ErpSyncLevel.Order;
        if (syncTaskType == ErpDataSchedulerDefaults.ErpStockSyncTask)
            return ErpSyncLevel.Stock;
        if (syncTaskType == ErpDataSchedulerDefaults.ErpShipToAddressSyncTask)
            return ErpSyncLevel.ShipToAddress;
        if (syncTaskType == ErpDataSchedulerDefaults.ErpSpecialPriceSyncTask)
            return ErpSyncLevel.SpecialPrice;
        if (syncTaskType == ErpDataSchedulerDefaults.ErpGroupPriceSyncTask)
            return ErpSyncLevel.GroupPrice;
        if (syncTaskType == ErpDataSchedulerDefaults.ErpInvoiceSyncTask)
            return ErpSyncLevel.Invoice;

        return 0;
    }

    #endregion

    #region Methods

    public async Task<SyncTaskSearchModel> PrepareTaskSearchModelAsync(SyncTaskSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        //prepare page parameters
        searchModel.SetGridPageSize();

        return searchModel;
    }

    public async Task<SyncTaskListModel> PrepareTaskListModelAsync(SyncTaskSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var tasks = (await _syncTaskService.GetAllTasksAsync()).ToPagedList(searchModel);

        return await new SyncTaskListModel().PrepareToGridAsync(searchModel, tasks, () =>
        {
            return tasks.SelectAwait(async task =>
            {
                var taskModel = task.ToModel<SyncTaskModel>();

                if (task.LastStartUtc.HasValue)
                {
                    taskModel.LastStartUtc = (await _dateTimeHelper
                        .ConvertToUserTimeAsync(task.LastStartUtc.Value, DateTimeKind.Utc)).ToString("G");
                }

                if (task.LastEndUtc.HasValue)
                {
                    taskModel.LastEndUtc = (await _dateTimeHelper
                        .ConvertToUserTimeAsync(task.LastEndUtc.Value, DateTimeKind.Utc)).ToString("G");
                }

                if (task.LastSuccessUtc.HasValue)
                {
                    taskModel.LastSuccessUtc = (await _dateTimeHelper
                        .ConvertToUserTimeAsync(task.LastSuccessUtc.Value, DateTimeKind.Utc)).ToString("G");
                }

                taskModel.IsRunning = await _qrtzFiredTriggersService
                    .CheckJobIsRunningAsync(task.QuartzJobName);

                return taskModel;
            });
        });
    }

    public async Task<SyncTaskModel> PrepareTaskModelByIdAsync(int taskId)
    {
        var task = await _syncTaskService.GetTaskByIdAsync(taskId);

        if (task is null)
        {
            return new SyncTaskModel();
        }

        var model = task.ToModel<SyncTaskModel>();
        
        var dayOfWeekSlots = new List<SyncTaskDaySlotModel>();

        try
        {
            dayOfWeekSlots = task.DayTimeSlots != null
                ? JsonConvert.DeserializeObject<List<SyncTaskDaySlotModel>>(task.DayTimeSlots) ?? new List<SyncTaskDaySlotModel>()
                : new List<SyncTaskDaySlotModel>();
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, await GetErpSyncLevelBySyncTaskType(task.Type), ex.Message, ex.StackTrace);
            dayOfWeekSlots = new List<SyncTaskDaySlotModel>();
        }

        #region DayOfWeek Slots arrangements

        var timeSlotModel = new Dictionary<int, List<SyncTaskTimeSlotModel>>();

        for (var dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
        {
            timeSlotModel[dayOfWeek] = new List<SyncTaskTimeSlotModel>();
        }

        if (!dayOfWeekSlots.IsNullOrEmpty())
        {
            foreach (var daySlot in dayOfWeekSlots)
            {
                foreach (var timeSlot in daySlot.TimeSlots)
                {
                    timeSlotModel[daySlot.DayOfWeek].Add(new SyncTaskTimeSlotModel
                    {
                        TimeSlot = timeSlot.TimeSlot,
                        IsSelected = true
                    });
                }
            }
        }

        var daySlotModel = new List<SyncTaskDaySlotModel>();
        for (var dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
        {
            daySlotModel.Add(new SyncTaskDaySlotModel
            {
                DayOfWeek = dayOfWeek,
                TimeSlots = timeSlotModel[dayOfWeek],
                IsSelected = timeSlotModel[dayOfWeek].Count > 0
            });
        }

        model.DayOfWeekSlots = daySlotModel;

        #endregion

        model.SyncLogSearchModel.SyncTaskName = task.Name;
        model.SyncLogSearchModel.SyncTaskId = task.Id;

        return model;
    }

    public async Task<SyncTaskModel> PrepareTaskModelByIdAsync(string quartzJobName)
    {
        var task = await _syncTaskService.GetTaskByQuartzJobNameAsync(quartzJobName);

        if (task is null)
        {
            return new SyncTaskModel();
        }

        var model = task.ToModel<SyncTaskModel>();

        var dayOfWeekSlots = new List<SyncTaskDaySlotModel>();

        try
        {
            dayOfWeekSlots = task.DayTimeSlots != null
                ? JsonConvert.DeserializeObject<List<SyncTaskDaySlotModel>>(task.DayTimeSlots) ?? new List<SyncTaskDaySlotModel>()
                : new List<SyncTaskDaySlotModel>();
        }
        catch (Exception ex)
        {
            await _erpLogsService.InsertErpLogAsync(ErpLogLevel.Error, await GetErpSyncLevelBySyncTaskType(task.Type), ex.Message, ex.StackTrace);
            dayOfWeekSlots = new List<SyncTaskDaySlotModel>();
        }

        #region DayOfWeek Slots arrangements

        var timeSlotModel = new Dictionary<int, List<SyncTaskTimeSlotModel>>();

        for (var dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
        {
            timeSlotModel[dayOfWeek] = new List<SyncTaskTimeSlotModel>();
        }

        if (!dayOfWeekSlots.IsNullOrEmpty())
        {
            foreach (var daySlot in dayOfWeekSlots)
            {
                foreach (var timeSlot in daySlot.TimeSlots)
                {
                    timeSlotModel[daySlot.DayOfWeek].Add(new SyncTaskTimeSlotModel
                    {
                        TimeSlot = timeSlot.TimeSlot,
                        IsSelected = true
                    });
                }
            }
        }

        var daySlotModel = new List<SyncTaskDaySlotModel>();
        for (var dayOfWeek = 0; dayOfWeek < 7; dayOfWeek++)
        {
            daySlotModel.Add(new SyncTaskDaySlotModel
            {
                DayOfWeek = dayOfWeek,
                TimeSlots = timeSlotModel[dayOfWeek],
                IsSelected = timeSlotModel[dayOfWeek].Count > 0
            });
        }

        model.DayOfWeekSlots = daySlotModel;

        #endregion

        model.SyncLogSearchModel.SyncTaskName = task.Name;
        model.SyncLogSearchModel.SyncTaskId = task.Id;

        return model;
    }

    public async Task<SyncLogListModel> PrepareSyncLogListModelAsync(SyncLogSearchModel searchModel)
    {
        ArgumentNullException.ThrowIfNull(searchModel);

        var syncLogFiles = (await _erpSyncLogService.GetAllSyncLogFiles(searchModel.SyncTaskName, searchModel.SyncTaskId)).ToPagedList(searchModel);

        return await new SyncLogListModel().PrepareToGridAsync(searchModel, syncLogFiles, () =>
        {
            return syncLogFiles.SelectAwait(async file => new SyncLogModel
            {
                Name = _fileProvider.GetFileName(file),

                Length = $"{_fileProvider.FileLength(file) / 1024f:F2} Kb",

                TaskId = searchModel.SyncTaskId

                //Link = $"{_webHelper.GetStoreLocation()}{ErpDataSchedulerDefaults.SyncLogFileSaveDefaultPath}/{_fileProvider.GetFileName(file)}"
            });
        });
    }

    public async Task<QuartzJobListModel> PrepareJobListPagedModelAsync(SyncTaskSearchModel searchModel)
    {
        ArgumentException.ThrowIfNullOrEmpty(nameof(searchModel));

        var scheduleJobs = await _quartzJobDetailService.GetPagedListAsync(searchModel.Page - 1, searchModel.PageSize);

        return await new QuartzJobListModel().PrepareToGridAsync(searchModel, scheduleJobs, () =>
        {
            return scheduleJobs.SelectAwait(async job => new QuartzJobDetailModel
            {
                JOB_NAME = job.JOB_NAME,
                DESCRIPTION = job.DESCRIPTION,
                JOB_GROUP = job.JOB_GROUP,
            });
        });
    }

    #endregion
}